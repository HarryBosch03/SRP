Shader "BMRP/Unlit"
{
    Properties 
    {
        [MainTexture]_BaseMap("Texture", 2D) = "white" {}
        [MainCol]_BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
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
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex UnlitVert
            #pragma fragment UnlitFrag

            #include "UnlitPass.hlsl"
            
            ENDHLSL
        }   
    }
    
    CustomEditor "BMRP.Editor.ShaderGUIWithPresets"
}