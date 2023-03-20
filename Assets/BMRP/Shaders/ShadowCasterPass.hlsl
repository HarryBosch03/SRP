#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

bool _ShadowPancaking;

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

    if (_ShadowPancaking)
    {
#if UNITY_REVERSED_Z
        o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
        o.positionCS.z = max(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif
    }
    
    o.baseUV = TransformBaseUV(i.baseUV);
    return o;
}

void ShadowCasterPassFragment (const Varyings i)
{
    UNITY_SETUP_INSTANCE_ID(input);
    ClipLod(i.positionCS.xy, unity_LODFade.x);

    float4 base = GetBase(i.baseUV);
#if defined(_SHADOWS_CLIP)
    clip(base.a - GetCutoff(i.baseUV));
#elif defined(_SHADOWS_DITHER)
    float dither = InterleavedGradientNoise(i.positionCS.xy, 0);
    clip(base.a - dither);
#endif
}

#endif