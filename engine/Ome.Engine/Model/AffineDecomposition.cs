namespace Ome.Engine.Model;

/// <summary>
/// The scale part of native's affine matrix decomposition, matrix4_decompose_affine (4.24J 0x48d7c8; D3DX's
/// D3DXMatrixDecompose, linked into the engine), as op 0x226 reads it (vm-map/opcodes.toml). Each scale is its row's
/// length. The rows are made an orthonormal basis, largest first: a row shorter than 1e-4 becomes the identity's row
/// (the largest), the cross product of the largest with the axis it has least of (the middle), or the cross product of
/// the other two (the smallest). When that basis is mirrored (a negative determinant) the largest scale is negated.
/// The decomposition fails when the determinant is then not 1.
/// </summary>
public static class AffineDecomposition
{
    // DAT_0058075c in 4.24J, the float 1e-4.
    private const float Epsilon = 1e-4f;

    /// <summary>The scale of the 3x3 part given by its rows; false where native returns D3DERR_INVALIDCALL.</summary>
    public static bool TryScale(float[][] rows, out (float X, float Y, float Z) scale)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Length != 3 || rows.Any(row => row is null || row.Length != 3))
            throw new ArgumentException("three rows of three", nameof(rows));
        float[][] r = rows.Select(row => (float[])row.Clone()).ToArray();
        float[] s = r.Select(Length).ToArray();

        // The largest, middle and smallest rows, with native's tie order.
        int max, mid, min;
        if (s[1] <= s[0])
        {
            if (s[2] <= s[0])
            {
                max = 0;
                (mid, min) = s[2] <= s[1] ? (1, 2) : (2, 1);
            }
            else (max, mid, min) = (2, 0, 1);
        }
        else if (s[2] <= s[1])
        {
            max = 1;
            (mid, min) = s[2] <= s[0] ? (0, 2) : (2, 0);
        }
        else (max, mid, min) = (2, 1, 0);

        if (s[max] < Epsilon) r[max] = Identity(max);
        Normalize(r[max]);
        if (s[mid] < Epsilon)
        {
            float ax = Math.Abs(r[max][0]), ay = Math.Abs(r[max][1]), az = Math.Abs(r[max][2]);
            int axis;
            if (ay <= ax) axis = az <= ax && az <= ay ? 2 : 1;
            else axis = ay < az || ax < az ? 0 : 2;
            r[mid] = Cross(r[max], Identity(axis));
        }
        Normalize(r[mid]);
        if (s[min] < Epsilon) r[min] = Cross(r[max], r[mid]);
        Normalize(r[min]);

        float determinant = Determinant(r);
        if (determinant < 0f)
        {
            s[max] = -s[max];
            determinant = -determinant;
        }
        scale = (s[0], s[1], s[2]);
        return (determinant - 1f) * (determinant - 1f) <= Epsilon;
    }

    private static float Length(float[] v) => (float)Math.Sqrt((double)v[0] * v[0] + (double)v[1] * v[1] + (double)v[2] * v[2]);

    private static float[] Identity(int axis) => [axis == 0 ? 1f : 0f, axis == 1 ? 1f : 0f, axis == 2 ? 1f : 0f];

    // D3DXVec3Normalize: a zero vector stays zero.
    private static void Normalize(float[] v)
    {
        float length = Length(v);
        if (length == 0f) return;
        for (int i = 0; i < 3; i++) v[i] /= length;
    }

    private static float[] Cross(float[] a, float[] b) =>
        [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];

    private static float Determinant(float[][] m) =>
        m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
        - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
        + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);
}
