#pragma once

#include "../ShaderLibrary/Common.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

BUFFER_START

DEF_PROP(float4, _BaseMap_ST)
DEF_PROP(float4, _BaseColor)
DEF_PROP(float, _Cutoff)
DEF_PROP(float, _Wobble)
DEF_PROP(float, _Value)

BUFFER_END

struct Attributes
{
    float3 pos : POSITION;
    float2 uv : TEXCOORD0;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 pos : SV_POSITION;
    
    #ifdef _UVDISTORT
    noperspective
#endif
    float2 uv : VAR_BASE_UV;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings vert (Attributes i)
{
    Varyings o;
    
    UNITY_SETUP_INSTANCE_ID(i);
    
    float3 positionWS = TransformObjectToWorld(i.pos);
    o.pos = TransformWorldToHClip(positionWS);
    
    o.pos.xy = Wobble(o.pos.xy, GET_PROP(_Wobble));

    float4 baseST = GET_PROP(_BaseMap_ST);
    o.uv = i.uv * baseST.xy + baseST.zw;

    return o;
}

float4 frag (Varyings i) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(i);

    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
    float4 baseColor = baseMap * GET_PROP(_BaseColor);

    #ifdef _CLIPPING
    clip(baseColor.a - GET_PROP(_Cutoff));
    #endif
    
    return baseColor * (1 + _Value);
}