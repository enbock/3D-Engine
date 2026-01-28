using Core.Math;
using Core.Scene.Geometry;

namespace Core.Acceleration.Tasks;

public class CalculateBoundsTask
{
    public AABB Execute(List<TriangleEntity> triangles, List<int> indices)
    {
        var bounds = AABB.Empty;

        foreach (var index in indices)
        {
            var triangle = triangles[index];
            bounds.Expand(triangle.V0);
            bounds.Expand(triangle.V1);
            bounds.Expand(triangle.V2);
        }

        return bounds;
    }
}
