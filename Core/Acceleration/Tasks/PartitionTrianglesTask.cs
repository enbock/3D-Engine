using Core.Math;
using Core.Scene.Geometry;

namespace Core.Acceleration.Tasks;

public class PartitionTrianglesTask
{
    public static (List<int> left, List<int> right) Execute(List<TriangleEntity> triangles, List<int> indices, int axis,
        float splitPos)
    {
        List<int> leftIndices = [];
        List<int> rightIndices = [];

        foreach (int index in indices)
        {
            TriangleEntity triangle = triangles[index];
            Vector3 center = triangle.Center;
            float value = center[axis];

            if (value < splitPos)
                leftIndices.Add(index);
            else
                rightIndices.Add(index);
        }

        return (leftIndices, rightIndices);
    }
}