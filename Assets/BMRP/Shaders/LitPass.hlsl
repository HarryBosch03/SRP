#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float4 tangentOS : TANGENT;
    float2 baseUV : TEXCOORD0;
    float2 detailUV : TEXCOORD1;

    GI_ATTRIBUTE_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
	float3 positionWS : VAR_POSITION;
	float3 normalWS : VAR_NORMAL;
	float4 tangentWS : VAR_TANGENT;
    float2 baseUV : VAR_BASE_UV;
    float2 detailUV : VAR_DETAIL_UV;

    GI_VARYINGS_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitVert (Attributes i)
{
    Varyings o;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, output);
    TRANSFER_GI_DATA(i, o);
    
    o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
    o.positionCS = TransformWorldToHClip(o.positionWS);
    
    o.baseUV = TransformBaseUV(i.baseUV);
    o.detailUV = TransformDetailUV(i.detailUV);
    
    o.normalWS = TransformObjectToWorldNormal(i.normalOS);
    o.tangentWS = float4(TransformObjectToWorldDir(i.tangentOS.xyz), i.tangentOS.w);
    
    return o;
}

float4 LitFrag (Varyings i) : SV_TARGET 
{
    UNITY_SETUP_INSTANCE_ID(input);
    ClipLod(i.positionCS.xy, unity_LODFade.x);

    float4 base = GetBase(i.baseUV, i.detailUV);
    #if defined(_CLIPPING)
    clip(base.a - GetCutoff(i.baseUV));
    #endif

    float3 normalWS = normalize(i.normalWS);
    
    Surface surface;
    surface.position = i.positionWS;
    surface.normal = NormalTangentToWorld(GetNormalTS(i.baseUV, i.detailUV), normalWS, i.tangentWS);
    surface.interpolatedNormal = normalWS;
    surface.viewDirection = normalize(_WorldSpaceCameraPos - i.positionWS);
    surface.depth = -TransformWorldToView(i.positionWS).z;
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = GetMetallic(i.baseUV);
	surface.occlusion = GetOcclusion(i.baseUV);
    surface.roughness = GetRoughness(i.baseUV, i.detailUV);
    surface.fresnelStrength = GetFresnel(i.baseUV);
    surface.dither = InterleavedGradientNoise(i.positionCS.xy, 0);

#if defined(_PREMULTIPLY_ALPHA)
    BRDF brdf = GetBRDF(surface, true);
#else
    BRDF brdf = GetBRDF(surface);
#endif

    const GI gi = GetGI(GI_FRAGMENT_DATA(i), surface, brdf);
    float3 color = GetLighting(surface, brdf, gi);
    color += GetEmission(i.baseUV);
    return float4(color, surface.alpha);
}

#endif