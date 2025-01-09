Shader "Hidden/RosettaUI_AnimationCurveEditorShader"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"



            float EvaluateCurve(float t, float4 keyFrame0, float4 keyFrame1)
            {
                float dt = keyFrame1.x - keyFrame0.x;

                float m0 = keyFrame0.w * dt;
                float m1 = keyFrame1.z * dt;

                float t2 = t * t;
                float t3 = t2 * t;

                float a = 2 * t3 - 3 * t2 + 1;
                float b = t3 - 2 * t2 + t;
                float c = t3 - t2;
                float d = -2 * t3 + 3 * t2;

                return a * keyFrame0.y + b * m0 + c * m1 + d * keyFrame1.y;
            }

            float CurveDeriv(float t, float4 keyFrame0, float4 keyFrame1)
            {
                float dt = keyFrame1.x - keyFrame0.x;

                float m0 = keyFrame0.w * dt;
                float m1 = keyFrame1.z * dt;

                float t2 = t * t;
                float t3 = t2 * t;

                float a = 6 * t2 - 6 * t;
                float b = 3 * t2 - 4 * t + 1;
                float c = 3 * t2 - 2 * t;
                float d = -6 * t2 + 6 * t;

                return a * keyFrame0.y + b * m0 + c * m1 + d * keyFrame1.y;
            }

            // float EvaluateCurveSDF(float2 p, float4 keyFrame0, float4 keyFrame1)
            // {
            //     float2 b0 = keyFrame0.xy;
            //     float2 b1 = keyFrame0.xy + float2(1.0, keyFrame0.w) / 3.0;
            //     float2 b2 = keyFrame1.xy - float2(1.0, keyFrame1.z) / 3.0;
            //     float2 b3 = keyFrame1.xy;
            //     
            // }

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

            StructuredBuffer<float4> _Keyframes;
            int _KeyframesCount;
            float4 _Resolution;
            float4 _OffsetZoom;

            v2f Vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv = uv / _OffsetZoom.z + _OffsetZoom.xy;

                // Curve (Temp)
                for (int idx = 0; idx < _KeyframesCount - 1; idx++)
                {
                    float4 keyFrame0 = _Keyframes[idx];
                    float4 keyFrame1 = _Keyframes[idx + 1];

                    float t0 = keyFrame0.x;
                    float t1 = keyFrame1.x;
                    float t = (uv.x - t0) / (t1 - t0);
                    if (0.0 <= t && t <= 1.0)
                    {
                        float y = EvaluateCurve(t, keyFrame0, keyFrame1);
                        float dy = CurveDeriv(t, keyFrame0, keyFrame1);

                        if (abs(y - uv.y) < _Resolution.w * 1.5 * sqrt(1 + dy * dy) / _OffsetZoom.z)
                        {
                            return float4(0.0, 1.0, 0.0, 1.0);
                        }
                    }
                }

                // Grid (Temp)
                if (abs(frac(uv.x - 0.5) - 0.5) < _Resolution.z * 2.0 / _OffsetZoom.z || abs(frac(uv.y - 0.5) - 0.5) < _Resolution.w * 2.0 / _OffsetZoom.z)
                {
                    return float4(0.4, 0.4, 0.4, 0.5);
                }
                if (abs(frac(uv.x * 10.0 - 0.5) - 0.5) < _Resolution.z * 10.0 / _OffsetZoom.z || abs(frac(uv.y * 10.0 - 0.5) - 0.5) < _Resolution.w * 10.0 / _OffsetZoom.z)
                {
                    return float4(0.4, 0.4, 0.4, 0.5);
                }

                
                // float4 col = tex2D(_MainTex, i.uv);
                // // just invert the colors
                // col = 1 - col;
                return 0.0;
            }
            ENDHLSL
        }
    }
}