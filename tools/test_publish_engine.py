#!/usr/bin/env python3
"""publish_engine.py on synthetic publishes (docs/platform-portability.md, "Fewer files in a package"): no dotnet runs."""
from __future__ import annotations

import json
import os
import subprocess
import tempfile
import unittest
from pathlib import Path

import publish_engine

COMMIT = "0123456789abcdef0123456789abcdef01234567"


def write_publish(directory: Path, runtime: str, *extra: str, tool: bool = False) -> Path:
    files = publish_engine.tool_files(runtime) if tool else publish_engine.publish_files(runtime)
    for name in (*files, *extra):
        path = directory / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(f"fixture:{name}".encode())
    return directory


class FakeRun:
    """Stands in for subprocess.run: answers git with `commit`, and for `dotnet publish` records the command and writes
    the publish's files and the kept dependency list: the library's, or the tool's for its project. `extra` goes into
    the library's publish, `tool_extra` into the tool's; `tool_returncode` is the tool publish's exit code."""

    def __init__(self, *extra: str, commit: str = COMMIT, returncode: int = 0, keep_list: bool = True,
                 git_error: str = "", tool_extra: tuple[str, ...] = (), tool_returncode: int = 0):
        self.extra, self.commit, self.returncode, self.keep_list = extra, commit, returncode, keep_list
        self.git_error, self.tool_extra, self.tool_returncode = git_error, tool_extra, tool_returncode
        self.publishes: list[list[str]] = []
        self.environments: list[dict[str, str]] = []

    def __call__(self, command, cwd, check, env=None, capture_output=False, text=False):
        if command[0] == "git":
            if self.git_error:
                return subprocess.CompletedProcess(command, 128, stdout="", stderr=self.git_error + "\n")
            answer = self.commit if command[1] == "rev-parse" else ""
            return subprocess.CompletedProcess(command, 0, stdout=answer + "\n", stderr="")
        self.publishes.append(command)
        self.environments.append(env)
        tool = command[2] == str(publish_engine.TOOL_PROJECT)
        returncode = self.tool_returncode if tool else self.returncode
        if returncode == 0:
            runtime = command[command.index("--runtime") + 1]
            write_publish(Path(command[command.index("--output") + 1]), runtime,
                          *(self.tool_extra if tool else self.extra), tool=tool)
            if self.keep_list:
                listed = next(part for part in command if part.startswith("-p:OmeDependencyList="))
                Path(listed.split("=", 1)[1]).write_text("{}", encoding="utf-8")
        return subprocess.CompletedProcess(command, returncode)


class PublishEngineTests(unittest.TestCase):
    def test_a_git_failure_carries_gits_message(self) -> None:
        # A container's root user does not own a checkout the runner made, and git refuses it (release-build.yml).
        refusal = "fatal: detected dubious ownership in repository at '/__w/OpenMaidEngine/OpenMaidEngine'"
        with self.assertRaisesRegex(RuntimeError, "git rev-parse HEAD failed with exit code 128: fatal: detected dubious"):
            publish_engine.source_state(FakeRun(git_error=refusal))

    def test_the_publish_is_the_projects_nativeaot_form_with_one_node(self) -> None:
        root = Path("/opt/build/engine-publish")
        for runtime, platform, library in (("win-x64", "windows", "OME.dll"), ("linux-x64", "linuxbsd", "OME.so")):
            with self.subTest(runtime):
                command = publish_engine.publish_command("dotnet", runtime, root)
                self.assertEqual(["dotnet", "publish", str(publish_engine.PROJECT)], command[:3])
                self.assertEqual(runtime, command[command.index("--runtime") + 1])
                self.assertEqual("ExportRelease", command[command.index("--configuration") + 1])
                self.assertEqual(str(root / runtime), command[command.index("--output") + 1])
                for flag in ("-p:PublishAot=true", f"-p:GodotTargetPlatform={platform}", "--disable-build-servers",
                             "-nodeReuse:false", "-m:1", "-p:DebugType=None", "-p:DebugSymbols=false"):
                    self.assertIn(flag, command)
                self.assertIn(f"-p:OmeDependencyList={root / f'{runtime}.deps.json'}", command)
                self.assertEqual(library, publish_engine.publish_files(runtime)[0])
        # gcc links the Linux library: the manylinux image has no clang.
        self.assertIn("-p:CppCompilerAndLinker=gcc", publish_engine.publish_command("dotnet", "linux-x64", root))
        self.assertNotIn("-p:CppCompilerAndLinker=gcc", publish_engine.publish_command("dotnet", "win-x64", root))

    def test_the_tools_publish_is_nativeaot_with_no_symbols(self) -> None:
        root = Path("/opt/build/engine-publish")
        for runtime in ("win-x64", "linux-x64"):
            with self.subTest(runtime):
                command = publish_engine.tool_publish_command("dotnet", runtime, root)
                self.assertEqual(["dotnet", "publish", str(publish_engine.TOOL_PROJECT)], command[:3])
                self.assertEqual("Release", command[command.index("--configuration") + 1])
                self.assertEqual(str(root / f"tool-{runtime}"), command[command.index("--output") + 1])
                for flag in ("-p:PublishAot=true", "--disable-build-servers", "-nodeReuse:false", "-m:1",
                             "-p:DebugType=None", "-p:DebugSymbols=false",
                             "-p:CopyOutputSymbolsToPublishDirectory=false"):
                    self.assertIn(flag, command)
                self.assertIn(f"-p:OmeDependencyList={root / f'tool-{runtime}.deps.json'}", command)
        self.assertIn("-p:CppCompilerAndLinker=gcc", publish_engine.tool_publish_command("dotnet", "linux-x64", root))
        self.assertEqual(("OME-Tool.exe", "OME-Tool-notices/Microsoft.NETCore.App.Runtime.win-x64-LICENSE.TXT",
                          "OME-Tool-notices/Microsoft.NETCore.App.Runtime.win-x64-THIRD-PARTY-NOTICES.TXT"),
                         publish_engine.tool_files("win-x64"))
        self.assertEqual("OME-Tool", publish_engine.tool_files("linux-x64")[0])

    def test_verify_refuses_the_tools_symbols_and_missing_files(self) -> None:
        for extra in ("OME-Tool.pdb", "OME-Tool.dbg", "OME-Tool.deps.json", "Ome.Commands.dll"):
            with self.subTest(extra), tempfile.TemporaryDirectory() as temporary:
                directory = write_publish(Path(temporary), "win-x64", extra, tool=True)
                with self.assertRaisesRegex(ValueError, f"the tool does not ship: {extra}$"):
                    publish_engine.verify_tool(directory, "win-x64")
        with tempfile.TemporaryDirectory() as temporary:
            directory = write_publish(Path(temporary), "linux-x64", tool=True)
            (directory / "OME-Tool").unlink()
            with self.assertRaisesRegex(ValueError, "the tool publish is incomplete; missing: OME-Tool in"):
                publish_engine.verify_tool(directory, "linux-x64")

    def test_the_tool_is_placed_beside_the_engine(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            publish_engine.publish("win-x64", output_root=root / "publish", run=FakeRun(), platform="win32")
            export = root / "export"
            export.mkdir()
            with self.assertRaisesRegex(ValueError, "the export has no OME.exe to put the tool beside"):
                publish_engine.place_tool("win-x64", export, root / "publish")
            (export / "OME.exe").write_bytes(b"engine")
            copies = publish_engine.place_tool("win-x64", export, root / "publish")
            self.assertEqual([export.resolve() / name for name in publish_engine.tool_files("win-x64")], copies)
            self.assertEqual(b"fixture:OME-Tool.exe", (export / "OME-Tool.exe").read_bytes())

    @unittest.skipUnless(os.name == "posix", "file modes are POSIX's")
    def test_the_placed_linux_tool_is_executable(self) -> None:
        # A workflow artifact keeps no file modes, so the tool reaches the package job unexecutable.
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            tool = write_publish(publish_engine.tool_directory("linux-x64", root), "linux-x64", tool=True)
            (tool / "OME-Tool").chmod(0o644)
            export = root / "export"
            export.mkdir()
            (export / "OME").write_bytes(b"engine")
            publish_engine.place_tool("linux-x64", export, root)
            self.assertEqual(0o755, (export / "OME-Tool").stat().st_mode & 0o777)

    def test_the_publish_holds_the_library_and_the_runtimes_notices(self) -> None:
        self.assertEqual(("OME.so", "Microsoft.NETCore.App.Runtime.linux-x64-LICENSE.TXT",
                          "Microsoft.NETCore.App.Runtime.linux-x64-THIRD-PARTY-NOTICES.TXT"),
                         publish_engine.publish_files("linux-x64"))
        with tempfile.TemporaryDirectory() as temporary:
            publish_engine.verify_publish(write_publish(Path(temporary), "win-x64"), "win-x64")

    def test_verify_refuses_a_runtime_assemblies_symbols_and_missing_files(self) -> None:
        # A JIT runtime file would send Godot down its hostfxr route; the library holds the assemblies.
        for extra in ("hostfxr.dll", "coreclr.dll", "System.Private.CoreLib.dll", "Ome.Engine.dll", "OME.pdb",
                      "OME.deps.json", "OME.runtimeconfig.json", "avcodec-62.dll"):
            with self.subTest(extra), tempfile.TemporaryDirectory() as temporary:
                directory = write_publish(Path(temporary), "win-x64", extra)
                with self.assertRaisesRegex(ValueError, f"does not ship: {extra}$"):
                    publish_engine.verify_publish(directory, "win-x64")
        with tempfile.TemporaryDirectory() as temporary:
            directory = write_publish(Path(temporary), "linux-x64")
            (directory / "OME.so").unlink()
            with self.assertRaisesRegex(ValueError, "incomplete; missing: OME.so"):
                publish_engine.verify_publish(directory, "linux-x64")

    def test_it_compiles_only_on_its_own_system(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            run = FakeRun()
            with self.assertRaisesRegex(ValueError, "compiles linux-x64 on its own system only; this is win32"):
                publish_engine.publish("linux-x64", output_root=Path(temporary), run=run, platform="win32")
            with self.assertRaisesRegex(ValueError, "compiles win-x64 on its own system only; this is linux"):
                publish_engine.publish("win-x64", output_root=Path(temporary), run=run, platform="linux")
            self.assertEqual([], run.publishes)

    def test_a_publish_is_stamped_with_its_commit_and_checked_against_the_checkout(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            run = FakeRun()
            directory = publish_engine.publish("linux-x64", output_root=root, run=run, platform="linux")
            self.assertEqual(root / "linux-x64", directory)
            # The library, then the tool: two publishes, one after the other, with the low-memory flags.
            self.assertEqual([str(publish_engine.PROJECT), str(publish_engine.TOOL_PROJECT)],
                             [command[2] for command in run.publishes])
            self.assertEqual(["9", "9"], [environment["DOTNET_GCConserveMemory"] for environment in run.environments])
            stamp = json.loads((root / "linux-x64.json").read_text(encoding="utf-8"))
            self.assertEqual({"runtime": "linux-x64", "commit": COMMIT, "dirty": False}, stamp)
            self.assertEqual(stamp, publish_engine.check_stamp("linux-x64", root, run))
            # A library built from another commit is refused, so a release never packs a stale engine.
            with self.assertRaisesRegex(ValueError, f"built from {COMMIT}, not this checkout's fedcba"):
                publish_engine.check_stamp("linux-x64", root, FakeRun(commit="fedcba"))
            (root / "tool-linux-x64/OME-Tool").unlink()
            with self.assertRaisesRegex(ValueError, "the tool publish is incomplete; missing: OME-Tool"):
                publish_engine.check_stamp("linux-x64", root, run)
            (root / "tool-linux-x64.deps.json").unlink()
            with self.assertRaisesRegex(ValueError, "the tool publish kept no dependency list"):
                publish_engine.check_stamp("linux-x64", root, run)
            (root / "linux-x64.deps.json").unlink()
            with self.assertRaisesRegex(ValueError, "kept no dependency list"):
                publish_engine.check_stamp("linux-x64", root, run)
            with self.assertRaisesRegex(ValueError, "no engine publish for win-x64: run tools/publish_engine.py"):
                publish_engine.check_stamp("win-x64", root, run)

    def test_a_failed_publish_a_lost_list_or_a_wrong_output_stamps_nothing(self) -> None:
        cases = (
            (FakeRun(returncode=1), RuntimeError, "failed with exit code 1"),
            (FakeRun(keep_list=False), ValueError, "kept no dependency list"),
            (FakeRun("libcoreclr.so"), ValueError, "does not ship: libcoreclr.so"),
            (FakeRun(tool_returncode=1), RuntimeError, "the tool publish for linux-x64 failed with exit code 1"),
            (FakeRun(tool_extra=("OME-Tool.dbg",)), ValueError, "the tool does not ship: OME-Tool.dbg"),
        )
        for run, error, message in cases:
            with self.subTest(message), tempfile.TemporaryDirectory() as temporary:
                root = Path(temporary)
                with self.assertRaisesRegex(error, message):
                    publish_engine.publish("linux-x64", output_root=root, run=run, platform="linux")
                self.assertFalse((root / "linux-x64.json").exists())

    def test_a_second_publish_starts_from_an_empty_folder(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            publish_engine.publish("win-x64", output_root=root, run=FakeRun(), platform="win32")
            (root / "win-x64/stale.dll").write_bytes(b"stale")
            (root / "tool-win-x64/stale.pdb").write_bytes(b"stale")
            publish_engine.publish("win-x64", output_root=root, run=FakeRun(), platform="win32")
            self.assertFalse((root / "win-x64/stale.dll").exists())
            self.assertFalse((root / "tool-win-x64/stale.pdb").exists())


if __name__ == "__main__":
    unittest.main()
