Shader "BMRP/Post Process/CRT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Uplift("Uplift", float) = 1.2
        _Amount ("Amount", Range(0.0, 1.0)) = 0.3
        _Distortion("Distortion", Range(0.0, 1.0)) = 0.0
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
            float _Uplift;
            float _Amount;
            float _Distortion;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 suv = uv * 2.0 - 1.0;
                float innerCircle = 1.0;
                suv = asin(suv) * 0.636619772;
                suv = suv * 0.5 + 0.5;

                uv = lerp(uv, float2(suv.x, uv.y), _Distortion);
                
                fixed4 col = tex2D(_MainTex, uv);

                int2 rowColumn = floor(uv * _MainTex_TexelSize.zw);

                float invAmount = 1.0 - _Amount;
                
                col.r *= rowColumn.x % 4 == 0 ? 1.0 : invAmount;
                col.g *= rowColumn.x % 4 == 1 ? 1.0 : invAmount;
                col.b *= rowColumn.x % 4 == 2 ? 1.0 : invAmount;

                col *= rowColumn.x % 4 != 3 ? 1.0 : invAmount;
                col *= rowColumn.y % 4 != 0 ? 1.0 : invAmount;
                col *= _Uplift;
                
                return col;
            }
            ENDCG
        }
    }
}
