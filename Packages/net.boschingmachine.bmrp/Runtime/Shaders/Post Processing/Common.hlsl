#pragma once

#include "../../ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

float2 Downscale (float2 uv, float maxV, float4 texelSize)
{
    float aspect = texelSize.z / texelSize.w;
    float2 tRes = float2(aspect * maxV, maxV);
    return floor(uv * tRes) / tRes;
}

float3 dither (int2 pixel, float3 t)
{
    const int d[] =
        {
         0,  8,  2, 10,
        12,  4, 14,  6,
         3, 11,  1,  9,
        15,  7, 13,  5,
        };
    const int columns = 4;
    const float divisor = 16.0;
    
    float b = (d[pixel.x % columns + columns * (pixel.y % columns)]) / divisor;
    return t > b;
}

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

v2f vert (appdata v)
{
    v2f o;
    o.vertex = TransformWorldToHClip(v.vertex.xyz);
    o.uv = v.uv;
    return o;
}

sampler2D _MainTex;
float4 _MainTex_TexelSize;