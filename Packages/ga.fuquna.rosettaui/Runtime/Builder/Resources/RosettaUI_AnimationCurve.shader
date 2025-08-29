Shader "Hidden/RosettaUI_AnimationCurve"
{
    Properties
    {
        _LineColor("Line Color", Color) = (0,1,0,1)
        [Toggle] _GRID("Grid", Integer) = 1
        [Toggle] _Wrap("Wrap", Integer) = 1
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile _ _GRID_ON
            #pragma multi_compile _ _WRAP_ON
            #pragma vertex vert_img
            #pragma fragment frag
            //#pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"
            #include "SdfBezierSpline.hlsl"
            
            static const float4 LineColorOnWrap = float4(0.4, 0.4, 0.4, 1);
            static const float4 YZeroLineColor = float4(0, 0, 0, 1);
            static const float LineWidth = 3.5; //[px]
            
            struct CubicBezierData
            {
                float2 p0;
                float2 p1;
                float2 p2;
                float2 p3;
            };

            StructuredBuffer<CubicBezierData> _SegmentBuffer;
            int _SegmentCount;

            float2 _Resolution; // x: width, y: height
            float4 _OffsetZoom; // xy: offset.xy, zw: zoom.xy

            float4 _LineColor = float4(0, 1, 0, 1);
            
            #ifdef _WRAP_ON

            //
            // enum WrapModeForPreview:
            //
            #define WrapMode_Loop (0)
            #define WrapMode_PingPong (1)
            #define WrapMode_Clamp (2)
            
            int _PreWrapMode; // 0: once, 1: loop, 2: pingpong　
            int _PostWrapMode; // 0: once, 1: loop, 2: pingpong
            
            #endif

            
            #ifdef _GRID_ON
            
            float4 _GridParams; // xy: order of grid（range=10^order), zw: grid unit size
            
            static const float4 GridColor = float4(0.25, 0.25, 0.25, 1);
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

            
            inline float CalcCurveDistance(float2 currentPx, float2x3 curveToPx)
            {
                float dist = INITIALLY_FAR;
                
                for (int idx = 0; idx < _SegmentCount; idx++)
                {
                    CubicBezierData segment = _SegmentBuffer[idx];
                    
                    const float2 p0 = mul(curveToPx, float3(segment.p0, 1)); // start pos
                    const float2 p1 = mul(curveToPx, float3(segment.p1, 1));
                    const float2 p2 = mul(curveToPx, float3(segment.p2, 1));
                    const float2 p3 = mul(curveToPx, float3(segment.p3, 1)); // end pos

                    const bool isStartInf = isinf(p1.y);
                    const bool isEndInf = isinf(p2.y);

                    // if p1 or p2 inf draw HV
                    // elif p1 or p2 -inf draw VH
                    // else draw curve
                    if (isStartInf || isEndInf)
                    {
                        const bool horizontalFirst = (isStartInf && (p1.y > 0)) || (isEndInf && (p2.y < 0));
                        const float2 mid = horizontalFirst
                                               ? float2(p3.x, p0.y)
                                               : float2(p0.x, p3.y);
                        
                        dist = min(dist, SdSegment(currentPx, p0, mid));
                        dist = min(dist, SdSegment(currentPx, mid, p3));
                    }
                    else
                    {
                        dist = min(dist, CubicBezierSegmentSdfL2(currentPx, p0, p1, p2, p3));
                    }
                }

                return dist;
            }

            
            #ifdef _WRAP_ON

            inline float RepeatPositionX(float currentX, int wrapMode, float splineWidth, float splineStartX)
            {
                float currentPxXFromStart = currentX - splineStartX;
                int cycleCount = floor(currentPxXFromStart / splineWidth);

                float pxOnWrap = currentPxXFromStart - splineWidth * cycleCount;
                if ((WrapMode_PingPong == wrapMode) && ((uint)abs(cycleCount) % 2 != 0))
                {
                    pxOnWrap = splineWidth - pxOnWrap;
                }

                return pxOnWrap + splineStartX;
            }

            inline float ClacCurveDistanceOfWrap(float2 currentPx, float2x3 curveToPx, float2 startPx, float2 endPx)
            {
                float dist = INITIALLY_FAR;
                float lineWidthHalf = LineWidth * 0.5f;
                
                // Wrapの範囲はlineWidthHalf分、本来のカーブ側に寄せる
                // Wrapカーブのラインがその幅分本来のカーブ領域にもはみ出るため
                const bool isInPreWrap = currentPx.x < (startPx.x + lineWidthHalf);
                const bool isInPostWrap = currentPx.x > (endPx.x - lineWidthHalf);

                if (isInPreWrap || isInPostWrap)
                {
                    float2 edgePx = isInPreWrap ? startPx : endPx;
                    int wrapMode = isInPreWrap ? _PreWrapMode : _PostWrapMode;
                    
                    float curveWidth = endPx.x - startPx.x;

                    // Keyframeが一つのときはstartPx==endPx
                    // UnityのAnimationCurveEditorの挙動を模倣してWrapMode_Clampとして扱う
                    if (WrapMode_Clamp == wrapMode || curveWidth <= 0)
                    {
                        dist = abs(currentPx.y - edgePx.y);
                    }
                    else
                    {
                        float offsetSign = isInPreWrap ? 1.0f : -1.0f;

                        float startX = startPx.x + offsetSign * lineWidthHalf;

                        currentPx.x = RepeatPositionX(currentPx.x, wrapMode, curveWidth, startX);

                        // LoopではWrapしたカーブとひとつ前のカーブを繋ぐ
                        if (WrapMode_Loop == wrapMode)
                        {
                            float2 edgePxOfNextWrap = edgePx + float2(offsetSign * curveWidth, 0);
                            float2 oppositeEdgePx = isInPreWrap ? endPx : startPx;
                            dist = SdSegment(currentPx, oppositeEdgePx, edgePxOfNextWrap);
                        }

                        dist = min(dist, CalcCurveDistance(currentPx, curveToPx));
                    }
                }

                return dist;
            }

            #endif
            

            // 座標系が３つある
            // - uv: 0-1
            // - px: pixel
            // - curve: x(time), y(value)
            float4 frag(v2f_img i) : SV_Target
            {
                const float2 uv = i.uv;
                
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


                float4 col = 0;

                
                #ifndef _GRID_ON
                
                // Y=0 black line
                // Gridとは排他的
                float yZeroPx = mul(curveToPx, float3(0, 0, 1)).y;
                float distFromYZero = abs(currentPx.y - yZeroPx);
                col = lerp(YZeroLineColor, col, smoothstep(0.0, LineWidth * 0.5, distFromYZero));
                
                #else
                
                // Grid
                const float2 currentOnCurve = uv / _OffsetZoom.zw + _OffsetZoom.xy;

                float gridAlphaRate = CalcGridAlphaRate(currentOnCurve, curveToPx);
                col = gridAlphaRate > 0 ? float4(GridColor.rgb, GridColor.a * gridAlphaRate) : col;
                
                #endif

                
                // CurveLine
                float2 startPx = mul(curveToPx, float3(_SegmentBuffer[0].p0, 1));
                float2 endPx = mul(curveToPx, float3(_SegmentBuffer[_SegmentCount - 1].p3, 1));

                #ifdef _WRAP_ON
                float distanceFromWrap = ClacCurveDistanceOfWrap(currentPx, curveToPx, startPx, endPx);
                float wrapLineRate = smoothstep(LineWidth * 0.5f, 0, distanceFromWrap);
                col = lerp(col, LineColorOnWrap, wrapLineRate);
                #endif

                float lineWidthHalf = LineWidth * 0.5f;
                if (((startPx.x - lineWidthHalf) <= currentPx.x) && (currentPx.x <= (endPx.x + lineWidthHalf)))
                {
                    float distanceFromCurve = CalcCurveDistance(currentPx, curveToPx);
                    float lineRate = smoothstep(LineWidth * 0.5f, 0, distanceFromCurve);
                    col = lerp(col, _LineColor, lineRate);
                }

                return col;
            }
            ENDHLSL
        }
    }
}
