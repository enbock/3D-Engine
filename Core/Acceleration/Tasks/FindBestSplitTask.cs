using Core.Math;
using Core.Scene.Geometry;

namespace Core.Acceleration.Tasks;

public class FindBestSplitTask
{
    public static (int axis, float splitPos) Execute(List<TriangleEntity> triangles, List<int> indices, Aabb bounds)
    {
        Vector3 size = bounds.Size;

        int bestAxis = 0;
        if (size.Y > size.X && size.Y > size.Z)
            bestAxis = 1;
        else if (size.Z > size.X && size.Z > size.Y)
            bestAxis = 2;

        float splitPos = bounds.Center[bestAxis];

        return (bestAxis, splitPos);
    }
}