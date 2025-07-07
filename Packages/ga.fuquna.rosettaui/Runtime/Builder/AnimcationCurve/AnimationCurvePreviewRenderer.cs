using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace RosettaUI.Builder
{
    public static class AnimationCurvePreviewRenderer
    {
        private static class ShaderParam
        {
            public const string GridOnKeyword = "_GRID_ON";
        }
        
        
        private static readonly CommandBuffer CommandBuffer = new() { name = "AnimationCurvePreview" };
        private static Material _curveDrawMaterial;
        private static GraphicsBuffer _curveDataBuffer;

        public struct CurvePreviewViewInfo
        {
            public Vector2 resolution;
            public Vector4 offsetZoom;
            public bool gridEnabled;
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

        private static int UpdateData(CommandBuffer cmdBuf, AnimationCurve animationCurve)
        {
            var segments = animationCurve.keys.Zip(animationCurve.keys.Skip(1), ValueTuple.Create);
            var splineData = segments.Select(seg => new SplineSegmentData {
                StartPos = new Vector2(seg.Item1.time, seg.Item1.value),
                EndPos = new Vector2(seg.Item2.time, seg.Item2.value),
                StartVel = (seg.Item2.time - seg.Item1.time) * seg.Item1.GetStartVel(),
                EndVel = (seg.Item2.time - seg.Item1.time) * seg.Item2.GetEndVel()
            }).ToArray();

            var numActiveSegments = splineData.Length;

            if (_curveDataBuffer == null || _curveDataBuffer.count < numActiveSegments)
            {
                _curveDataBuffer?.Dispose();
                _curveDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Mathf.Max(1, numActiveSegments), Marshal.SizeOf<SplineSegmentData>());
            }

            cmdBuf.SetBufferData(_curveDataBuffer, splineData);
            
            return numActiveSegments;
        }
        
        public static void Render(AnimationCurve animationCurve, CurvePreviewViewInfo viewInfo)
        {
            if (_curveDrawMaterial == null)
            {
                _curveDrawMaterial = new Material(Resources.Load<Shader>("RosettaUI_AnimationCurve"));
            }
            
            CommandBuffer.Clear();
            var numActiveSegments = UpdateData(CommandBuffer, animationCurve);

            var resolution = new Vector4(
                viewInfo.resolution.x,
                viewInfo.resolution.y,
                1f / viewInfo.resolution.x,
                1f / viewInfo.resolution.y
            );
            
            _curveDrawMaterial.SetVector("_Resolution", resolution);
            _curveDrawMaterial.SetVector("_OffsetZoom", viewInfo.offsetZoom);
            _curveDrawMaterial.SetBuffer("_Spline", _curveDataBuffer);
            _curveDrawMaterial.SetInt("_SegmentCount", numActiveSegments);

            if (viewInfo.gridEnabled)
            {
                _curveDrawMaterial.EnableKeyword(ShaderParam.GridOnKeyword);
                _curveDrawMaterial.SetVector("_GridParams", viewInfo.gridParams);
            }
            else
            {
                _curveDrawMaterial.DisableKeyword(ShaderParam.GridOnKeyword);
            }
            
            CommandBuffer.Blit(null, viewInfo.outputTexture, _curveDrawMaterial);
            
            Graphics.ExecuteCommandBuffer(CommandBuffer);
        }
    }
}
