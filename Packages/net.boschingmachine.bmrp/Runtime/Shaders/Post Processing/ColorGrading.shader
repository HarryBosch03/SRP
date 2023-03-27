Shader "BMRP/Post Process/Color Grading"
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

            #include "../../Shaders/Post Processing/Common.hlsl"

            float _Exposure, _Contrast, _Temperature, _Tint;
            
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                col.rgb *= pow(2, _Exposure);
                col.rgb = (col.rgb * 2.0 - 1.0) * _Contrast * 0.5 + 0.5;
                col.rgb = white_balance(col.rgb, _Temperature, _Tint);
                return col;
            }
            ENDHLSL
        }
    }
}