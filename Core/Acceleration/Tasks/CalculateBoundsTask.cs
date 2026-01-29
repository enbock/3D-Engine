using Core.Math;
using Core.Scene.Geometry;

namespace Core.Acceleration.Tasks;

public class CalculateBoundsTask
{
    public AABB Execute(List<TriangleEntity> triangles, List<int> indices)
    {
        AABB bounds = AABB.Empty;

        foreach (int index in indices)
        {
            TriangleEntity triangle = triangles[index];
            bounds.Expand(triangle.V0);
            bounds.Expand(triangle.V1);
            bounds.Expand(triangle.V2);
        }

        return bounds;
    }
}