#!/usr/bin/env python3
"""Dump one profile's unpacked AGE engine image from a live process for native RE.

The installed executable is packed; native opcode handlers become readable only after AGE has
unpacked itself in memory. Launch the selected game to its title screen, then attach by PID. Common
``--game``/``--game-root`` options are consumed by ``tools/paths.py`` before this parser runs.

Output is profile-scoped and provenance-bearing:
``build/games/<game>/engine-dump/{manifest.json,range_<base>.bin}``. A reference-only install (no port profile;
``paths.REFERENCE_INSTALLS``) is dumped with ``--reference ID`` into ``build/engines/<id>/engine-dump/``.

Usage:
  py -3.11 -u -X utf8 tools/frida/dump_engine.py --game kamidori --game-root PATH PID
  py -3.11 -u -X utf8 tools/frida/dump_engine.py --reference ID PID
  py -3.11 -u -X utf8 tools/frida/dump_engine.py --reference ID --spawn

``--spawn`` starts the native_scratch.py copy itself instead of attaching: SHGetFolderPathA (save root, SYS4REG.INI,
screenshots) points into a disposable ``dump-sandbox/`` beside the copy, so the real save folders are never touched.
After ``--wait`` seconds (AGE has unpacked itself by then) it dumps and ends the process.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import struct
import sys
import time
from datetime import datetime, timezone
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import paths

OUTDIR = paths.BUILD / "engine-dump"   # main() points it at build/engines/<id>/engine-dump for --reference
CHUNK = 2 * 1024 * 1024

# A landmark is only a convenience check after its RVA has been proven for that exact executable.
# Absence is the correct default for a new profile; capture itself never depends on a landmark.
PROFILE_LANDMARK_RVAS = {
    "himegari": 0x74F1F,
}

JS = r"""
const CHUNK = %d;
const LANDMARK_RVA = %d;

function ranges(prot){ return Process.enumerateRanges(prot).map(r => ({
    base: r.base.toString(), size: r.size, prot: r.protection,
    file: r.file ? r.file.path : null })); }

function moduleRows(){ return Process.enumerateModules().map(m => ({
    name:m.name, path:m.path, base:m.base.toString(), size:m.size })); }

function isInsideModule(range, modules){
    const start = ptr(range.base);
    const end = start.add(range.size);
    return modules.some(m => start.compare(ptr(m.base)) >= 0
        && end.compare(ptr(m.base).add(m.size)) <= 0);
}

function dump(baseStr, size, tag){
    const base = ptr(baseStr);
    for (let off = 0; off < size; off += CHUNK){
        const n = Math.min(CHUNK, size - off);
        let buf;
        try { buf = base.add(off).readByteArray(n); }
        catch(e){ send({kind:'gap', base:baseStr, off:off, n:n, tag:tag, err:''+e}); continue; }
        if (buf === null){
            send({kind:'gap', base:baseStr, off:off, n:n, tag:tag, err:'null'});
            continue;
        }
        send({kind:'chunk', base:baseStr, off:off, n:n, tag:tag}, buf);
    }
    send({kind:'done', base:baseStr, size:size, tag:tag});
}

const mod = Process.getModuleByName('AGE.EXE');
const rx = ranges('r-x');
const rw = ranges('rw-');
const modules = moduleRows();
let landmark = null;
if (LANDMARK_RVA >= 0 && LANDMARK_RVA + 16 <= mod.size){
    try { landmark = mod.base.add(LANDMARK_RVA).readByteArray(16); } catch(e){}
}

send({kind:'manifest', age_base:mod.base.toString(), age_size:mod.size,
      age_path:mod.path, landmark_rva:LANDMARK_RVA >= 0 ? LANDMARK_RVA : null,
      rx:rx, rw:rw, modules:modules}, landmark);

// Dump the complete AGE module plus large anonymous executable ranges. The latter are retained as
// evidence because packers/revisions can move generated or unpacked code outside the module.
dump(mod.base.toString(), mod.size, 'age-module');
for (const r of rx){
    if (isInsideModule(r, modules)) continue;
    if (r.file) continue;
    if (r.size >= 1*1024*1024) dump(r.base, r.size, 'rx-anonymous');
}
send({kind:'alldone'});
"""


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def read_catalog_identity(path: Path) -> dict[str, str]:
    header = path.read_bytes()[:264]
    if len(header) < 264:
        raise ValueError(f"{path}: SYS4INI header is truncated")

    def cstring(data: bytes) -> str:
        return data.split(b"\0", 1)[0].decode("cp932", errors="replace").rstrip(" ")

    return {
        "catalog_revision": cstring(header[:8]),
        "title": cstring(header[8:264]),
    }


def profile_provenance() -> dict[str, object]:
    manifest_path = paths.PROFILE_MANIFEST_DIR / f"{paths.CONTEXT.profile_id}.json"
    profile = json.loads(manifest_path.read_text(encoding="utf-8"))
    executable = paths.AGE_EXE
    if not executable.is_file():
        raise ValueError(f"selected profile executable does not exist: {executable}")
    identity = read_catalog_identity(paths.GAME_DIR / "SYS4INI.BIN")
    return {
        "profile_id": paths.CONTEXT.profile_id,
        "sys_frontend_id": profile["sysFrontendId"],
        "engine_abi_id": profile["engineAbiId"],
        **identity,
        "game_root": str(paths.GAME_DIR),
        "packed_executable": {
            "name": executable.name,
            "size": executable.stat().st_size,
            "sha256": sha256_file(executable),
        },
    }


# The folders native AGE asks SHGetFolderPathA for (CSIDL & 0xff), as tools/drive_core.py SANDBOX_FOLDERS.
SPAWN_FOLDERS = {0x1C: "LocalAppData", 0x1A: "AppData", 0x10: "Desktop", 0x05: "Documents", 0x23: "ProgramData"}

SPAWN_JS = r"""
const folders = %s;
function hookShell32(module){
    const target = module.findExportByName('SHGetFolderPathA');
    if (target === null) return;
    Interceptor.attach(target, {
        onEnter(args){ this.csidl = args[1].toInt32(); this.out = args[4]; },
        onLeave(result){
            const folder = folders[String(this.csidl & 0xff)];
            if (folder === undefined || this.out.isNull()) return;
            this.out.writeUtf8String(folder);
            result.replace(ptr(0));
        },
    });
    send({kind:'hooked', module:module.name});
}
let hooked = false;
Process.attachModuleObserver({ onAdded(module){
    if (!hooked && module.name.toLowerCase() === 'shell32.dll'){ hooked = true; hookShell32(module); }
}});
"""


def spawn_sandboxed(exe: Path, sandbox: Path):
    """Start exe suspended, redirect its shell folders into sandbox, resume. Returns (device, pid, session)."""
    from frida_runtime import frida

    folders = {}
    for csidl, name in SPAWN_FOLDERS.items():
        folder = sandbox / name
        folder.mkdir(parents=True, exist_ok=True)
        if not str(folder).isascii() or len(str(folder)) > 200:
            raise ValueError(f"sandbox path must be ASCII and short enough for AGE's buffers: {folder}")
        folders[str(csidl)] = str(folder)
    device = frida.get_local_device()
    pid = device.spawn([str(exe)], cwd=str(exe.parent))
    session = device.attach(pid)
    script = session.create_script(SPAWN_JS % json.dumps(folders))
    script.on("message", lambda message, data: print("[spawn]", message.get("payload", message)))
    script.load()
    device.resume(pid)
    print(f"[spawn] {exe} pid={pid}; shell folders -> {sandbox}")
    return device, pid, session


def reference_provenance(reference_id: str) -> dict[str, object]:
    """Provenance of a reference-only install: no profile, so no frontend or ABI id."""
    install = paths.reference_install(reference_id)
    executable = install / "AGE.EXE"
    if not executable.is_file():
        raise ValueError(f"reference install executable does not exist: {executable}")
    return {
        "reference_id": reference_id,
        **read_catalog_identity(install / "SYS4INI.BIN"),
        "game_root": str(install),
        "packed_executable": {
            "name": executable.name,
            "size": executable.stat().st_size,
            "sha256": sha256_file(executable),
        },
    }


def pe_base_relocations(pe: bytes) -> tuple[int, list[int]]:
    """A PE32 executable's preferred image base and the RVAs of its HIGHLOW base relocations, which a packed AGE.EXE
    has none of. 天秤のLa DEA。's 4.45D is not packed and keeps its relocations, so Windows loads it elsewhere."""
    pe_at = struct.unpack_from("<I", pe, 0x3C)[0]
    if pe[pe_at:pe_at + 4] != b"PE\0\0":
        raise ValueError("not a PE executable")
    sections = struct.unpack_from("<H", pe, pe_at + 6)[0]
    optional_size = struct.unpack_from("<H", pe, pe_at + 20)[0]
    optional = pe_at + 24
    if struct.unpack_from("<H", pe, optional)[0] != 0x10B:
        raise ValueError("not a PE32 executable")
    image_base = struct.unpack_from("<I", pe, optional + 28)[0]
    if struct.unpack_from("<I", pe, optional + 92)[0] <= 5:
        return image_base, []
    reloc_rva, reloc_size = struct.unpack_from("<II", pe, optional + 96 + 5 * 8)
    if not reloc_rva or not reloc_size:
        return image_base, []
    table = optional + optional_size

    def file_offset(rva: int) -> int:
        for index in range(sections):
            virtual_size, address, raw_size, raw = struct.unpack_from("<IIII", pe, table + 40 * index + 8)
            if address <= rva < address + max(virtual_size, raw_size):
                return raw + rva - address
        raise ValueError(f"RVA 0x{rva:x} is in no section")

    sites: list[int] = []
    at = file_offset(reloc_rva)
    end = at + reloc_size
    while at + 8 <= end:
        page, block = struct.unpack_from("<II", pe, at)
        if block < 8:
            break
        for index in range((block - 8) // 2):
            entry = struct.unpack_from("<H", pe, at + 8 + 2 * index)[0]
            kind = entry >> 12
            if kind == 3:                                    # IMAGE_REL_BASED_HIGHLOW
                sites.append(page + (entry & 0xFFF))
            elif kind != 0:                                  # 0 pads a block
                raise ValueError(f"base relocation type {kind} at page 0x{page:x}")
        at += block
    return image_base, sites


def undo_relocations(image: bytearray, sites: list[int], delta: int) -> int:
    """Take the loader's delta back off every relocated dword the image holds; returns how many."""
    undone = 0
    for rva in sites:
        if rva + 4 <= len(image):
            value = struct.unpack_from("<I", image, rva)[0]
            struct.pack_into("<I", image, rva, (value - delta) & 0xFFFFFFFF)
            undone += 1
    return undone


def rebase_module(outdir: Path, entry: dict[str, object], executable: Path) -> None:
    """Write a module Windows relocated at its preferred base, as range_<preferred>.bin, so every tool reads the
    image as it reads the packed builds' (operand_access.py's IMAGE_BASE, the Ghidra images at 0x400000). The
    manifest's runtime_module keeps the base it ran at; the entry keeps it as rebased_from."""
    loaded = int(str(entry["base"]), 16)
    preferred, sites = pe_base_relocations(executable.read_bytes())
    if loaded == preferred or not sites:
        return
    old = outdir / str(entry["file"])
    image = bytearray(old.read_bytes())
    undone = undo_relocations(image, sites, loaded - preferred)
    entry["file"] = f"range_{preferred:08x}.bin"
    (outdir / str(entry["file"])).write_bytes(image)
    old.unlink()
    entry["rebased_from"] = str(entry["base"])
    entry["base"] = f"0x{preferred:x}"
    entry["relocations_undone"] = undone
    print(f"[frida] rebased age-module 0x{loaded:x} -> 0x{preferred:x} ({undone} relocations undone)")


def build_manifest(
    provenance: dict[str, object],
    runtime: dict[str, object],
    dumped_ranges: list[dict[str, object]],
    target: str,
    captured_at_utc: str,
) -> dict[str, object]:
    return {
        "schema_version": 2,
        "capture_tool": "tools/frida/dump_engine.py",
        "captured_at_utc": captured_at_utc,
        "attach_target": target,
        "profile": provenance,
        "runtime_module": {
            "path": runtime.get("age_path"),
            "image_base": runtime.get("age_base"),
            "size": runtime.get("age_size"),
            "landmark_rva": runtime.get("landmark_rva"),
            "landmark_bytes": runtime.get("landmark_bytes"),
        },
        "dumped_ranges": dumped_ranges,
        "loaded_modules": runtime.get("modules", []),
        "executable_ranges": runtime.get("rx", []),
        "writable_ranges": runtime.get("rw", []),
    }


def capture(proc: str, *, overwrite: bool = False, reference: str | None = None) -> int:
    from frida_runtime import frida

    provenance = reference_provenance(reference) if reference else profile_provenance()
    OUTDIR.mkdir(parents=True, exist_ok=True)
    existing = [OUTDIR / "manifest.json", *OUTDIR.glob("range_*.bin")]
    existing = [path for path in existing if path.exists()]
    if existing and not overwrite:
        print(f"[frida] refusing to overwrite {OUTDIR}; use --overwrite for a deliberate recapture")
        return 2
    if overwrite:
        for path in existing:
            path.unlink()

    files: dict[str, object] = {}
    range_records: dict[str, dict[str, object]] = {}
    runtime: dict[str, object] = {}
    finished = {"flag": False}
    captured_at = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")

    def range_file(base: str):
        if base not in files:
            files[base] = (OUTDIR / f"range_{int(base, 16):08x}.bin").open("wb")
        return files[base]

    def record(base: str, tag: str) -> dict[str, object]:
        return range_records.setdefault(base, {
            "base": base,
            "tag": tag,
            "file": f"range_{int(base, 16):08x}.bin",
            "declared_size": None,
            "captured_bytes": 0,
            "gaps": [],
        })

    def on_message(message, data):
        if message.get("type") == "error":
            print("[frida-error]", message.get("description"))
            return
        if message.get("type") != "send":
            return
        payload = message["payload"]
        kind = payload.get("kind")
        if kind == "manifest":
            runtime.update(payload)
            runtime.pop("kind", None)
            runtime["landmark_bytes"] = data.hex() if data else None
            landmark = runtime["landmark_bytes"] or "not configured"
            print(f"[frida] AGE.EXE base={payload['age_base']} size=0x{payload['age_size']:x}")
            identity = (f"reference={provenance['reference_id']}" if "reference_id" in provenance
                        else f"profile={provenance['profile_id']} abi={provenance['engine_abi_id']}")
            print(f"[frida] {identity} landmark={landmark}")
            print(f"[frida] r-x ranges: {len(payload['rx'])}  rw- ranges: {len(payload['rw'])}")
        elif kind == "chunk":
            stream = range_file(payload["base"])
            stream.seek(payload["off"])
            stream.write(data)
            record(payload["base"], payload["tag"])["captured_bytes"] += payload["n"]
        elif kind == "gap":
            entry = record(payload["base"], payload["tag"])
            entry["gaps"].append({
                "offset": payload["off"], "size": payload["n"], "error": payload["err"],
            })
            print(f"    [gap] {payload['base']}+0x{payload['off']:x} "
                  f"n=0x{payload['n']:x} {payload['err']}")
        elif kind == "done":
            entry = record(payload["base"], payload["tag"])
            entry["declared_size"] = payload["size"]
            stream = range_file(payload["base"])
            stream.truncate(payload["size"])
            print(f"[frida] dumped {payload['tag']} {payload['base']} (0x{payload['size']:x} bytes)")
        elif kind == "alldone":
            finished["flag"] = True

    target: int | str = int(proc) if proc.isdigit() else proc
    try:
        session = frida.attach(target)
    except (frida.ProcessNotFoundError, frida.ServerNotRunningError):
        processes = frida.get_local_device().enumerate_processes()
        hits = [(process.pid, process.name) for process in processes if "age" in process.name.lower()]
        print("[frida] AGE.EXE not found. Running AGE-like processes:", hits or "(none)")
        print("        Launch the selected game to the title screen, then attach by PID.")
        return 2

    landmark_rva = -1 if reference else PROFILE_LANDMARK_RVAS.get(paths.CONTEXT.profile_id, -1)
    script = session.create_script(JS % (CHUNK, landmark_rva))
    script.on("message", on_message)
    script.load()
    print(f"[frida] attached to {proc}; dumping engine code -> {OUTDIR}")
    try:
        for _ in range(600):
            if finished["flag"]:
                break
            time.sleep(0.1)
    except KeyboardInterrupt:
        pass
    finally:
        for stream in files.values():
            stream.close()
        session.detach()

    module = next((entry for entry in range_records.values() if entry["tag"] == "age-module"), None)
    if module is not None and runtime.get("age_path"):
        rebase_module(OUTDIR, module, Path(str(runtime["age_path"])))
    dumped_ranges = []
    for base, entry in sorted(range_records.items(), key=lambda item: int(str(item[1]["base"]), 16)):
        dump_path = OUTDIR / str(entry["file"])
        entry["file_size"] = dump_path.stat().st_size
        entry["sha256"] = sha256_file(dump_path)
        dumped_ranges.append(entry)
    if runtime:
        manifest = build_manifest(provenance, runtime, dumped_ranges, str(proc), captured_at)
        (OUTDIR / "manifest.json").write_text(
            json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(f"\n[frida] wrote {len(dumped_ranges)} range file(s) + manifest.json to {OUTDIR}")
    if any(entry["gaps"] for entry in dumped_ranges):
        print("[frida] capture contains unreadable gaps; inspect manifest.json before import")
        return 1
    return 0 if finished["flag"] else 1


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("target", nargs="?", default="AGE.EXE", help="live AGE process PID or name")
    parser.add_argument(
        "--overwrite", action="store_true",
        help="replace an existing dump in the selected profile's disposable build directory",
    )
    parser.add_argument(
        "--reference", choices=sorted(paths.REFERENCE_INSTALLS),
        help="dump a reference-only install (paths.REFERENCE_INSTALLS) into build/engines/<id>/engine-dump",
    )
    parser.add_argument("--spawn", action="store_true",
                        help="start the native_scratch.py copy with sandboxed shell folders instead of attaching")
    parser.add_argument("--wait", type=float, default=20.0, help="seconds between --spawn and the dump")
    arguments = parser.parse_args()
    global OUTDIR
    build_root = paths.ENGINES_BUILD / arguments.reference if arguments.reference else paths.BUILD
    OUTDIR = build_root / "engine-dump"
    try:
        if not arguments.spawn:
            return capture(arguments.target, overwrite=arguments.overwrite, reference=arguments.reference)
        exe = build_root / "native-scratch" / "AGE.EXE"
        if not exe.is_file():
            parser.error(f"no {exe}; build it with tools/native_scratch.py first")
        device, pid, session = spawn_sandboxed(exe, build_root / "dump-sandbox")
        try:
            time.sleep(arguments.wait)
            return capture(str(pid), overwrite=arguments.overwrite, reference=arguments.reference)
        finally:
            session.detach()
            device.kill(pid)
            print(f"[spawn] ended pid={pid}")
    except (OSError, ValueError, KeyError, json.JSONDecodeError) as error:
        parser.error(str(error))


if __name__ == "__main__":
    raise SystemExit(main())
