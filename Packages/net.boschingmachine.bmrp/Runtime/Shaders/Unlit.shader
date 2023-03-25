Shader "BMRP/Unlit"
{
    Properties 
	{
		[Toggle(_WOBBLE)] _DoWobble ("Vertex Wobble", Float) = 0
    	_Wobble ("Wobble Amount", Range(0, 1)) = 1.0
    	
		[Toggle(_UVDISTORT)] _AffineUVs ("Affine UV's", Float) = 0
		
		[MainTexture]_BaseMap("Texture", 2D) = "white" {}
		[MainColor]_BaseColor ("Color", Color) = (1, 1, 1, 1)
		_Value ("Brightness", float) = 0.0
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
	}
	
	SubShader 
	{
		Pass 
		{
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			
			HLSLPROGRAM

			#pragma shader_feature _CLIPPING

			#pragma shader_feature _WOBBLE
			#pragma shader_feature _UVDISTORT
            
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag

			#include "UnlitPass.hlsl"
			
			ENDHLSL
		}
	}
}