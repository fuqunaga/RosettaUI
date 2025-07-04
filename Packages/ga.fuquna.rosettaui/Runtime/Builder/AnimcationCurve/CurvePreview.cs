using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace RosettaUI.Builder
{
    public class CurvePreview : IDisposable
    {
        private CommandBuffer _commandBuffer = new() { name = "AnimationCurvePreview" };
        private Material _curveDrawMaterial = new(Resources.Load<Shader>("RosettaUI_AnimationCurveEditorShader"));
        private GraphicsBuffer _curveDataBuffer;
        private int _numActiveSegments;

        public struct CurvePreviewViewInfo
        {
            public Vector4 Resolution;
            public Vector4 OffsetZoom;
            public Vector4 GridParams;
            public RenderTexture OutputTexture;
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

            _curveDrawMaterial.SetVector("_Resolution", viewInfo.Resolution);
            _curveDrawMaterial.SetVector("_OffsetZoom", viewInfo.OffsetZoom);
            _curveDrawMaterial.SetVector("_GridParams", viewInfo.GridParams);

            DrawCurve(_commandBuffer, viewInfo.OutputTexture);

            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        public void Dispose()
        {
            if (_curveDrawMaterial != null)
            {
                Object.DestroyImmediate(_curveDrawMaterial);
                _curveDrawMaterial = null;
            }

            _commandBuffer?.Dispose();
            _commandBuffer = null;

            _curveDataBuffer?.Dispose();
            _curveDataBuffer = null;
        }
    }
}
