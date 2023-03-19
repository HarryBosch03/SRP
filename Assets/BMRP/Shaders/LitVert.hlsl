#ifndef CUSTOM_LIT_VERT_INCLUDED
#define CUSTOM_LIT_VERT_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

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

#endif