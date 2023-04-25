Shader "BMRP/Post Process/BloomAdd"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Common.hlsl"

            TEX(_BloomBuffer1)

            float4 frag(v2f i) : SV_Target
            {
                float3 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                float3 bloom = SAMPLE_TEXTURE2D(_BloomBuffer1, sampler_BloomBuffer1, i.uv).rgb;

                return float4(base + bloom, 1.0);
            }
            ENDHLSL
        }
    }
}