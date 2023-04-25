Shader "Hidden/LensFlare"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Blend One One
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #include "Packages/net.boschingmachine.bmrp/Runtime/ShaderLibrary/Common.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 cPos : VAR_CENTER_POSITION;
                float2 uv : VAR_UV;
                float blend : VAR_BLEND;
                float depth : VAR_DEPTH;
                float cDepth : VAR_CENTER_DEPTH;
                uint id : VAR_ID;
                float value : VAR_BRIGHTNESS;
                float size : VAR_SIZE;
            };

            const static float3 positions[] =
            {
                float3(1.0, 1.0, 0.0),
                float3(-1.0, 1.0, 0.0),
                float3(-1.0, -1.0, 0.0),

                float3(-1.0, -1.0, 0.0),
                float3(1.0, -1.0, 0.0),
                float3(1.0, 1.0, 0.0),
            };

            const static float2 uvs[] =
            {
                float2(0.0, 0.0),
                float2(1.0, 0.0),
                float2(1.0, 1.0),

                float2(1.0, 1.0),
                float2(0.0, 1.0),
                float2(0.0, 0.0),
            };

            float _Size;

            float smoothstep(float x)
            {
                return clamp(x * x * x * (x * (6.0 * x - 15.0) + 10.0), 0.0, 1.0);
            }

            const static uint BIT_NOISE1 = 0xB5297A4D;
            const static uint BIT_NOISE2 = 0x68E31DA4;
            const static uint BIT_NOISE3 = 0x1B56C4E9;
            const static uint PRIME1 = 198491317;
            const static uint PRIME2 = 6542989;

            float rand(int x, uint seed = 0)
            {
                x *= BIT_NOISE1;
                x += seed;
                x ^= (x >> 8);
                x += BIT_NOISE2;
                x ^= (x << 8);
                x *= BIT_NOISE3;
                x ^= (x >> 8);

                return x / 2147483647.0;
            }

            float rand(int2 p, uint seed = 0)
            {
                return rand(p.x + p.y * PRIME1, seed);
            }

            float randF(float x, uint seed = 0)
            {
                int x1 = (int)x;
                int x2 = x1 + 1;
                float p = x - x1;

                float r1 = rand(x1, seed);
                float r2 = rand(x2, seed);
                return lerp(r1, r2, p);
            }

            float randF(float2 x, uint seed = 0)
            {
                int2 x1 = (int2)x;
                int2 x2 = x1 + 1;
                float2 px = x - x1;

                float r1 = rand(int2(x1.x, x1.y), seed);
                float r2 = rand(int2(x2.x, x1.y), seed);

                float r3 = rand(int2(x1.x, x2.y), seed);
                float r4 = rand(int2(x2.x, x2.y), seed);

                r1 = lerp(r1, r2, px.x);
                r3 = lerp(r3, r4, px.x);

                return lerp(r1, r3, px.y);
            }

            void centerFlare(uint vertexID, inout Varyings o, out float3 worldPos)
            {
                float3 dir1 = normalize(TransformObjectToWorld(0.0) - _WorldSpaceCameraPos);
                float3 dir2 = normalize(mul(unity_CameraToWorld, float4(0.0, 0.0, 1.0, 0.0)).xyz);

                float3 diff = dir1 - dir2;

                float f = clamp(1.0 - length(diff) / 0.1, 0.0, 1.0);
                f = smoothstep(f);

                o.value = 1.0;

                worldPos = TransformObjectToWorld(positions[vertexID % 6] * _Size * 0.5);
                o.pos = TransformWorldToHClip(worldPos);
                o.uv = uvs[vertexID % 6];
                o.blend = randF(length(diff) * 100.0);

                o.id = 0;
                o.depth = 0.0;
                o.cDepth = 0.0;
                o.cPos = 0.0;
                o.size = 1.0;
            }

            TEX(_MainTex)
            float _PixelSize;
            
            float2 texSize()
            {
                float aspect = _PixelSize / min(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                return _MainTex_TexelSize.zw * aspect;
            }

            float _Asymmetry;
            float _Length;
            
            uint _ChildCount;
            float _ChildSize;
            float _ChildSlope;
            
            void extraFlare(uint vertexID, inout Varyings o, out float3 worldPos)
            {
                o.id = vertexID / 6;
                float3 dir1 = normalize(TransformObjectToWorld(0.0) - _WorldSpaceCameraPos);
                float3 dir2 = normalize(mul(unity_CameraToWorld, float4(0.0, 0.0, 1.0, 0.0)).xyz);

                float3 diff = dir1 - dir2;
                float percent = max(o.id - 1, 0) / (float)(_ChildCount - 1.0);
                percent += (_Asymmetry - 1.0) / 2.0;
                percent = percent / (1 + abs(_Asymmetry));
                float swing = percent * _Length;
                float3 worldOffset = -diff * swing;

                float f = clamp(1.0 - length(diff) / 0.1, 0.0, 1.0);
                f = smoothstep(f);
                o.value = f * 0.1;

                float size = _Size * pow(2.0, percent * _ChildSlope) * _ChildSize;
                
                float3 offset = mul(UNITY_MATRIX_I_M, float4(worldOffset, 0.0)).xyz;
                float2 uvSize = _Size / texSize();
                offset.xy = round(offset.xy / uvSize) * uvSize;
                offset.z = 0.0;
                worldOffset = mul(UNITY_MATRIX_M, float4(offset, 0.0)).xyz;

                worldPos = TransformObjectToWorld(positions[vertexID % 6] * size) + worldOffset;
                o.pos = TransformWorldToHClip(worldPos);
                o.uv = uvs[vertexID % 6];
                o.blend = randF(length(diff) * 100.0);

                if (o.id % 2 == 0)
                {
                    o.uv.y = 1.0 - o.uv.y;
                }
                if (o.id % 3 == 0)
                {
                    o.uv.x = 1.0 - o.uv.x;
                }

                o.depth = 0.0;
                o.cDepth = 0.0;
                o.cPos = 0.0;
                o.size = size / (_Size * 0.5);
            }
            
            Varyings vert(uint vertexID : SV_VertexID)
            {
                Varyings o;
                float3 worldPos;
                if (vertexID < 6) centerFlare(vertexID, o, worldPos);
                else extraFlare(vertexID, o, worldPos);

                float3 cPosW = TransformObjectToWorld(0.0);
                float2 ndc = ComputeNormalizedDeviceCoordinates(cPosW, UNITY_MATRIX_VP);
                o.cPos = ndc * _ScreenParams.xy;

                o.depth = -mul(unity_MatrixV, float4(worldPos, 1.0)).z * _ProjectionParams.w;
                o.cDepth = -mul(unity_MatrixV, float4(cPosW, 1.0)).z * _ProjectionParams.w;

                return o;
            }

            float3 _Color;
            float _Value;

            TEX(_ExTex);
            
            float4 frag(Varyings i) : SV_TARGET
            {
                float4 weights = 0.0;
                for (int j = 0; j < 4; j++) weights[j] = max(1.0 - 3.0 * abs(j / 3.0 - i.blend), 0.0);

                float2 scale = texSize() * i.size;
                float2 uv = round(i.uv * scale) / scale;
                
                float3 baseColor = 0.0;
                baseColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(uv.x, uv.y)).rgb * weights[0];
                baseColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(1.0 - uv.x, uv.y)).rgb * weights[1];
                baseColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(uv.x, 1.0 - uv.y)).rgb * weights[2];
                baseColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(1.0 - uv.x, 1.0 - uv.y)).rgb * weights[3];

                float depth = SAMPLE_DEPTH(i.pos);
                float cDepth = SAMPLE_DEPTH(i.cPos);

                if (depth < i.depth && i.id == 0) discard;
                if (cDepth < i.cDepth) discard;
                return float4(baseColor * _Color * _Value * i.value, 1.0);
            }
            ENDHLSL
        }
    }
}