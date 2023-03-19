#ifndef CUSTOM_META_PASS_INCLUDED
#define CUSTOM_META_PASS_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
	float2 lightMapUV : TEXCOORD1;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
};

bool4 unity_MetaFragmentControl;
float unity_OneOverOutputBoost;
float unity_MaxOutputValue;

Varyings MetaPassVertex (Attributes i)
{
    Varyings o;
    i.positionOS.xy = i.lightMapUV * unity_LightmapST.xy + unity_LightmapST.zw;
    i.positionOS.z = i.positionOS.z > 0.0 ? FLT_MIN : 0.0;
    o.positionCS = TransformWorldToHClip(i.positionOS);
    o.baseUV = TransformBaseUV(i.baseUV);
    return o;
}

float4 MetaPassFragment (const Varyings i) : SV_TARGET
{
    float4 base = GetBase(i.baseUV);
    Surface surface;
    ZERO_INITIALIZE(Surface, surface);
    surface.color = base.rgb;
    surface.metallic = GetMetallic();
    surface.roughness = GetRoughness();
    BRDF brdf = GetBRDF(surface);
    
    float4 meta = 0.0;
    if (unity_MetaFragmentControl.x)
    {
        meta = float4(brdf.diffuse, 1.0);
        meta.rgb += brdf.specular * brdf.roughness * 0.5;
        meta.rgb = min(PositivePow(meta.rgb, unity_OneOverOutputBoost), unity_MaxOutputValue);
    }
    else if (unity_MetaFragmentControl.y)
    {
        meta = float4(GetEmission(i.baseUV), 1.0);
    }
    return meta;
}

#endif