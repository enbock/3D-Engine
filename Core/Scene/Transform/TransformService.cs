using Core.Math;
using Core.Scene.Geometry;

namespace Core.Scene.Transform;

public class TransformService
{
    public TriangleEntity ApplyTransform(TriangleEntity triangle, TransformData transform)
    {
        Matrix4X4 matrix = transform.GetMatrix();

        Vector3 v0 = matrix.TransformPoint(triangle.V0);
        Vector3 v1 = matrix.TransformPoint(triangle.V1);
        Vector3 v2 = matrix.TransformPoint(triangle.V2);

        Vector3 n0 = matrix.TransformDirection(triangle.N0).Normalized;
        Vector3 n1 = matrix.TransformDirection(triangle.N1).Normalized;
        Vector3 n2 = matrix.TransformDirection(triangle.N2).Normalized;

        return new TriangleEntity(
            v0, v1, v2,
            triangle.Material,
            n0, n1, n2,
            triangle.UV0, triangle.UV1, triangle.UV2
        );
    }

    public List<TriangleEntity> ApplyTransform(List<TriangleEntity> triangles, TransformData transform)
    {
        List<TriangleEntity> result = [];
        foreach (TriangleEntity triangle in triangles)
        {
            result.Add(ApplyTransform(triangle, transform));
        }

        return result;
    }

    public void TransformInPlace(TriangleEntity triangle, TransformData transform)
    {
        Matrix4X4 matrix = transform.GetMatrix();

        triangle.V0 = matrix.TransformPoint(triangle.V0);
        triangle.V1 = matrix.TransformPoint(triangle.V1);
        triangle.V2 = matrix.TransformPoint(triangle.V2);

        triangle.N0 = matrix.TransformDirection(triangle.N0).Normalized;
        triangle.N1 = matrix.TransformDirection(triangle.N1).Normalized;
        triangle.N2 = matrix.TransformDirection(triangle.N2).Normalized;
    }

    public void TransformInPlace(List<TriangleEntity> triangles, TransformData transform)
    {
        foreach (TriangleEntity triangle in triangles)
        {
            TransformInPlace(triangle, transform);
        }
    }
}