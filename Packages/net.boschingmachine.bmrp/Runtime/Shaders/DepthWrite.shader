Shader "Hidden/DepthWrite" 
{
	Properties { }
	SubShader 
	{
		Pass 
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../ShaderLibrary/Common.hlsl"
			
			struct Attributes
			{
				float4 pos : SV_POSITION;
			};

			struct Varyings
			{
				float4 pos : SV_POSITION;
			};

			Varyings vert (Attributes i)
			{
				Varyings o;
			    float3 positionWS = TransformObjectToWorld(i.pos);
			    o.pos = TransformWorldToHClip(positionWS);
				return o;
			}

			float4 frag (Varyings i) : SV_TARGET
			{
				return 1.0;	
			}
			
			ENDHLSL
		}
	}
}