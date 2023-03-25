Shader "BMRP/Post Process/Dither"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float3 lower = floor(col * 32) / 32;
                float3 upper = lower + (1.0 / 32.0);
                float3 diff = (col - lower) / (upper - lower);
                float3 d = dither(i.uv * _MainTex_TexelSize.zw, diff);

                col.rgb = lerp(lower, upper, d);

                return col;
            }
            ENDHLSL
        }
    }
}
