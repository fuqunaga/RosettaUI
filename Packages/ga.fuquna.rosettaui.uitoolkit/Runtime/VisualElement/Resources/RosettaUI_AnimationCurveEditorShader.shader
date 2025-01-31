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
            #include "SdfBezierSpline.cginc"
            #define LINE_WIDTH_PX 2.0f


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
            float4 _Resolution;
            float4 _OffsetZoom;

            struct spline_segment
            {
                float2 startPos;
                float2 startVel;
                float2 endPos;
                float2 endVel;
            };

            StructuredBuffer<spline_segment> _Spline;
            int _SegmentCount;

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

                // Curve
                float2x3 CurveUvToViewUv = {
                    1, 0, -_OffsetZoom.x,
                    0, 1, -_OffsetZoom.y,
                };
                CurveUvToViewUv *= _OffsetZoom.z;
                float2 ViewUvToPx = _Resolution.xy;

                float dist = INITIALLY_FAR;
                for (int idx = 0; idx < _SegmentCount; idx++)
                {
                    /* if startVel or endVel inf draw HV
                     * elif startVel or endVel -inf draw VH
                     * else draw curve
                     */
                    spline_segment seg = _Spline[idx];

                    const float2 p = ViewUvToPx * i.uv;
                    const float2 pStart = ViewUvToPx * mul(CurveUvToViewUv, float3(seg.startPos, 1));
                    const float2 vStart = ViewUvToPx * mul(CurveUvToViewUv, float3(seg.startVel, 0));
                    const float2 pEnd = ViewUvToPx * mul(CurveUvToViewUv, float3(seg.endPos, 1));
                    const float2 vEnd = ViewUvToPx * mul(CurveUvToViewUv, float3(seg.endVel, 0));

                    const bool isStartPosInf = any(isinf(seg.startVel) && seg.startVel > 0);
                    const bool isStartNegInf = any(isinf(seg.startVel) && seg.startVel < 0);
                    const bool isEndPosInf = any(isinf(seg.endVel) && seg.endVel > 0);
                    const bool isEndNegInf = any(isinf(seg.endVel) && seg.endVel < 0);

                    if (isStartPosInf || isStartNegInf || isEndPosInf || isEndNegInf)
                    {
                        const float2 mid = isStartPosInf || isEndPosInf
                                               ? float2(pEnd.x, pStart.y)
                                               : float2(pStart.x, pEnd.y);
                        dist = min(dist, sdSegment(p, pStart, mid));
                        dist = min(dist, sdSegment(p, mid, pEnd));
                    }
                    else
                    {
                        const float2 p0 = pStart;
                        const float2 p1 = pStart + vStart / 3.;
                        const float2 p2 = pEnd - vEnd / 3.;
                        const float2 p3 = pEnd;
                        dist = min(dist, CubicBezierSegmentSdfL2(p, p0, p1, p2, p3));
                    }
                }

                float4 col = 0;

                // Grid (Temp)
                if (abs(frac(uv.x - 0.5) - 0.5) < _Resolution.z * 2.0 / _OffsetZoom.z || abs(frac(uv.y - 0.5) - 0.5) < _Resolution.w * 2.0 / _OffsetZoom.z)
                {
                    col = float4(0.4, 0.4, 0.4, 0.5);
                }
                else if (abs(frac(uv.x * 10.0 - 0.5) - 0.5) < _Resolution.z * 10.0 / _OffsetZoom.z || abs(frac(uv.y * 10.0 - 0.5) - 0.5) < _Resolution.w * 10.0 / _OffsetZoom.z)
                {
                    col = float4(0.4, 0.4, 0.4, 0.5);
                }

                const float radius = LINE_WIDTH_PX * 0.5f;
                dist -= radius;
                col = lerp(float4(0, 1, 0, 1), col, smoothstep(-.5f, .5f, dist));

                return col;
            }
            ENDHLSL
        }
    }
}
