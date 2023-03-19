#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

#define INPUT_PROP(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)

TEXTURE2D(_BaseMap);
TEXTURE2D(_MaskMap);
TEXTURE2D(_NormalMap);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _DetailMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Occlusion)
    UNITY_DEFINE_INSTANCED_PROP(float, _Roughness)
    UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _EmissionBrightness)
    UNITY_DEFINE_INSTANCED_PROP(float, _Fresnel)
    UNITY_DEFINE_INSTANCED_PROP(float, _DetailAlbedo)
    UNITY_DEFINE_INSTANCED_PROP(float, _DetailSmoothness)
    UNITY_DEFINE_INSTANCED_PROP(float, _NormalScale)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float2 TransformBaseUV (const float2 baseUV)
{
    float4 baseST = INPUT_PROP(_BaseMap_ST);
    return baseUV * baseST.xy + baseST.zw;
}

float2 TransformDetailUV (const float2 detailUV)
{
    float4 detailST = INPUT_PROP(_DetailMap_ST);
    return detailUV * detailST.xy + detailST.zw;
}

float4 GetDetail (const float2 detailUV)
{
    float4 map = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV);
    return map * 2.0 - 1.0;
}

float4 GetMask (const float2 baseUV)
{
    return SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, baseUV);
}

float4 GetBase (const float2 baseUV, const float2 detailUV = 0.0)
{
    float4 map = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
    float4 color = INPUT_PROP(_BaseColor);

    float detail = GetDetail(detailUV).r * INPUT_PROP(_DetailAlbedo);
    float mask = GetMask(baseUV).b;
    map.rgb = lerp(sqrt(map.rgb), detail < 0.0 ? 0.0 : 1.0, abs(detail) * mask);
    map.rgb *= map.rgb;
    
    return map * color;
}

float GetCutoff (const float2 baseUV)
{
    return INPUT_PROP(_Cutoff);
}

float GetMetallic (const float2 baseUV = 0.0)
{
    float metallic = INPUT_PROP(_Metallic);
    metallic *= GetMask(baseUV).r;
    return metallic;
}

float GetRoughness (const float2 baseUV = 0.0, const float2 detailUV = 0.0)
{
    float roughness = INPUT_PROP(_Roughness);
    roughness *= GetMask(baseUV).a;
    
    float detail = GetDetail(detailUV).b * INPUT_PROP(_DetailSmoothness);
    float mask = GetMask(baseUV).b;
    roughness = lerp(roughness, detail < 0.0 ? 0.0 : 1.0, abs(detail) * mask);
	
    return roughness;
}

float GetOcclusion (const float2 baseUV)
{
    float strength = INPUT_PROP(_Occlusion);
    float occlusion = GetMask(baseUV).g;
    occlusion = lerp(1.0, occlusion, strength);
    return occlusion;
}

float3 GetEmission (const float2 baseUV)
{
    float4 map = SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, baseUV);
    float4 color = INPUT_PROP(_EmissionColor);
    return map.rgb * color.rgb * INPUT_PROP(_EmissionBrightness);
}

float GetFresnel (const float2 baseUV)
{
    return INPUT_PROP(_Fresnel);
}

float3 GetNormalTS (const float2 baseUV)
{
    float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, baseUV);
    float scale = INPUT_PROP(_NormalScale);
    float3 normal = DecodeNormal(map, scale);
    return normal;
}

#endif