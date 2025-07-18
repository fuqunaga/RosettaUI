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

            
            float2 _Resolution; // x: width, y: height
            float4 _OffsetZoom; // xy: offset.xy, zw: zoom.xy
            
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
            
            float4 _GridParams; // xy: order of grid（range=10^order), zw: grid unit size
            
            static const float4 GridColor = float4(0.4, 0.4, 0.4, 1);
            static const float GridWidth = 1;

            inline float2 CalcDistanceFromGridOnPx(float2 positionOnCurve, float2 gridUnit, float2x3 curveToPx)
            {
                float2 distanceRate = abs(frac(positionOnCurve / gridUnit - 0.5) - 0.5); // 0: on grid, 0.5: far from grid by 0.5 grid unit
                float2 distanceOnCurve = distanceRate * gridUnit;
                return mul(curveToPx, float3(distanceOnCurve, 0)).xy; // convert to pixel distance
            }

            inline bool2 IsInGrid(float2 positionOnCurve, float2 gridUnit, float2x3 curveToPx)
            {
                float2 distanceFromGrid = CalcDistanceFromGridOnPx(positionOnCurve, gridUnit, curveToPx);
                return distanceFromGrid <= GridWidth;
            }
            
            inline float CalcGridAlphaRate(float2 positionOnCurve, float2x3 curveToPx)
            {
                const float2 gridOrder = _GridParams.xy; // range = 10^order
                const float2 gridUnit = _GridParams.zw;

                const float2 currentOrderRate = (1).xx - frac(gridOrder); // 0なら現在のgridOrder、１に近いとほぼ次の桁
                const float subGridAlphaRate = 0.5f;

                bool2 isInSubGrid = IsInGrid(positionOnCurve, gridUnit * 0.1, curveToPx);
                bool2 isInGrid = IsInGrid(positionOnCurve, gridUnit, curveToPx);
                
                float2 subGridAlphaRateXy = isInSubGrid * currentOrderRate * subGridAlphaRate;
                //　currentOrderRateが0と１付近でSubGridと入れ替わるのでSubGridと同じ値になるように補間
                // 0.5で最大値1、0と1で最小値0
                float2 gridAlphaPeakRateXy = 1.0 - pow(2.0 * abs(currentOrderRate - 0.5), 3.0);
                float2 gridAlphaRateXy = isInGrid * lerp(subGridAlphaRate, 1,  gridAlphaPeakRateXy);  
                
                return max(max(subGridAlphaRateXy.x, subGridAlphaRateXy.y), max(gridAlphaRateXy.x, gridAlphaRateXy.y));
            }
            
            #endif


            // 座標系が３つある
            // - uv: 0-1
            // - px: pixel
            // - curve: x(time), y(value)
            float4 Frag(v2f_img i) : SV_Target
            {
                const float2 uv = i.uv;
                
                // Curve
                const float2x3 offsetMatrix = {
                    1, 0, -_OffsetZoom.x,
                    0, 1, -_OffsetZoom.y,
                };
                const float2x2 scaleMatrix = {
                    _OffsetZoom.z, 0,
                    0, _OffsetZoom.w
                };
                const float2x2 uvToPxMatrix = {
                    _Resolution.x, 0,
                    0, _Resolution.y
                };
                const float2x3 curveToPx = mul(uvToPxMatrix, mul(scaleMatrix, offsetMatrix));
                
                const float2 currentPx = mul(uvToPxMatrix, uv);

                
                float dist = INITIALLY_FAR;
                for (int idx = 0; idx < _SegmentCount; idx++)
                {
                    /* if startVel or endVel inf draw HV
                     * elif startVel or endVel -inf draw VH
                     * else draw curve
                     */
                    spline_segment seg = _Spline[idx];
                    
                    const float2 pStart = mul(curveToPx, float3(seg.startPos, 1));
                    const float2 vStart = mul(curveToPx, float3(seg.startVel, 0));
                    const float2 pEnd = mul(curveToPx, float3(seg.endPos, 1));
                    const float2 vEnd = mul(curveToPx, float3(seg.endVel, 0));

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
                float yZeroPx = mul(curveToPx, float3(0, 0, 1)).y;
                float distFromYZero = abs(currentPx.y - yZeroPx);
                col = lerp(YZeroLineColor, col, smoothstep(0.0, LineWidth * 0.5, distFromYZero));

                
                // Grid
                #ifdef _GRID_ON
                
                const float2 currentOnCurve = uv / _OffsetZoom.zw + _OffsetZoom.xy;

                float gridAlphaRate = CalcGridAlphaRate(currentOnCurve, curveToPx);
                col = gridAlphaRate > 0 ? float4(GridColor.rgb, GridColor.a * gridAlphaRate) : col;
                
                #endif

                
                // CurveLine
                dist -= LineWidth * 0.5f;
                col = lerp(LineColor, col, smoothstep(-.5f, .5f, dist));

                return col;
            }
            ENDHLSL
        }
    }
}
