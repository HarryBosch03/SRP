#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

#define UNITY_PREV_MATRIX_M unity_PrevObjectToWorld
#define UNITY_PREV_MATRIX_I_M unity_PrevWorldToObject

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#define DEF_PROP(t, n) UNITY_DEFINE_INSTANCED_PROP(t, n)
#define BUFFER_START UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
#define BUFFER_END UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define GET_PROP(n) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, n)

float _WobbleAmount = 6.0;

float2 Wobble (float2 pos, float amount)
{
#ifdef _WOBBLE
    if (_WobbleAmount == 0.0) return pos;
    
    amount *= _WobbleAmount;
    return round(pos * screenSize.xy * 0.5f / amount) * screenSize.zw * 2.0f * amount;
#else
    return pos;
#endif
}

float Square(float v)
{
    return v * v;
}

float3 dither (int2 pixel, float3 t)
{
    const int d[] =
        {
        0,  8,  2,  10,
        12, 4,  14, 6,
        3,  11, 1,  9,
        15, 7,  13, 5,
       };
    const int columns = 4;
    const float divisor = 16.0;
    
    float b = (d[pixel.x % columns + columns * (pixel.y % columns)]) / divisor;
    return t > b;
}

float3 ditherColor (float3 col, float2 pixelCoords)
{
    float3 lower = floor(col * 32) / 32;
    float3 upper = lower + (1.0 / 32.0);
    float3 diff = (col - lower) / (upper - lower);
    float3 d = dither(pixelCoords, diff);
    return lerp(lower, upper, d);
}

#define CLIP_DITHER \
float cutoff = GET_PROP(_Cutoff); \
float a = max((baseColor.a - 1.0) / (1.0f - cutoff) + 1.0f, 0.0f); \
if (cutoff >= 1.0) a = baseColor.a == 1.0f; \
clip(dither(i.pos.xy, pow(a, 2.0)).r * 2.0 - 1.0);

#define CLIP_OTHER \
float cutoff = GET_PROP(_Cutoff); \
clip(baseColor.a - cutoff);

#define DITHER_COLOR(col, _Texture) col.rgb = ditherColor(col.rgb, i.uv * _Texture ## _TexelSize.zw)

float SampleDepth(float2 uv)
{
    return Linear01Depth(SAMPLE_TEXTURE2D(_DepthTex, sampler_DepthTex, uv).r, _ZBufferParams);
}

#define SAMPLE_DEPTH(cpos) SampleDepth(cpos.xy / _ScreenParams.xy)