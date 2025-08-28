using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public static class AnimationCurvePreviewRenderer
    {
        private enum WrapModeForPreview
        {
            Loop,
            PingPong,
            Clamp,
        }
        
        private static WrapModeForPreview ToWrapModeForPreview(WrapMode wrapMode)
        {
            return wrapMode switch
            {
                WrapMode.Loop => WrapModeForPreview.Loop,
                WrapMode.PingPong => WrapModeForPreview.PingPong,
                _ => WrapModeForPreview.Clamp,
            };
        }
        
        private static class ShaderParam
        {
            public const string WrapOnKeyword = "_WRAP_ON";
            public const string GridOnKeyword = "_GRID_ON";

            public static readonly int ResolutionId = Shader.PropertyToID("_Resolution");
            public static readonly int OffsetZoomId = Shader.PropertyToID("_OffsetZoom");
            public static readonly int SegmentBufferId = Shader.PropertyToID("_SegmentBuffer");
            public static readonly int SegmentCountId = Shader.PropertyToID("_SegmentCount");
            public static readonly int PreWrapModeId = Shader.PropertyToID("_PreWrapMode");
            public static readonly int PostWrapModeId = Shader.PropertyToID("_PostWrapMode");
            public static readonly int GridParamsId = Shader.PropertyToID("_GridParams");
        }
        
        
        private const string ShaderName = "RosettaUI_AnimationCurve";
        private const string CommandBufferName = nameof(AnimationCurvePreviewRenderer);
        private static CommandBuffer _commandBuffer;
        private static GraphicsBuffer _segmentBuffer;
        private static Material _curveDrawMaterial;

        public struct CurvePreviewViewInfo
        {
            public Vector4 offsetZoom; // xy: offset.xy, zw: zoom.xy
            public bool wrapEnabled;
            public bool gridEnabled;
            public Vector4 gridParams; // xy: order of grid（range=10^order), zw: grid unit size
        }
        
        static AnimationCurvePreviewRenderer()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _commandBuffer?.Dispose();
                _commandBuffer = null;
                
                _segmentBuffer?.Dispose();
                _segmentBuffer = null;
                
                Object.DestroyImmediate(_curveDrawMaterial);
                _curveDrawMaterial = null;
            });
        }
        
        private static int UpdateSegmentBuffer(AnimationCurve animationCurve)
        {
            if (animationCurve is null || animationCurve.length == 0)
            {
                return 0;
            }
            
            var keyframeCount = animationCurve.length;

            // セグメント数はキー数-1
            // キーフレームが１つのときは同一点でCubicBezierDataを作ってシェーダーに伝える
            var segmentCount = Mathf.Max(1, keyframeCount - 1);
            
            var cubicBezierArray = new NativeArray<CubicBezier>(
                segmentCount,
                Allocator.Temp
            );
            
            var keys = animationCurve.keys;
            if (keyframeCount == 1)
            {
                cubicBezierArray[0] = AnimationCurveHelper.CalcCubicBezierData(keys[0], keys[0]);
            }
            else
            {
                for (var i = 0; i < cubicBezierArray.Length; ++i)
                {
                    cubicBezierArray[i] = AnimationCurveHelper.CalcCubicBezierData(keys[i], keys[i + 1]);
                }
            }

            if (_segmentBuffer == null || _segmentBuffer.count < segmentCount)
            {
                _segmentBuffer?.Dispose();
                _segmentBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, segmentCount, Marshal.SizeOf<CubicBezier>());
            }

            _segmentBuffer.SetData(cubicBezierArray);
            
            cubicBezierArray.Dispose();
            
            return segmentCount;
        }
        
        public static void Render(AnimationCurve animationCurve, RenderTexture targetTexture, in CurvePreviewViewInfo viewInfo)
        {
            _commandBuffer ??= new CommandBuffer { name = CommandBufferName };
            _commandBuffer.Clear();
            
            var segmentCount = UpdateSegmentBuffer(animationCurve);

            _curveDrawMaterial ??= new Material(Resources.Load<Shader>(ShaderName));
            _curveDrawMaterial.SetVector(ShaderParam.ResolutionId, new Vector2(targetTexture.width, targetTexture.height));
            _curveDrawMaterial.SetVector(ShaderParam.OffsetZoomId, viewInfo.offsetZoom);
            _curveDrawMaterial.SetBuffer(ShaderParam.SegmentBufferId, _segmentBuffer);
            _curveDrawMaterial.SetInt(ShaderParam.SegmentCountId, segmentCount);
            _curveDrawMaterial.SetInt(ShaderParam.PreWrapModeId, (int)ToWrapModeForPreview(animationCurve.preWrapMode));
            _curveDrawMaterial.SetInt(ShaderParam.PostWrapModeId, (int)ToWrapModeForPreview(animationCurve.postWrapMode));
            
            SetKeyword(_curveDrawMaterial, ShaderParam.WrapOnKeyword, viewInfo.wrapEnabled);

            SetKeyword(_curveDrawMaterial, ShaderParam.GridOnKeyword, viewInfo.gridEnabled);
            if (viewInfo.gridEnabled)
            {
                _curveDrawMaterial.SetVector(ShaderParam.GridParamsId, viewInfo.gridParams);
            }
            
            _commandBuffer.Blit(null, targetTexture, _curveDrawMaterial);
            
            Graphics.ExecuteCommandBuffer(_commandBuffer);

            return;
            

            static void SetKeyword(Material m, string keyword, bool state)
            {
                if (state)
                {
                    m.EnableKeyword(keyword);
                }
                else
                {
                    m.DisableKeyword(keyword);
                }
            }
        }
    }
}
