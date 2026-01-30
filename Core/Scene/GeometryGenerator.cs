using Core.Math;
using Core.Scene;
using Core.Scene.Geometry;

namespace Application.Scene;

public static class GeometryGenerator
{
    public static void AddCylinder(SceneEntity scene, Vector3 center, float radius, float height, int segments, Color color)
    {
        float halfHeight = height / 2.0f;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(2.0 * Math.PI * i / segments);
            float angle2 = (float)(2.0 * Math.PI * (i + 1) / segments);

            float x1 = (float)Math.Cos(angle1) * radius;
            float z1 = (float)Math.Sin(angle1) * radius;
            float x2 = (float)Math.Cos(angle2) * radius;
            float z2 = (float)Math.Sin(angle2) * radius;

            Vector3 bottomOuter1 = new(center.X + x1, center.Y - halfHeight, center.Z + z1);
            Vector3 bottomOuter2 = new(center.X + x2, center.Y - halfHeight, center.Z + z2);
            Vector3 topOuter1 = new(center.X + x1, center.Y + halfHeight, center.Z + z1);
            Vector3 topOuter2 = new(center.X + x2, center.Y + halfHeight, center.Z + z2);

            scene.AddTriangle(new TriangleEntity(bottomOuter1, topOuter1, bottomOuter2, color));
            scene.AddTriangle(new TriangleEntity(topOuter1, topOuter2, bottomOuter2, color));

            Vector3 bottomCenter = new(center.X, center.Y - halfHeight, center.Z);
            scene.AddTriangle(new TriangleEntity(bottomCenter, bottomOuter1, bottomOuter2, color));

            Vector3 topCenter = new(center.X, center.Y + halfHeight, center.Z);
            scene.AddTriangle(new TriangleEntity(topCenter, topOuter2, topOuter1, color));
        }
    }

    public static void AddSphere(SceneEntity scene, Vector3 center, float radius, int rings, int segments, Color color)
    {
        for (int ring = 0; ring < rings; ring++)
        {
            float theta1 = (float)(Math.PI * ring / rings);
            float theta2 = (float)(Math.PI * (ring + 1) / rings);

            for (int segment = 0; segment < segments; segment++)
            {
                float phi1 = (float)(2.0 * Math.PI * segment / segments);
                float phi2 = (float)(2.0 * Math.PI * (segment + 1) / segments);

                Vector3 v1 = SphericalToCartesian(center, radius, theta1, phi1);
                Vector3 v2 = SphericalToCartesian(center, radius, theta1, phi2);
                Vector3 v3 = SphericalToCartesian(center, radius, theta2, phi1);
                Vector3 v4 = SphericalToCartesian(center, radius, theta2, phi2);

                if (ring > 0)
                {
                    scene.AddTriangle(new TriangleEntity(v1, v2, v3, color));
                }

                if (ring < rings - 1)
                {
                    scene.AddTriangle(new TriangleEntity(v2, v4, v3, color));
                }
            }
        }
    }

    public static void AddCube(SceneEntity scene, Vector3 center, float size, Color color)
    {
        float half = size / 2.0f;

        Vector3 v000 = new(center.X - half, center.Y - half, center.Z - half);
        Vector3 v001 = new(center.X - half, center.Y - half, center.Z + half);
        Vector3 v010 = new(center.X - half, center.Y + half, center.Z - half);
        Vector3 v011 = new(center.X - half, center.Y + half, center.Z + half);
        Vector3 v100 = new(center.X + half, center.Y - half, center.Z - half);
        Vector3 v101 = new(center.X + half, center.Y - half, center.Z + half);
        Vector3 v110 = new(center.X + half, center.Y + half, center.Z - half);
        Vector3 v111 = new(center.X + half, center.Y + half, center.Z + half);

        scene.AddTriangle(new TriangleEntity(v000, v001, v010, color));
        scene.AddTriangle(new TriangleEntity(v010, v001, v011, color));

        scene.AddTriangle(new TriangleEntity(v100, v110, v101, color));
        scene.AddTriangle(new TriangleEntity(v110, v111, v101, color));

        scene.AddTriangle(new TriangleEntity(v000, v100, v001, color));
        scene.AddTriangle(new TriangleEntity(v100, v101, v001, color));

        scene.AddTriangle(new TriangleEntity(v010, v011, v110, color));
        scene.AddTriangle(new TriangleEntity(v110, v011, v111, color));

        scene.AddTriangle(new TriangleEntity(v000, v010, v100, color));
        scene.AddTriangle(new TriangleEntity(v010, v110, v100, color));

        scene.AddTriangle(new TriangleEntity(v001, v101, v011, color));
        scene.AddTriangle(new TriangleEntity(v101, v111, v011, color));
    }

    private static Vector3 SphericalToCartesian(Vector3 center, float radius, float theta, float phi)
    {
        float sinTheta = (float)Math.Sin(theta);
        float cosTheta = (float)Math.Cos(theta);
        float sinPhi = (float)Math.Sin(phi);
        float cosPhi = (float)Math.Cos(phi);

        return new Vector3(
            center.X + radius * sinTheta * cosPhi,
            center.Y + radius * cosTheta,
            center.Z + radius * sinTheta * sinPhi
        );
    }
}