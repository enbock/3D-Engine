namespace Core.Math;

public struct Matrix4X4
{
    private readonly float[] _elements;

    public Matrix4X4()
    {
        _elements = new float[16];
        _elements[0] = 1;
        _elements[5] = 1;
        _elements[10] = 1;
        _elements[15] = 1;
    }

    public float this[int index]
    {
        get => _elements[index];
        set => _elements[index] = value;
    }

    public static Matrix4X4 Identity()
    {
        return new Matrix4X4();
    }

    public static Matrix4X4 Translation(Vector3 translation)
    {
        Matrix4X4 m = new();
        m._elements[12] = translation.X;
        m._elements[13] = translation.Y;
        m._elements[14] = translation.Z;
        return m;
    }

    public static Matrix4X4 Scale(Vector3 scale)
    {
        Matrix4X4 m = new();
        m._elements[0] = scale.X;
        m._elements[5] = scale.Y;
        m._elements[10] = scale.Z;
        return m;
    }

    public static Matrix4X4 RotationX(float angle)
    {
        float c = MathF.Cos(angle);
        float s = MathF.Sin(angle);
        Matrix4X4 m = new();
        m._elements[5] = c;
        m._elements[6] = s;
        m._elements[9] = -s;
        m._elements[10] = c;
        return m;
    }

    public static Matrix4X4 RotationY(float angle)
    {
        float c = MathF.Cos(angle);
        float s = MathF.Sin(angle);
        Matrix4X4 m = new();
        m._elements[0] = c;
        m._elements[2] = -s;
        m._elements[8] = s;
        m._elements[10] = c;
        return m;
    }

    public static Matrix4X4 RotationZ(float angle)
    {
        float c = MathF.Cos(angle);
        float s = MathF.Sin(angle);
        Matrix4X4 m = new();
        m._elements[0] = c;
        m._elements[1] = s;
        m._elements[4] = -s;
        m._elements[5] = c;
        return m;
    }

    public static Matrix4X4 Rotation(Vector3 rotation)
    {
        return RotationZ(rotation.Z) * RotationY(rotation.Y) * RotationX(rotation.X);
    }

    public static Matrix4X4 operator *(Matrix4X4 a, Matrix4X4 b)
    {
        Matrix4X4 result = new();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                result._elements[i * 4 + j] =
                    a._elements[i * 4 + 0] * b._elements[0 * 4 + j] +
                    a._elements[i * 4 + 1] * b._elements[1 * 4 + j] +
                    a._elements[i * 4 + 2] * b._elements[2 * 4 + j] +
                    a._elements[i * 4 + 3] * b._elements[3 * 4 + j];
            }
        }

        return result;
    }

    public Vector3 TransformPoint(Vector3 point)
    {
        float x = _elements[0] * point.X + _elements[4] * point.Y + _elements[8] * point.Z + _elements[12];
        float y = _elements[1] * point.X + _elements[5] * point.Y + _elements[9] * point.Z + _elements[13];
        float z = _elements[2] * point.X + _elements[6] * point.Y + _elements[10] * point.Z + _elements[14];
        float w = _elements[3] * point.X + _elements[7] * point.Y + _elements[11] * point.Z + _elements[15];

        if (MathF.Abs(w) > 0.0001f)
        {
            return new Vector3(x / w, y / w, z / w);
        }

        return new Vector3(x, y, z);
    }

    public Vector3 TransformDirection(Vector3 direction)
    {
        float x = _elements[0] * direction.X + _elements[4] * direction.Y + _elements[8] * direction.Z;
        float y = _elements[1] * direction.X + _elements[5] * direction.Y + _elements[9] * direction.Z;
        float z = _elements[2] * direction.X + _elements[6] * direction.Y + _elements[10] * direction.Z;
        return new Vector3(x, y, z);
    }
}