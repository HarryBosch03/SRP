#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowCasterPassVertex (const Attributes i)
{
    Varyings o;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, o);
    const float3 positionWS = TransformObjectToWorld(i.positionOS);
    o.positionCS = TransformWorldToHClip(positionWS);

#if UNITY_REVERSED_Z
    o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    o.baseUV = i.baseUV * baseST.xy + baseST.zw;
    return o;
}

void ShadowCasterPassFragment (const Varyings i)
{
    UNITY_SETUP_INSTANCE_ID(input);
    ClipLod(i.positionCS.xy, unity_LODFade.x);

    const float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.baseUV);
    const float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base = baseMap * baseColor;
#if defined(_SHADOWS_CLIP)
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
#elif defined(_SHADOWS_DITHER)
    float dither = InterleavedGradientNoise(i.positionCS.xy, 0);
    clip(base.a - dither);
#endif
}

#endif