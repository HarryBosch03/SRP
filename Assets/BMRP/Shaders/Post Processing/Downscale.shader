Shader "BMRP/Post Process/Downscale"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Fac ("Downscale Factor", int) = 2
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Common.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			int _Fac;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 col = tex2D(_MainTex, Downscale(i.uv, _MainTex_TexelSize.zw, _Fac));

				return float4(col, 1.0);
			}
			ENDCG
		}
	}
}