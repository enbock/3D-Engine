using Core.Math;
using Core.Scene.Geometry;

namespace Core.Acceleration.Tasks;

public class CalculateBoundsTask
{
    public static Aabb Execute(List<TriangleEntity> triangles, List<int> indices)
    {
        Aabb bounds = Aabb.Empty;

        foreach (TriangleEntity triangle in indices.Select(index => triangles[index]))
        {
            bounds.Expand(triangle.V0);
            bounds.Expand(triangle.V1);
            bounds.Expand(triangle.V2);
        }

        return bounds;
    }
}