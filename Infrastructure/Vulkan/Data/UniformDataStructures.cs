using System.Numerics;
using System.Runtime.InteropServices;

namespace Infrastructure.Vulkan.Data;

[StructLayout(LayoutKind.Sequential)]
public struct CameraUniformData
{
    public Vector3 Position;
    public float Pad1;
    public Vector3 Target;
    public float Pad2;
    public Vector2 Resolution;
    public float Time;
    public float Fov;
}

[StructLayout(LayoutKind.Sequential)]
public struct LightData
{
    public int Type;
    public float Intensity;
    public float ShadowSoftness;
    public float Pad1;

    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float Pad2;

    public float DirectionX;
    public float DirectionY;
    public float DirectionZ;
    public float Pad3;

    public float ColorR;
    public float ColorG;
    public float ColorB;
    public float Pad4;
}

[StructLayout(LayoutKind.Sequential)]
public struct LightUniformData
{
    public int NumLights;
    public int Pad1;
    public int Pad2;
    public int Pad3;
    public LightData Light0;
    public LightData Light1;
    public LightData Light2;
    public LightData Light3;
    public LightData Light4;
    public LightData Light5;
    public LightData Light6;
    public LightData Light7;

    public void SetLight(int index, LightData light)
    {
        switch (index)
        {
            case 0: Light0 = light; break;
            case 1: Light1 = light; break;
            case 2: Light2 = light; break;
            case 3: Light3 = light; break;
            case 4: Light4 = light; break;
            case 5: Light5 = light; break;
            case 6: Light6 = light; break;
            case 7: Light7 = light; break;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct RenderSettingsData
{
    public int MaxBounces;
    public int EnableShadows;
    public int EnableReflections;
    public float ReflectionStrength;
    public int ShadowSamples;
    public int Pad1;
    public Vector2 Pad2;
}

[StructLayout(LayoutKind.Sequential)]
public struct TriangleData
{
    public Vector3 V0;
    public float Pad0;
    public Vector3 V1;
    public float Pad1;
    public Vector3 V2;
    public float Pad2;
    public Vector3 Color;
    public float Transparency;
    public Vector3 N0;
    public float IndexOfRefraction;
    public Vector3 N1;
    public float Reflectivity;
    public Vector3 N2;
    public float EnableSchlieren;
}

[StructLayout(LayoutKind.Sequential)]
public struct BvhNodeData
{
    public Vector3 BoundsMin;
    public int LeftChild;
    public Vector3 BoundsMax;
    public int RightChild;
    public int TriangleStart;
    public int TriangleCount;
    public int Pad0;
    public int Pad1;
}