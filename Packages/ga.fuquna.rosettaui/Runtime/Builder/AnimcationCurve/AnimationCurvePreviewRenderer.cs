using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public static class AnimationCurvePreviewRenderer
    {
        private static class ShaderParam
        {
            public const string GridOnKeyword = "_GRID_ON";

            private const string Resolution = "_Resolution";
            private const string OffsetZoom = "_OffsetZoom";
            private const string SplineBuffer = "_Spline";
            private const string SegmentCount = "_SegmentCount";
            private const string GridParams = "_GridParams";
            
            public static readonly int ResolutionId = Shader.PropertyToID(Resolution);
            public static readonly int OffsetZoomId = Shader.PropertyToID(OffsetZoom);
            public static readonly int SplineBufferId = Shader.PropertyToID(SplineBuffer);
            public static readonly int SegmentCountId = Shader.PropertyToID(SegmentCount);
            public static readonly int GridParamsId = Shader.PropertyToID(GridParams);
        }
        
        private const string ShaderName = "RosettaUI_AnimationCurve";
        private const string CommandBufferName = nameof(AnimationCurvePreviewRenderer);
        private static CommandBuffer _commandBuffer;
        private static GraphicsBuffer _curveDataBuffer;
        private static Material _curveDrawMaterial;

        public struct CurvePreviewViewInfo
        {
            public Vector4 offsetZoom;
            public bool gridEnabled;
            public Vector4 gridParams;
        }

        /// <summary>
        /// RosettaUI_AnimationCurve.shader の spline_segment 構造体と同じデータ構造。
        /// </summary>
        private struct SplineSegmentData
        {
            public Vector2 startPos;
            public Vector2 startVel;
            public Vector2 endPos;
            public Vector2 endVel;
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
            var keys = animationCurve.keys;
            if (keys.Length < 2)
            {
                return 0;
            }
            
            var segmentCount = keys.Length - 1;
            
            var splineData = new NativeArray<SplineSegmentData>(
                segmentCount,
                Allocator.Temp
            );
            
            for (var i = 0; i < splineData.Length; ++i)
            {
                var startKey = keys[i];
                var endKey = keys[i+1];
                
                splineData[i] = new SplineSegmentData {
                    startPos = new Vector2(startKey.time, startKey.value),
                    endPos = new Vector2(endKey.time, endKey.value),
                    startVel = (endKey.time - startKey.time) * startKey.GetStartVel(),
                    endVel = (endKey.time - startKey.time) * endKey.GetEndVel()
                };
            }
            
            if (_curveDataBuffer == null || _curveDataBuffer.count < segmentCount)
            {
                _curveDataBuffer?.Dispose();
                _curveDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, segmentCount, Marshal.SizeOf<SplineSegmentData>());
            }

            cmdBuf.SetBufferData(_curveDataBuffer, splineData);
            
            splineData.Dispose();
            
            return segmentCount;
        }
        
        public static void Render(AnimationCurve animationCurve, RenderTexture targetTexture, CurvePreviewViewInfo viewInfo)
        {
            _commandBuffer ??= new CommandBuffer { name = CommandBufferName };
            _commandBuffer.Clear();
            
            var segmentCount = UpdateData(_commandBuffer, animationCurve);

            var width = targetTexture.width;
            var height = targetTexture.height;
            
            var resolution = new Vector4(
                width,
                height,
                1f / width,
                1f / height
            );
            
            _curveDrawMaterial ??= new Material(Resources.Load<Shader>(ShaderName));
            _curveDrawMaterial.SetVector(ShaderParam.ResolutionId, resolution);
            _curveDrawMaterial.SetVector(ShaderParam.OffsetZoomId, viewInfo.offsetZoom);
            _curveDrawMaterial.SetBuffer(ShaderParam.SplineBufferId, _curveDataBuffer);
            _curveDrawMaterial.SetInt(ShaderParam.SegmentCountId, segmentCount);

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
