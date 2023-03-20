#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#if defined(_DIRECTIONAL_PCF3)
    #define DIRECTIONAL_FILTER_SAMPLES 4
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
    #define DIRECTIONAL_FILTER_SAMPLES 9
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
    #define DIRECTIONAL_FILTER_SAMPLES 16
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#if defined(_OTHER_PCF3)
    #define OTHER_FILTER_SAMPLES 4
    #define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_OTHER_PCF5)
    #define OTHER_FILTER_SAMPLES 9
    #define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_OTHER_PCF7)
    #define OTHER_FILTER_SAMPLES 16
    #define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_SHADOWED_OTHER_LIGHT_COUNT 16
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
TEXTURE2D_SHADOW(_OtherShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
int _CascadeCount;
float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
float4 _CascadeData[MAX_CASCADE_COUNT];
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
float4x4 _OtherShadowMatrices[MAX_SHADOWED_OTHER_LIGHT_COUNT];
float4 _OtherShadowTiles[MAX_SHADOWED_OTHER_LIGHT_COUNT];
float4 _ShadowAtlasSize;
float4 _ShadowDistanceFade;
CBUFFER_END

struct ShadowMask
{
    bool always;
    bool distance;
    float4 shadows;
};

struct ShadowData
{
    int cascadeIndex;
	float cascadeBlend;
    float strength;
    ShadowMask shadowMask;
};

struct OtherShadowData
{
    float strength;
	int tileIndex;
    int shadowMaskChannel;
    float3 lightPositionWS;
    float3 spotDirectionWS;
};

float FadedShadowStrength (const float distance, const float scale, const float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData (const Surface surfaceWS)
{
    ShadowData data;
    data.shadowMask.always = false;
    data.shadowMask.distance = false;
    data.shadowMask.shadows = 1.0;
    
    data.cascadeBlend = 1.0;
    data.strength = FadedShadowStrength( surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);

    int i;
    for (i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
        if (distSqr < sphere.w)
        {
            float fade = FadedShadowStrength(distSqr, _CascadeData[i].x, _ShadowDistanceFade.z);
            if (i == _CascadeCount - 1)
            {
                data.strength *= FadedShadowStrength(distSqr, _CascadeData[i].x, _ShadowDistanceFade.z);
            }
            else
            {
                data.cascadeBlend = fade;
            }
            break;
        }
    }

    if (i == _CascadeCount && _CascadeCount > 0) data.strength = 0.0;

#if defined(_CASCADE_BLEND_DITHER)
    else if (data.cascadeBlend < surfaceWS.dither) i += 1;
#endif
#if !defined(_CASCADE_BLEND_SOFT)
    data.cascadeBlend = 1.0;
#endif
    
    data.cascadeIndex = i;
    return data;
}

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
	float normalBias;
	int shadowMaskChannel;
};

float SampleDirectionalShadowAtlas (float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float FilterDirectionalShadow (const float3 positionSTS)
{
#if defined(DIRECTIONAL_FILTER_SETUP)
    
    float weights[DIRECTIONAL_FILTER_SAMPLES];
    float2 positions[DIRECTIONAL_FILTER_SAMPLES];
    float4 size = _ShadowAtlasSize.yyxx;
    DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
    float shadow = 0;
    for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++)
    {
        shadow += weights[i] * SampleDirectionalShadowAtlas(float3(positions[i].xy, positionSTS.z));
    }
    return shadow;
    
#else
    
    return SampleDirectionalShadowAtlas(positionSTS);
    
#endif
}

float SampleOtherShadowAtlas  (float3 positionSTS, float3 bounds)
{
    positionSTS.xy = clamp(positionSTS.xy, bounds.xy, bounds.xy + bounds.z);
    return SAMPLE_TEXTURE2D_SHADOW(_OtherShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float FilterOtherShadow  (const float3 positionSTS, float3 bounds)
{
#if defined(OTHER_FILTER_SETUP)
    float weights[DIRECTIONAL_FILTER_SAMPLES];
    float2 positions[DIRECTIONAL_FILTER_SAMPLES];
    float4 size = _ShadowAtlasSize.yyxx;
    OTHER_FILTER_SETUP(size, positionSTS.xy, weights, positions);
    float shadow = 0;
    for (int i = 0; i < OTHER_FILTER_SAMPLES; i++)
    {
        shadow += weights[i] * SampleOtherShadowAtlas(float3(positions[i].xy, positionSTS.z), bounds);
    }
    return shadow;
#else
    return SampleOtherShadowAtlas(positionSTS, bounds);
#endif
}

float GetCascadedShadow (DirectionalShadowData directional, ShadowData global, const Surface surfaceWS)
{
    float3 normalBias = surfaceWS.interpolatedNormal * (directional.normalBias * _CascadeData[global.cascadeIndex].y);
    float3 positionSTS = mul(_DirectionalShadowMatrices[directional.tileIndex], float4(surfaceWS.position + normalBias, 1.0)).xyz;
    float shadow = FilterDirectionalShadow(positionSTS);

    if (global.cascadeBlend < 1.0)
    {
        normalBias = surfaceWS.interpolatedNormal * (directional.normalBias * _CascadeData[global.cascadeIndex + 1].y);
        positionSTS = mul(_DirectionalShadowMatrices[directional.tileIndex + 1], float4(surfaceWS.position + normalBias, 1.0)).xyz;
        shadow = lerp(FilterDirectionalShadow(positionSTS), shadow, global.cascadeBlend);
    }
    return shadow;
}

float GetBakedShadow (ShadowMask mask, int channel)
{
    float shadow = 1.0;
    if (mask.always || mask.distance)
    {
        if (channel >= 0) shadow = mask.shadows[channel];
    }
    return shadow;
}

float GetBakedShadow (ShadowMask mask, int channel, float strength)
{
    if (mask.always || mask.distance)
    {
        return lerp(1.0, GetBakedShadow(mask, channel), strength);
    }
    return 1.0;
}

float MixBakedAndRealtimeShadows (const ShadowData global, float shadow, int shadowMaskChannel, const float strength)
{
    const float baked = GetBakedShadow(global.shadowMask, shadowMaskChannel);
    if (global.shadowMask.always)
    {
        shadow = lerp(1.0, shadow, global.strength);
        shadow = min(baked, shadow);
        return lerp(1.0, shadow, strength);
    }
    if (global.shadowMask.distance)
    {
        shadow = lerp(baked, shadow, global.strength);
        return lerp(1.0, shadow, strength);
    }
    return lerp(1.0, shadow, strength * global.strength);
}

float GetDirectionalShadowAttenuation (const DirectionalShadowData directional, const ShadowData global, const Surface surfaceWS)
{
#if !defined(_RECEIVE_SHADOWS)
    return 1.0;
#endif
	
    float shadow;
    if (directional.strength * global.strength <= 0.0)
    {
        shadow = GetBakedShadow(global.shadowMask, directional.shadowMaskChannel, abs(directional.strength));
    }
    else
    {
        shadow = GetCascadedShadow(directional, global, surfaceWS);
        shadow = MixBakedAndRealtimeShadows(global, shadow, directional.shadowMaskChannel, directional.strength);
    }
    return shadow;
}

float GetOtherShadow (const OtherShadowData other, const ShadowData global, const Surface surface)
{
    float4 tileData = _OtherShadowTiles[other.tileIndex];
    float3 surfaceToLight = other.lightPositionWS - surface.position;
    float distanceToLightPlane = dot(surfaceToLight, other.spotDirectionWS);
    float3 normalBias = surface.interpolatedNormal * (distanceToLightPlane * tileData.w);
    float4 positionSTS = mul(_OtherShadowMatrices[other.tileIndex], float4(surface.position + normalBias, 1.0f));
    return FilterOtherShadow(positionSTS.xyz / positionSTS.w, tileData.xyz);
}

float GetOtherShadowAttenuation (OtherShadowData other, ShadowData global, Surface surface)
{
#if !defined(_RECEIVE_SHADOWS)
    return 1.0;
#endif

    float shadow;
    if (other.strength * global.strength <= 0.0)
    {
        shadow = GetBakedShadow(global.shadowMask, other.shadowMaskChannel, abs(other.strength));
    }
    else
    {
        shadow = GetOtherShadow(other, global, surface);
        shadow = MixBakedAndRealtimeShadows(global, shadow, other.shadowMaskChannel, other.strength);
    }
    return shadow;
}

#endif