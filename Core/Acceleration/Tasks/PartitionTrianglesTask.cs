using Core.Scene.Geometry;

namespace Core.Acceleration.Tasks;

public class PartitionTrianglesTask
{
    public (List<int> left, List<int> right) Execute(List<TriangleEntity> triangles, List<int> indices, int axis, float splitPos)
    {
        var leftIndices = new List<int>();
        var rightIndices = new List<int>();

        foreach (var index in indices)
        {
            var triangle = triangles[index];
            var center = triangle.Center;
            var value = center[axis];

            if (value < splitPos)
            {
                leftIndices.Add(index);
            }
            else
            {
                rightIndices.Add(index);
            }
        }

        return (leftIndices, rightIndices);
    }
}
