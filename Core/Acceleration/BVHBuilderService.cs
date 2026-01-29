using Core.Acceleration.Tasks;
using Core.Scene.Acceleration;
using Core.Scene.Geometry;

namespace Core.Acceleration;

public class BvhBuilderService
{
    private readonly CalculateBoundsTask calculateBoundsTask = new();
    private readonly FindBestSplitTask findBestSplitTask = new();
    private readonly PartitionTrianglesTask partitionTrianglesTask = new();

    public BvhNodeEntity Build(List<TriangleEntity> triangles)
    {
        if (triangles.Count == 0) return new BvhNodeEntity();

        List<int> indices = Enumerable.Range(0, triangles.Count).ToList();
        return BuildRecursive(triangles, indices);
    }

    private BvhNodeEntity BuildRecursive(List<TriangleEntity> triangles, List<int> indices)
    {
        BvhNodeEntity node = new();

        node.Bounds = CalculateBoundsTask.Execute(triangles, indices);

        if (indices.Count <= 2)
        {
            node.TriangleStartIndex = indices[0];
            node.TriangleCount = indices.Count;
            return node;
        }

        (int axis, float splitPos) = FindBestSplitTask.Execute(triangles, indices, node.Bounds);

        (List<int> leftIndices, List<int> rightIndices) =
            PartitionTrianglesTask.Execute(triangles, indices, axis, splitPos);

        if (leftIndices.Count == 0 || rightIndices.Count == 0)
        {
            node.TriangleStartIndex = indices[0];
            node.TriangleCount = indices.Count;
            return node;
        }

        node.Left = BuildRecursive(triangles, leftIndices);
        node.Right = BuildRecursive(triangles, rightIndices);

        return node;
    }
}