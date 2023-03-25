Shader "BMRP/Post Process/ACES"
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

            #include "../../Shaders/Post Processing/Common.hlsl"


            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return float4(AcesTonemap(unity_to_ACES(col.rgb)), col.a);
            }
            ENDHLSL
        }
    }
}