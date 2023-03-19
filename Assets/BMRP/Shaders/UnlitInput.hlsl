#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Brightness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float2 TransformBaseUV (float2 baseUV)
{
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    return baseUV * baseST.xy + baseST.zw;
}

float4 GetBase (float2 baseUV)
{
    float4 map = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
    float4 color = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    return map * color * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Brightness);
}

float GetCutoff (float2 baseUV = 0.0)
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff);
}

float GetMetallic (float2 baseUV = 0.0)
{
    return 0.0;
}

float GetRoughness (const float2 baseUV = 0.0, const float2 detailUV = 0.0)
{
    return 0.0;
}

float GetOcclusion (const float2 baseUV = 0.0)
{
    return 0.0;
}

float3 GetEmission (const float2 baseUV)
{
    return GetBase(baseUV).rgb;
}

#endif