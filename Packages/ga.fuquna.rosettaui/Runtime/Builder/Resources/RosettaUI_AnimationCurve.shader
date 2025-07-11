Shader "Hidden/RosettaUI_AnimationCurve"
{
    Properties
    {
        [Toggle] _GRID("Grid", Integer) = 1
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile _ _GRID_ON
            #pragma vertex vert_img
            #pragma fragment Frag
            //#pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"
            #include "SdfBezierSpline.hlsl"
            #define EPSILON 0.00000001

            
            StructuredBuffer<float4> _Keyframes;
            float4 _Resolution;
            float4 _OffsetZoom;
            float4 _GridParams;

            struct spline_segment
            {
                float2 startPos;
                float2 startVel;
                float2 endPos;
                float2 endVel;
            };

            StructuredBuffer<spline_segment> _Spline;
            int _SegmentCount;

            static const float LineWidth = 2.0f;
            static const float4 LineColor = float4(0, 1, 0, 1);
            static const float4 YZeroLineColor = float4(0, 0, 0, 1);

            #ifdef _GRID_ON
            static const float4 MainGridColor = float4(0.6, 0.6, 0.6, 0.5);
            static const float4 GridColor = float4(0.4, 0.4, 0.4, 0.5);


            inline bool2 IsGrid(float2 uv, float2 gridUnit, float2 thickness)
            {
                return abs(frac(uv / gridUnit - 0.5) - 0.5) < _Resolution.zw / (gridUnit * _OffsetZoom.zw) * thickness;
            }
            #endif


            float4 Frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                

                // Curve
                float2x3 CurveUvToViewUv = {
                    1, 0, -_OffsetZoom.x,
                    0, 1, -_OffsetZoom.y,
                };
                float2x2 scaleMatrix = {
                    _OffsetZoom.z, 0,
                    0, _OffsetZoom.w
                };
                CurveUvToViewUv = mul(scaleMatrix, CurveUvToViewUv);
                
                float2 ViewUvToPx = _Resolution.xy;
                
                const float2 currentPx = ViewUvToPx * uv;

                float dist = INITIALLY_FAR;
                for (int idx = 0; idx < _SegmentCount; idx++)
                {
                    /* if startVel or endVel inf draw HV
                     * elif startVel or endVel -inf draw VH
                     * else draw curve
                     */
                    spline_segment seg = _Spline[idx];
                    
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
                        dist = min(dist, SdSegment(currentPx, pStart, mid));
                        dist = min(dist, SdSegment(currentPx, mid, pEnd));
                    }
                    else
                    {
                        const float2 p0 = pStart;
                        const float2 p1 = pStart + vStart; // Hermite curve 1/3 factor rolled into tangent value
                        const float2 p2 = pEnd - vEnd;
                        const float2 p3 = pEnd;
                        dist = min(dist, CubicBezierSegmentSdfL2(currentPx, p0, p1, p2, p3));
                    }
                }

                float4 col = 0;

                // Y=0 black line
                float yZeroPx = ViewUvToPx.y * mul(CurveUvToViewUv, float3(0, 0, 1)).y;
                float distFromYZero = abs(currentPx.y - yZeroPx);
                col = lerp(YZeroLineColor, col, smoothstep(0.0, LineWidth * 0.5, distFromYZero));
                
                #ifdef _GRID_ON
                // Grid
                uv = uv / _OffsetZoom.zw + _OffsetZoom.xy;
                
                bool2 isSubGrid = IsGrid(uv, _GridParams.zw * 0.1, 1.0) && frac(_GridParams) > 0.0;
                bool isGrid = any(IsGrid(uv, _GridParams.zw, 1.0));
                bool2 isMainGrid = IsGrid(uv, _GridParams.zw, 2.0 * cos(frac(_GridParams - EPSILON) * UNITY_PI * 0.5));
                col = isSubGrid.x ? float4(GridColor.rgb, 1.0 - frac(_GridParams.x)) : col;
                col = isSubGrid.y ? float4(GridColor.rgb, 1.0 - frac(_GridParams.y)) : col;
                col = isGrid ? GridColor : col;
                col = isMainGrid.x ? MainGridColor : col;
                col = isMainGrid.y ? MainGridColor : col;
                #endif

                // Line
                dist -= LineWidth * 0.5f;
                col = lerp(LineColor, col, smoothstep(-.5f, .5f, dist));

                return col;
            }
            ENDHLSL
        }
    }
}
