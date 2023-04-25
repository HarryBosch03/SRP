Shader "BMRP/Post Process/Bloom"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
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

            float _Threshold;
            float _Strength;
            float2 _Axis = float2(1.0, 0.0);

            static const float weights[] =
            {
                0.01621622, 0.05405405, 0.12162162, 0.19459459, 0.22702703,
                0.19459459, 0.12162162, 0.05405405, 0.01621622
            };

            float4 frag(v2f i) : SV_Target
            {
                float3 bloom = 0.0;
                for (int x = 0; x < 9; x++)
                {
                    int y = x - 4;
                    float weight = weights[x];
                    float2 offset = _Axis * y * _MainTex_TexelSize.xy;
                    float3 sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset).rgb;
                    bloom += max(sample - _Threshold, 0.0) * _Strength * weight;
                }

                return float4(bloom, 1.0);
            }
            ENDHLSL
        }
    }
}