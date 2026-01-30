using Core.Math;

namespace Core.Scene.Geometry;

public class TriangleEntity
{
    public TriangleEntity(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Material = MaterialEntity.Opaque(color);
        Vector3 faceNormal = FaceNormal;
        N0 = faceNormal;
        N1 = faceNormal;
        N2 = faceNormal;
    }

    public TriangleEntity(Vector3 v0, Vector3 v1, Vector3 v2, Color color, Vector3 n0, Vector3 n1, Vector3 n2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Material = MaterialEntity.Opaque(color);
        N0 = n0;
        N1 = n1;
        N2 = n2;
    }

    public TriangleEntity(Vector3 v0, Vector3 v1, Vector3 v2, MaterialEntity material)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Material = material;
        Vector3 faceNormal = FaceNormal;
        N0 = faceNormal;
        N1 = faceNormal;
        N2 = faceNormal;
    }

    public TriangleEntity(Vector3 v0, Vector3 v1, Vector3 v2, MaterialEntity material, Vector3 n0, Vector3 n1, Vector3 n2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Material = material;
        N0 = n0;
        N1 = n1;
        N2 = n2;
    }

    public Vector3 V0 { get; set; }
    public Vector3 V1 { get; set; }
    public Vector3 V2 { get; set; }
    public MaterialEntity Material { get; set; }
    public Color Color => Material.Color;
    public Vector3 N0 { get; set; }
    public Vector3 N1 { get; set; }
    public Vector3 N2 { get; set; }

    public Vector3 FaceNormal
    {
        get
        {
            Vector3 edge1 = V1 - V0;
            Vector3 edge2 = V2 - V0;
            return Vector3.Cross(edge1, edge2).Normalized;
        }
    }

    public Vector3 Normal => FaceNormal;

    public Vector3 Center => (V0 + V1 + V2) / 3.0f;
}