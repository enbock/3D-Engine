using Core.Math;

namespace Core.Scene.Acceleration;

public class BvhNodeEntity
{
    public Aabb Bounds { get; set; } = Aabb.Empty;
    public BvhNodeEntity? Left { get; set; }
    public BvhNodeEntity? Right { get; set; }
    public int TriangleStartIndex { get; set; } = -1;
    public int TriangleCount { get; set; } = 0;

    public bool IsLeaf => Left == null && Right == null;
}