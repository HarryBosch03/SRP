#pragma once

CBUFFER_START(_CustomLight)

static const int MaxDirectionalLights = 4, MaxOtherLights = 64;

int _DirectionalLightCount;
float3 _DirectionalLightColors[MaxDirectionalLights];
float3 _DirectionalLightDirections[MaxDirectionalLights];

int _OtherLightCount;
float3 _OtherLightColors[MaxOtherLights];
float3 _OtherLightPositions[MaxOtherLights];
float4 _OtherLightDirections[MaxOtherLights];
float4 _OtherLightSpotAngles[MaxOtherLights];
CBUFFER_END

struct Light
{
    float3 direction;
    float3 color;
    float3 attenuation;
};

Light GetDirectionalLight (int index)
{
    Light light;

    light.direction = -_DirectionalLightDirections[index];
    light.color = _DirectionalLightColors[index];
    light.attenuation = 1.0;
    
    return light;
}

float remap (float v, float2 from, float2 to)
{
    v = (v - from.x) / (from.y - from.x);
    return v * (to.y - to.x) + to.x;
}

Light GetOtherLight (int index, Surface surface)
{
    Light light;

    light.color = _OtherLightColors[index];
    float3 ray = _OtherLightPositions[index].xyz - surface.position;
    float distance = max(Length2(ray), 0.0001);
    light.direction = normalize(ray);

    float4 spotAngles = _OtherLightSpotAngles[index];
    float4 direction = _OtherLightDirections[index];
    float spotAttenuation = dot(direction.xyz, light.direction);
    spotAttenuation = (spotAttenuation - spotAngles.x) / (1.0 - spotAngles.x);
    spotAttenuation = lerp(1.0, spotAttenuation, direction.w);
    spotAttenuation = saturate(spotAttenuation);
    
    light.attenuation = spotAttenuation / distance;
    
    return light;
}