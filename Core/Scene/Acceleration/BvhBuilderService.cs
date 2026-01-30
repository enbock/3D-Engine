using Core.Math;
using Core.Scene.Geometry;

namespace Core.Scene.Acceleration;

public class BvhBuilderService
{
    private const int MaxTrianglesPerLeaf = 4;
    private List<int> _triangleIndices = [];
    private List<TriangleEntity> _triangles = [];

    public BvhNodeEntity Build(List<TriangleEntity> triangles)
    {
        _triangles = triangles;
        _triangleIndices = Enumerable.Range(0, triangles.Count).ToList();

        if (triangles.Count == 0)
        {
            return new BvhNodeEntity { TriangleStartIndex = 0, TriangleCount = 0 };
        }

        return BuildRecursive(0, triangles.Count);
    }

    public (List<FlatBvhNode> nodes, List<int> reorderedIndices) FlattenForGpu(BvhNodeEntity root)
    {
        List<FlatBvhNode> flatNodes = [];
        FlattenRecursive(root, flatNodes);
        return (flatNodes, _triangleIndices.ToList());
    }

    public List<int> GetReorderedTriangleIndices()
    {
        return _triangleIndices.ToList();
    }

    private BvhNodeEntity BuildRecursive(int start, int count)
    {
        BvhNodeEntity node = new();
        node.Bounds = ComputeBounds(start, count);

        if (count <= MaxTrianglesPerLeaf)
        {
            node.TriangleStartIndex = start;
            node.TriangleCount = count;
            return node;
        }

        int axis = GetLongestAxis(node.Bounds);
        SortTrianglesByAxis(start, count, axis);

        int mid = count / 2;

        node.Left = BuildRecursive(start, mid);
        node.Right = BuildRecursive(start + mid, count - mid);

        return node;
    }

    private Aabb ComputeBounds(int start, int count)
    {
        Aabb bounds = Aabb.Empty;

        for (int i = 0; i < count; i++)
        {
            int idx = _triangleIndices[start + i];
            TriangleEntity tri = _triangles[idx];
            bounds.Expand(new Vector3(tri.V0.X, tri.V0.Y, tri.V0.Z));
            bounds.Expand(new Vector3(tri.V1.X, tri.V1.Y, tri.V1.Z));
            bounds.Expand(new Vector3(tri.V2.X, tri.V2.Y, tri.V2.Z));
        }

        return bounds;
    }

    private int GetLongestAxis(Aabb bounds)
    {
        Vector3 size = bounds.Size;
        if (size.X >= size.Y && size.X >= size.Z) return 0;
        if (size.Y >= size.X && size.Y >= size.Z) return 1;
        return 2;
    }

    private void SortTrianglesByAxis(int start, int count, int axis)
    {
        List<int> indices = _triangleIndices.GetRange(start, count);

        indices.Sort((a, b) =>
        {
            Vector3 centerA = _triangles[a].Center;
            Vector3 centerB = _triangles[b].Center;

            float valA = axis switch { 0 => centerA.X, 1 => centerA.Y, _ => centerA.Z };
            float valB = axis switch { 0 => centerB.X, 1 => centerB.Y, _ => centerB.Z };

            return valA.CompareTo(valB);
        });

        for (int i = 0; i < count; i++)
        {
            _triangleIndices[start + i] = indices[i];
        }
    }

    private int FlattenRecursive(BvhNodeEntity node, List<FlatBvhNode> flatNodes)
    {
        int myIndex = flatNodes.Count;

        FlatBvhNode flatNode = new()
        {
            BoundsMin = node.Bounds.Min,
            BoundsMax = node.Bounds.Max,
            LeftChild = -1,
            RightChild = -1,
            TriangleStart = node.TriangleStartIndex,
            TriangleCount = node.TriangleCount
        };

        flatNodes.Add(flatNode);

        if (!node.IsLeaf)
        {
            flatNodes[myIndex] = flatNode with
            {
                LeftChild = FlattenRecursive(node.Left!, flatNodes),
                TriangleStart = -1,
                TriangleCount = 0
            };

            FlatBvhNode updated = flatNodes[myIndex];
            flatNodes[myIndex] = updated with
            {
                RightChild = FlattenRecursive(node.Right!, flatNodes)
            };
        }

        return myIndex;
    }
}

public record struct FlatBvhNode
{
    public Vector3 BoundsMax;
    public Vector3 BoundsMin;
    public int LeftChild;
    public int RightChild;
    public int TriangleCount;
    public int TriangleStart;
}