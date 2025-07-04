using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace RosettaUI.Builder
{
    public class AnimationCurvePreviewRenderer : IDisposable
    {
        private Material _curveDrawMaterial = new(Resources.Load<Shader>("RosettaUI_AnimationCurve"));
        private CommandBuffer _commandBuffer = new() { name = "AnimationCurvePreview" };
        private GraphicsBuffer _curveDataBuffer;
        private int _numActiveSegments;

        public struct CurvePreviewViewInfo
        {
            public Vector2 resolution;
            public Vector4 offsetZoom;
            public Vector4 gridParams;
            public RenderTexture outputTexture;
        }

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private struct SplineSegmentData
        {
            public Vector2 StartPos;
            public Vector2 StartVel;

            public Vector2 EndPos;
            public Vector2 EndVel;
        }

        public void UpdateData(CommandBuffer cmdBuf, AnimationCurve animationCurve)
        {
            var segments = animationCurve.keys.Zip(animationCurve.keys.Skip(1), ValueTuple.Create);
            var splineData = segments.Select(seg => new SplineSegmentData {
                StartPos = new Vector2(seg.Item1.time, seg.Item1.value),
                EndPos = new Vector2(seg.Item2.time, seg.Item2.value),
                StartVel = (seg.Item2.time - seg.Item1.time) * seg.Item1.GetStartVel(),
                EndVel = (seg.Item2.time - seg.Item1.time) * seg.Item2.GetEndVel()
            }).ToArray();

            _numActiveSegments = splineData.Length;

            if (_curveDataBuffer == null || _curveDataBuffer.count < _numActiveSegments)
            {
                _curveDataBuffer?.Dispose();
                _curveDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Mathf.Max(1, _numActiveSegments), Marshal.SizeOf<SplineSegmentData>());
            }

            cmdBuf.SetBufferData(_curveDataBuffer, splineData);
        }

        public void DrawCurve(CommandBuffer cmdBuf, RenderTexture outputTexture)
        {
            _curveDrawMaterial.SetBuffer("_Spline", _curveDataBuffer);
            _curveDrawMaterial.SetInt("_SegmentCount", _numActiveSegments);
            cmdBuf.Blit(null, outputTexture, _curveDrawMaterial);
        }

        public void Render(AnimationCurve animationCurve, CurvePreviewViewInfo viewInfo)
        {
            _commandBuffer.Clear();
            UpdateData(_commandBuffer, animationCurve);

            var resolution = new Vector4(
                viewInfo.resolution.x,
                viewInfo.resolution.y,
                1f / viewInfo.resolution.x,
                1f / viewInfo.resolution.y
            );
              
            _curveDrawMaterial.SetVector("_Resolution", resolution);
            _curveDrawMaterial.SetVector("_OffsetZoom", viewInfo.offsetZoom);
            _curveDrawMaterial.SetVector("_GridParams", viewInfo.gridParams);

            DrawCurve(_commandBuffer, viewInfo.outputTexture);

            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        public void Dispose()
        {
            if (_curveDrawMaterial != null)
            {
                UnityEngine.Object.Destroy(_curveDrawMaterial);
                _curveDrawMaterial = null;
            }
            
            _commandBuffer?.Dispose();
            _commandBuffer = null;

            _curveDataBuffer?.Dispose();
            _curveDataBuffer = null;
        }
    }
}
