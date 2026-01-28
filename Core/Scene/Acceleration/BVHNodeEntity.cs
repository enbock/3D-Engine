using Core.Math;

namespace Core.Scene.Acceleration;

public class BVHNodeEntity
{
    public AABB Bounds { get; set; }
    public BVHNodeEntity? Left { get; set; }
    public BVHNodeEntity? Right { get; set; }
    public int TriangleStartIndex { get; set; }
    public int TriangleCount { get; set; }

    public bool IsLeaf => Left == null && Right == null;

    public BVHNodeEntity()
    {
        Bounds = AABB.Empty;
        TriangleStartIndex = -1;
        TriangleCount = 0;
    }
}
