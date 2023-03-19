﻿#ifndef CUSTOM_GI_INCLUDED
#define CUSTOM_GI_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"

#if defined(LIGHTMAP_ON)
    #define GI_ATTRIBUTE_DATA float2 lightMapUV : TEXCOORD1;
    #define GI_VARYINGS_DATA float2 lightMapUV : VAR_LIGHT_MAP_UV;
    #define TRANSFER_GI_DATA(input, output) \
    output.lightMapUV = input.lightMapUV * \
    unity_LightmapST.xy + unity_LightmapST.zw;
    #define GI_FRAGMENT_DATA(i) i.lightMapUV
#else
#define GI_ATTRIBUTE_DATA
#define GI_VARYINGS_DATA
#define TRANSFER_GI_DATA(input, output)
#define GI_FRAGMENT_DATA(input) 0.0
#endif

TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);

TEXTURE2D(unity_Lightmap);
SAMPLER(samplerunity_Lightmap);

TEXTURE2D(unity_ShadowMask);
SAMPLER(samplerunity_ShadowMask);

TEXTURE3D_FLOAT(unity_ProbeVolumeSH);
SAMPLER(samplerunity_ProbeVolumeSH);

struct GI
{
    float3 diffuse;
    float3 specular;
	ShadowMask shadowMask;
};

float3 SampleEnvironment(const Surface surfaceWS, BRDF brdf);
float3 SampleLightMap(float2 lightMapUV);
float3 SampleLightProbe(const Surface surfaceWS);
float4 SampleBakedShadows (float2 lightMapUV, const Surface surfaceWS);

GI GetGI(const float2 lightMapUv, const Surface surface, BRDF brdf)
{
    GI gi;
    gi.diffuse = SampleLightMap(lightMapUv) + SampleLightProbe(surface);

	gi.shadowMask.always = false;
    gi.shadowMask.distance = false;
    gi.shadowMask.shadows = 1.0;

    gi.specular = SampleEnvironment(surface, brdf);
    
#if defined(_SHADOW_MASK_ALWAYS)
    gi.shadowMask.always = true;
    gi.shadowMask.shadows = SampleBakedShadows(lightMapUv, surface);
#elif defined(_SHADOW_MASK_DISTANCE)
    gi.shadowMask.distance = true;
    gi.shadowMask.shadows = SampleBakedShadows(lightMapUv, surface);
#endif
    
    return gi;
}

float3 SampleLightProbe(const Surface surfaceWS)
{
#if defined(LIGHTMAP_ON)
    return 0.0;
#else
    float4 coefficients[7];
    coefficients[0] = unity_SHAr;
    coefficients[1] = unity_SHAg;
    coefficients[2] = unity_SHAb;
    coefficients[3] = unity_SHBr;
    coefficients[4] = unity_SHBg;
    coefficients[5] = unity_SHBb;
    coefficients[6] = unity_SHC;
    return max(0.0, SampleSH9(coefficients, surfaceWS.normal));
#endif
}

float3 SampleLightMap(float2 lightMapUV)
{
#if defined(LIGHTMAP_ON)
    return SampleSingleLightmap
    (
        TEXTURE2D_ARGS(unity_Lightmap, samplerunity_Lightmap),
        lightMapUV,
        float4(1.0, 1.0, 0.0, 0.0),
#if defined(UNITY_LIGHTMAP_FULL_HDR)
        false,
#else
        true,
#endif
        float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0, 0.0)
    );
#else
    return 0.0;
#endif
}


float3 SampleEnvironment(const Surface surfaceWS, BRDF brdf)
{
    float3 uvw = reflect(-surfaceWS.viewDirection, surfaceWS.normal);
    float mip = PerceptualRoughnessToMipmapLevel(brdf.perceptualRoughness);
    float4 environment = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uvw, mip);
    return DecodeHDREnvironment(environment, unity_SpecCube0_HDR);
}

float4 SampleBakedShadows (float2 lightMapUV, const Surface surfaceWS)
{
#if defined(LIGHTMAP_ON)
    return SAMPLE_TEXTURE2D(unity_ShadowMask, samplerunity_ShadowMask, lightMapUV);
#else
    if (unity_ProbeVolumeParams.x)
    {
        return SampleProbeOcclusion(
            TEXTURE3D_ARGS(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH),
            surfaceWS.position, unity_ProbeVolumeWorldToObject,
            unity_ProbeVolumeParams.y, unity_ProbeVolumeParams.z,
            unity_ProbeVolumeMin.xyz, unity_ProbeVolumeSizeInv.xyz
        );
    }
    return unity_ProbesOcclusion;
#endif
}

#endif
