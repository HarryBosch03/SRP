#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitVert (Attributes i)
{
    Varyings o;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, output);

    const float3 positionWS = TransformObjectToWorld(i.positionOS.xyz);
    o.positionCS = TransformWorldToHClip(positionWS);
    
    o.baseUV = TransformBaseUV(i.baseUV);

    return o;
}

float4 UnlitFrag (Varyings i) : SV_TARGET 
{
    UNITY_SETUP_INSTANCE_ID(input);
    ClipLod(i.positionCS.xy, unity_LODFade.x);

    float4 base = GetBase(i.baseUV);
    #if defined(_CLIPPING)
    clip(base.a - GetCutoff(i.baseUV));
    #endif
    
    #if defined(_CLIPPING)
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif
    
    return base;
}

#endif