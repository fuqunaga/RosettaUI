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
        private static GraphicsBuffer _curveDataBuffer;
        private static Material _curveDrawMaterial;

        public struct CurvePreviewViewInfo
        {
            public Vector4 offsetZoom; // xy: offset.xy, zw: zoom.xy
            public bool gridEnabled;
            public Vector4 gridParams; // xy: order of grid（range=10^order), zw: grid unit size
        }
        
        static AnimationCurvePreviewRenderer()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _commandBuffer?.Dispose();
                _commandBuffer = null;
                
                _curveDataBuffer?.Dispose();
                _curveDataBuffer = null;
                
                Object.DestroyImmediate(_curveDrawMaterial);
                _curveDrawMaterial = null;
            });
        }
        
        private static int UpdateData(CommandBuffer cmdBuf, AnimationCurve animationCurve)
        {
            if (animationCurve == null)
            {
                return 0;
            }
            
            var keys = animationCurve.keys;
            if (keys.Length < 2)
            {
                return 0;
            }
            
            var segmentCount = keys.Length - 1;
            
            var cubicBezierArray = new NativeArray<CubicBezierData>(
                segmentCount,
                Allocator.Temp
            );
            
            for (var i = 0; i < cubicBezierArray.Length; ++i)
            {
                cubicBezierArray[i] = AnimationCurveHelper.CalcCubicBeziers(keys[i], keys[i + 1]);
            }
            
            if (_curveDataBuffer == null || _curveDataBuffer.count < segmentCount)
            {
                _curveDataBuffer?.Dispose();
                _curveDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, segmentCount, Marshal.SizeOf<CubicBezierData>());
            }

            cmdBuf.SetBufferData(_curveDataBuffer, cubicBezierArray);
            
            cubicBezierArray.Dispose();
            
            return segmentCount;
        }
        
        public static void Render(AnimationCurve animationCurve, RenderTexture targetTexture, in CurvePreviewViewInfo viewInfo)
        {
            _commandBuffer ??= new CommandBuffer { name = CommandBufferName };
            _commandBuffer.Clear();
            
            var segmentCount = UpdateData(_commandBuffer, animationCurve);

            _curveDrawMaterial ??= new Material(Resources.Load<Shader>(ShaderName));
            _curveDrawMaterial.SetVector(ShaderParam.ResolutionId, new Vector2(targetTexture.width, targetTexture.height));
            _curveDrawMaterial.SetVector(ShaderParam.OffsetZoomId, viewInfo.offsetZoom);
            _curveDrawMaterial.SetBuffer(ShaderParam.SegmentBufferId, _curveDataBuffer);
            _curveDrawMaterial.SetInt(ShaderParam.SegmentCountId, segmentCount);
            _curveDrawMaterial.SetInt(ShaderParam.PreWrapModeId, (int)ToWrapModeForPreview(animationCurve.preWrapMode));
            _curveDrawMaterial.SetInt(ShaderParam.PostWrapModeId, (int)ToWrapModeForPreview(animationCurve.postWrapMode));
            

            if (viewInfo.gridEnabled)
            {
                _curveDrawMaterial.EnableKeyword(ShaderParam.GridOnKeyword);
                _curveDrawMaterial.SetVector(ShaderParam.GridParamsId, viewInfo.gridParams);
            }
            else
            {
                _curveDrawMaterial.DisableKeyword(ShaderParam.GridOnKeyword);
            }
            
            _commandBuffer.Blit(null, targetTexture, _curveDrawMaterial);
            
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }
    }
}
