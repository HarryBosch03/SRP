Shader "Hidden/LensFlare"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Value ("Brightness", float) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #include "Packages/net.boschingmachine.bmrp/Runtime/ShaderLibrary/Common.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : VAR_SCREEN_UV;
            };

            const static float3 positions[] =
            {
                float3(1.0, 1.0, 0.0),
                float3(-1.0, 1.0, 0.0),
                float3(-1.0, -1.0, 0.0),
                
                float3(-1.0, -1.0, 0.0),
                float3(1.0, -1.0, 0.0),
                float3(1.0, 1.0, 0.0),
            };

            const static float2 uvs[] =
            {
                float2(0.0, 0.0),
                float2(1.0, 0.0),
                float2(1.0, 1.0),
                
                float2(1.0, 1.0),
                float2(0.0, 1.0),
                float2(0.0, 0.0),
            };

            Varyings vert(uint vertexID : SV_VertexID)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(positions[vertexID]);
                o.pos = TransformWorldToHClip(worldPos);
                o.uv = uvs[vertexID];
                return o;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float3 _Color;
            float _Value;
            
            float4 frag(Varyings i) : SV_TARGET
            {
                return float4(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color * _Value, 1.0);
            }
            ENDHLSL
        }
    }
}