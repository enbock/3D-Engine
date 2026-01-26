using VulkanEngine.Core.Entities;
using VulkanEngine.Core.Math;

namespace VulkanEngine.Core.Acceleration;

public class BVHNode
{
    public AABB Bounds { get; set; }
    public BVHNode? Left { get; set; }
    public BVHNode? Right { get; set; }
    public List<Triangle> Triangles { get; set; }
    public bool IsLeaf => Left == null && Right == null;

    public BVHNode()
    {
        Bounds = AABB.Empty;
        Triangles = new List<Triangle>();
    }

    public BVHNode(AABB bounds, List<Triangle> triangles)
    {
        Bounds = bounds;
        Triangles = triangles;
    }
}
