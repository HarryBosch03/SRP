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
		Tags { "RenderType"="Transparent" }
		Blend One One
		ZWrite Off
		
		HLSLPROGRAM

        #pragma target 5.0 
        
        #pragma vertex vert
        #pragma fragment frag

		struct Varyings
		{
			float4 pos : SV_POSITION;
			float2 uv : VAR_SCREEN_UV;
		};

		float4 positions[]
		{
			float4(-1.0, 1.0, 0.0, 0.0),
			float4(1.0, 1.0, 0.0, 0.0),
			float4(1.0, -1.0, 0.0, 0.0),
			float4(-1.0, -1.0, 0.0, 0.0),
		};

		float2 uvs[]
		{
			float2(0.0, 0.0),	
			float2(1.0, 0.0),	
			float2(1.0, 1.0),	
			float2(0.0, 1.0),	
		};

		Varyings vert (uint vertexID : SV_VertexID)
		{
			Varyings o;
			o.pos = positions[vertexID];
			o.uv = uvs[vertexID];
			return o;
		}
		
		ENDHLSL
	}
	FallBack "Diffuse"
}