#pragma once

CBUFFER_START(_CustomLight)

static const int MaxDirectionalLights = 4, MaxOtherLights = 64;

int _DirectionalLightCount;
float3 _DirectionalLightColors[MaxDirectionalLights];
float3 _DirectionalLightDirections[MaxDirectionalLights];

int _OtherLightCount;
float3 _OtherLightColors[MaxOtherLights];
float3 _OtherLightPositions[MaxOtherLights];
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

Light GetOtherLight (int index, Surface surface)
{
    Light light;

    light.color = _OtherLightColors[index];
    float3 ray = _OtherLightPositions[index].xyz - surface.position;
    float distance = Length2(ray);
    light.direction = normalize(ray);
    light.attenuation = 1.0f / distance;
    
    return light;
}