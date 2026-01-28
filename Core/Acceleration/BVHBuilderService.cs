using Core.Math;
using Core.Scene.Geometry;
using Core.Scene.Acceleration;
using Core.Acceleration.Tasks;

namespace Core.Acceleration;

public class BVHBuilderService
{
    private readonly CalculateBoundsTask calculateBoundsTask;
    private readonly FindBestSplitTask findBestSplitTask;
    private readonly PartitionTrianglesTask partitionTrianglesTask;

    public BVHBuilderService()
    {
        calculateBoundsTask = new CalculateBoundsTask();
        findBestSplitTask = new FindBestSplitTask();
        partitionTrianglesTask = new PartitionTrianglesTask();
    }

    public BVHNodeEntity Build(List<TriangleEntity> triangles)
    {
        if (triangles.Count == 0)
        {
            return new BVHNodeEntity();
        }

        var indices = Enumerable.Range(0, triangles.Count).ToList();
        return BuildRecursive(triangles, indices);
    }

    private BVHNodeEntity BuildRecursive(List<TriangleEntity> triangles, List<int> indices)
    {
        var node = new BVHNodeEntity();

        node.Bounds = calculateBoundsTask.Execute(triangles, indices);

        if (indices.Count <= 2)
        {
            node.TriangleStartIndex = indices[0];
            node.TriangleCount = indices.Count;
            return node;
        }

        var (axis, splitPos) = findBestSplitTask.Execute(triangles, indices, node.Bounds);

        var (leftIndices, rightIndices) = partitionTrianglesTask.Execute(triangles, indices, axis, splitPos);

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
