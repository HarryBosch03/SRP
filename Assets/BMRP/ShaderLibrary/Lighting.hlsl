#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);

float3 IncomingLight(Surface surface, Light light);
float3 GetLighting(Surface surface, Light light);
float3 IndirectLight(Surface surface);
float3 SampleEnvironment(Surface surface);
float GetSpecular(Surface surface, Light light, float specScale, float specExponent);

float3 GetLighting(Surface surface)
{
    float3 color = IndirectLight(surface);

    for (int i = 0; i < _DirectionalLightCount; i++)
    {
        color += GetLighting(surface, GetDirectionalLight(i));
    }

    for (int j = 0; j < _OtherLightCount; j++)
    {
        Light light = GetOtherLight(j, surface);
        color += GetLighting(surface, light);
    }
    
    return color;
}

float3 GetLighting(Surface surface, Light light)
{
    return IncomingLight(surface, light);
}

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction)) * light.color * light.attenuation;
}

float3 IndirectLight(Surface surface)
{
    float4 coefficients[7];
    coefficients[0] = unity_SHAr;
    coefficients[1] = unity_SHAg;
    coefficients[2] = unity_SHAb;
    coefficients[3] = unity_SHBr;
    coefficients[4] = unity_SHBg;
    coefficients[5] = unity_SHBb;
    coefficients[6] = unity_SHC;
    return max(0.0, SampleSH9(coefficients, surface.normal));
}
