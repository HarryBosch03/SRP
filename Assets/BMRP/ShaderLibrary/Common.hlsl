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