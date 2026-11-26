using System.Numerics;

namespace Ome.Engine.Model;

// AGE 4.43B's models (ops 0x330 to 0x33e): a third map beside the retained objects' and the polygons', keyed by the
// same handles (manager +0x43c), each record a mesh slot of op 0x329 with a colour, a blend mode and world matrices
// (docs/engine-re.md, "The models"). The port keeps what the ops 魔導巧殻's scripts run write; it does not draw them
// (docs/backlog.md, "The models are not drawn (AGE 4.43B)").
public sealed partial class GfxState
{
    /// <summary>A model record (native's 143 dwords; gfx_model_record_init@0x48cd50 gives the defaults).</summary>
    public sealed class GfxModel
    {
        /// <summary>Flags bit 0: op 0x330 has given the model a mesh slot.</summary>
        public bool Made;
        /// <summary>The mesh slot op 0x329 loaded (+0x4).</summary>
        public int MeshSlot;
        /// <summary>Op 0x33b's three floats (+0x8 to +0x10), which only the frame walk's model draw reads.</summary>
        public Vector3 Vector8;
        /// <summary>Op 0x332's blend mode (+0x14).</summary>
        public long BlendMode;
        /// <summary>Op 0x332's colour, 0xAARRGGBB (+0x44); opaque white until set.</summary>
        public uint Argb = 0xffffffff;
        /// <summary>+0x4c: a transform op has run since op 0x334 reset them.</summary>
        public bool TransformSet;
        /// <summary>Op 0x335's scaling (+0x50).</summary>
        public Matrix4x4 Scale = Matrix4x4.Identity;
        /// <summary>Op 0x337's translation (+0x150).</summary>
        public Matrix4x4 Translation = Matrix4x4.Identity;
        /// <summary>The product of op 0x33e's rotations, each multiplied on the right (+0x1fc).</summary>
        public Matrix4x4 AxisRotation = Matrix4x4.Identity;

        public GfxModel Clone() => (GfxModel)MemberwiseClone();
    }

    /// <summary>The model record of a handle, or null.</summary>
    public GfxModel? TryGetModel(long handle) => _scene.Models.GetValueOrDefault(handle);

    /// <summary>How many model records the scene holds.</summary>
    public int ModelCount => _scene.Models.Count;

    // gfx_model_get_or_create@0x499b70: every model op makes the handle's record when it has none.
    private GfxModel GetOrCreateModel(long handle)
    {
        if (_scene.Models.TryGetValue(handle, out GfxModel? model)) return model;
        model = new GfxModel();
        _scene.Models[handle] = model;
        return model;
    }

    /// <summary>Op 0x330 (gfx_model_set_mesh@0x49cfa0): the model draws the mesh in <paramref name="meshSlot"/>, which
    /// native does not check; the frame walk traces an error for a slot holding no mesh.</summary>
    public void SetModelMesh(long handle, int meshSlot)
    {
        GfxModel model = GetOrCreateModel(handle);
        model.Made = true;
        model.MeshSlot = meshSlot;
        MarkRetainedMutation();
    }

    /// <summary>Op 0x332 (gfx_model_set_color@0x49cff0 and its handler): the blend mode and the colour. An alpha of
    /// 0x100 or more is 0xff; a negative alpha or RGB takes the retained object's colour (+0x60,
    /// gfx_object_get_color@0x49c8f0), opaque white for a handle with no object, which the read does not create.
    /// </summary>
    public void SetModelColor(long handle, long blendMode, long alpha, long rgb)
    {
        long objectArgb = _scene.Objects.TryGetValue(handle, out GfxObject? o) ? o.Color & 0xffffffff : 0xffffffff;
        long resolvedAlpha = alpha >= 0x100 ? 0xff : alpha < 0 ? objectArgb >> 24 : alpha;
        long resolvedRgb = rgb < 0 ? objectArgb : rgb;
        GfxModel model = GetOrCreateModel(handle);
        model.BlendMode = blendMode;
        model.Argb = (uint)PackColor(resolvedAlpha, resolvedRgb & 0xffffff);
        MarkRetainedMutation();
    }

    /// <summary>Op 0x334 (gfx_model_reset_transform@0x49d130): every matrix back to the identity, and the transform
    /// flag cleared.</summary>
    public void ResetModelTransform(long handle)
    {
        GfxModel model = GetOrCreateModel(handle);
        model.TransformSet = false;
        model.Scale = model.Translation = model.AxisRotation = Matrix4x4.Identity;
        MarkRetainedMutation();
    }

    /// <summary>Op 0x335 (gfx_model_set_scale@0x49d310): D3DXMatrixScaling of the three factors, which the handler has
    /// divided by 100.</summary>
    public void SetModelScale(long handle, float x, float y, float z)
    {
        GfxModel model = GetOrCreateModel(handle);
        model.TransformSet = true;
        model.Scale = Matrix4x4.CreateScale(x, y, z);
        MarkRetainedMutation();
    }

    /// <summary>Op 0x337 (gfx_model_set_translation@0x49d470): D3DXMatrixTranslation.</summary>
    public void SetModelTranslation(long handle, float x, float y, float z)
    {
        GfxModel model = GetOrCreateModel(handle);
        model.TransformSet = true;
        model.Translation = Matrix4x4.CreateTranslation(x, y, z);
        MarkRetainedMutation();
    }

    /// <summary>Op 0x33b (gfx_model_set_vector_8@0x49d0f0): three floats, as given.</summary>
    public void SetModelVector8(long handle, float a, float b, float c)
    {
        GetOrCreateModel(handle).Vector8 = new Vector3(a, b, c);
        MarkRetainedMutation();
    }

    /// <summary>Op 0x33e (gfx_model_rotate_axis@0x49d960): the rotation so far times D3DXMatrixRotationAxis(the axis,
    /// degrees × π / 180), so rotations add up until op 0x334. D3DX normalizes the axis; a zero axis stays zero.
    /// </summary>
    public void RotateModelAxis(long handle, float axisX, float axisY, float axisZ, float degrees)
    {
        var axis = new Vector3(axisX, axisY, axisZ);
        float length = axis.Length();
        if (length > 0) axis /= length;
        // The handler's x87 sum: degrees × π (as a float, 0x61d818) / 180, rounded once to a float.
        Matrix4x4 rotation = Matrix4x4.CreateFromAxisAngle(axis, (float)(degrees * (double)MathF.PI / 180.0));
        GfxModel model = GetOrCreateModel(handle);
        model.TransformSet = true;
        model.AxisRotation *= rotation;
        MarkRetainedMutation();
    }
}
