Shader "BMRP/Post Process/Bloom"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaxV ("Max Vertical Resolution", int) = 2
		_Threshold ("Threshold", float) = 1.1
		_Strength ("Strength", float) = 2.0
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
			float _Threshold;
			float _Strength;

			static const int BlurSamples = 16;

			float Falloff (float2 uv)
			{
				float dist = length(uv) / BlurSamples;
				return pow(2.718, -4.0 * dist * dist);
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float3 base = tex2D(_MainTex, i.uv).rgb;

				float3 sample = 0.0;
				
				for (int j = 0; j < BlurSamples; j++)
				{
					for (int k = 0; k < BlurSamples; k++)
					{
						int x = j - BlurSamples / 2;
						int y = k - BlurSamples / 2;

						float aspect = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
						float2 res = float2(aspect * _MaxV, _MaxV);
						
						float2 offset = float2(x, y) / res;
						float2 uv = Downscale(i.uv + offset, _MaxV, _MainTex_TexelSize);
						float3 col = tex2D(_MainTex, uv).rgb * Falloff(float2(x, y));
						sample += max(col - _Threshold, 0.0) * _Strength;
					}
				}

				sample /= BlurSamples * BlurSamples;

				float3 col = base + sample;
				
				return float4(col, 1.0);
			}
			ENDHLSL
		}
	}
}