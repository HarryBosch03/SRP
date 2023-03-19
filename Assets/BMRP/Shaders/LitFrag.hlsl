#ifndef CUSTOM_LIT_FRAG_INCLUDED
#define CUSTOM_LIT_FRAG_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

float4 LitFrag (Varyings i) : SV_TARGET 
{
    UNITY_SETUP_INSTANCE_ID(input);
    ClipLod(i.positionCS.xy, unity_LODFade.x);

    float4 base = GetBase(i.baseUV, i.detailUV);
    #if defined(_CLIPPING)
    clip(base.a - GetCutoff(i.baseUV));
    #endif

    Surface surface;
    surface.position = i.positionWS;

    surface.normal = NormalTangentToWorld(GetNormalTS(i.baseUV, i.detailUV), i.normalWS, i.tangentWS);
    surface.interpolatedNormal = normalize(i.normalWS);
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