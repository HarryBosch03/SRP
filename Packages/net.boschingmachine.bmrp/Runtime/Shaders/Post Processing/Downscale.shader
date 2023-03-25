Shader "BMRP/Post Process/Downscale"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaxV("Max Vertical Resolution", int) = 720
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

			int _MaxV;

			float4 frag (v2f i) : SV_Target
			{
				float3 col = tex2D(_MainTex, Downscale(i.uv, _MaxV, _MainTex_TexelSize)).rgb;

				return float4(col, 1.0);
			}
			ENDHLSL
		}
	}
}