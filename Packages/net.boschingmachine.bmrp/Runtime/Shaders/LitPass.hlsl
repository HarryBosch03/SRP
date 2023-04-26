#pragma once

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

float4 _BaseMap_TexelSize;

BUFFER_START

DEF_PROP(float4, _BaseMap_ST)
DEF_PROP(float4, _BaseColor)
DEF_PROP(float, _Cutoff)
DEF_PROP(float, _Wobble)

BUFFER_END

#ifdef _UVDISTORT
#define AFFINE noperspective
#else
#define AFFINE
#endif

struct Attributes
{
    float3 pos : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    AFFINE
    float4 pos : SV_POSITION;
    AFFINE
    float2 uv : VAR_BASE_UV;
    AFFINE
    float4 color : VAR_COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings vert (Attributes i)
{
    Varyings o;
    
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, o);
    
    float3 positionWS = TransformObjectToWorld(i.pos);
    o.pos = TransformWorldToHClip(positionWS);
    
    float wobble = GET_PROP(_Wobble);
    o.pos.xy = Wobble(o.pos.xy / o.pos.w, wobble) * o.pos.w;
    
    float3 normal = TransformObjectToWorldNormal(i.normal);

    float4 baseST = GET_PROP(_BaseMap_ST);
    o.uv = i.uv * baseST.xy + baseST.zw;
    
    Surface surface;
    surface.position = positionWS;
    surface.normal = normal;
    surface.color = i.color;
    surface.viewDirection = normalize(_WorldSpaceCameraPos - positionWS);

    o.color = float4(GetLighting(surface), i.color.a);
    return o;
}

float4 frag (Varyings i) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(i);
    
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
    float4 baseColor = baseMap * GET_PROP(_BaseColor);
    baseColor.rgb = DITHER_COLOR(baseColor, _BaseMap) * i.color;

#ifdef _ALPHA_DITHER
   CLIP_DITHER
#elif defined(_CLIPPING)
    CLIP_OTHER
#endif
    
    return float4(baseColor);
}