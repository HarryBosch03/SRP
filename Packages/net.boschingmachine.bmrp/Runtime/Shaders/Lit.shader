Shader "BMRP/Lit"
{
    Properties
    {
		[Toggle(_WOBBLE)] _DoWobble ("Vertex Wobble", Float) = 1
    	_Wobble ("Wobble Amount", Range(0, 1)) = 1.0
    	
		[Toggle(_UVDISTORT)] _AffineUVs ("Affine UV's", Float) = 0
    	
		[MainTexture]_BaseMap("Texture", 2D) = "white" {}
		[MainColor]_BaseColor ("Color", Color) = (1, 1, 1, 1)
    	_SpecAmount ("Specular Brightness", float) = 0.5
    	_SpecExp ("Specular Exponent", float) = 50.0
		[Toggle(_ALPHA_DITHER)] _AlphaDither ("Alpha Dithering", Float) = 0
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
        	
			Tags 
            {
				"LightMode" = "BMLit"
			}
            
            HLSLPROGRAM

            #pragma target 5.0 
			#pragma shader_feature _WOBBLE
			#pragma shader_feature _UVDISTORT
			#pragma shader_feature _CLIPPING
			#pragma shader_feature _ALPHA_DITHER
            
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "LitPass.hlsl"
            
            ENDHLSL
        }   
    }
}
