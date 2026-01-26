using VulkanEngine.Core.Entities;
using VulkanEngine.Core.Math;

namespace VulkanEngine.Core.Acceleration;

public class BVHBuilder
{
    private const int MaxLeafSize = 4;
    private const float TraversalCost = 1.0f;
    private const float IntersectionCost = 1.0f;

    public static BVHNode Build(List<Triangle> triangles)
    {
        if (triangles.Count == 0)
        {
            return new BVHNode();
        }

        var bounds = AABB.Empty;
        foreach (var triangle in triangles)
        {
            bounds.Expand(AABB.FromTriangle(triangle));
        }

        return BuildRecursive(triangles, bounds, 0);
    }

    private static BVHNode BuildRecursive(List<Triangle> triangles, AABB bounds, int depth)
    {
        var node = new BVHNode { Bounds = bounds };

        if (triangles.Count <= MaxLeafSize || depth > 32)
        {
            node.Triangles = new List<Triangle>(triangles);
            return node;
        }

        var split = FindBestSplit(triangles, bounds);
        if (split == null)
        {
            node.Triangles = new List<Triangle>(triangles);
            return node;
        }

        var leftTriangles = new List<Triangle>();
        var rightTriangles = new List<Triangle>();

        foreach (var triangle in triangles)
        {
            var center = (triangle.V0 + triangle.V1 + triangle.V2) / 3.0f;
            if (center[(int)split.Value.axis] < split.Value.position)
            {
                leftTriangles.Add(triangle);
            }
            else
            {
                rightTriangles.Add(triangle);
            }
        }

        if (leftTriangles.Count == 0 || rightTriangles.Count == 0)
        {
            node.Triangles = new List<Triangle>(triangles);
            return node;
        }

        var leftBounds = AABB.Empty;
        foreach (var triangle in leftTriangles)
        {
            leftBounds.Expand(AABB.FromTriangle(triangle));
        }

        var rightBounds = AABB.Empty;
        foreach (var triangle in rightTriangles)
        {
            rightBounds.Expand(AABB.FromTriangle(triangle));
        }

        node.Left = BuildRecursive(leftTriangles, leftBounds, depth + 1);
        node.Right = BuildRecursive(rightTriangles, rightBounds, depth + 1);

        return node;
    }

    private static (int axis, float position, float cost)? FindBestSplit(List<Triangle> triangles, AABB bounds)
    {
        float bestCost = float.MaxValue;
        int bestAxis = 0;
        float bestPosition = 0;

        for (int axis = 0; axis < 3; axis++)
        {
            var min = bounds.Min[axis];
            var max = bounds.Max[axis];
            var range = max - min;

            if (range < 1e-6f) continue;

            const int numBins = 16;
            for (int i = 1; i < numBins; i++)
            {
                var position = min + (range * i) / numBins;
                var cost = EvaluateSAH(triangles, bounds, axis, position);

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestAxis = axis;
                    bestPosition = position;
                }
            }
        }

        var leafCost = IntersectionCost * triangles.Count;
        if (bestCost >= leafCost)
        {
            return null;
        }

        return (bestAxis, bestPosition, bestCost);
    }

    private static float EvaluateSAH(List<Triangle> triangles, AABB bounds, int axis, float position)
    {
        var leftBounds = AABB.Empty;
        var rightBounds = AABB.Empty;
        int leftCount = 0;
        int rightCount = 0;

        foreach (var triangle in triangles)
        {
            var center = (triangle.V0 + triangle.V1 + triangle.V2) / 3.0f;
            if (center[axis] < position)
            {
                leftBounds.Expand(AABB.FromTriangle(triangle));
                leftCount++;
            }
            else
            {
                rightBounds.Expand(AABB.FromTriangle(triangle));
                rightCount++;
            }
        }

        if (leftCount == 0 || rightCount == 0)
        {
            return float.MaxValue;
        }

        var leftArea = leftBounds.SurfaceArea;
        var rightArea = rightBounds.SurfaceArea;
        var parentArea = bounds.SurfaceArea;

        return TraversalCost +
               (leftArea / parentArea * leftCount + rightArea / parentArea * rightCount) * IntersectionCost;
    }
}
