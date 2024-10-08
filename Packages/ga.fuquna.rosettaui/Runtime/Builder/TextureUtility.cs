﻿using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Builder
{
    public enum CheckerboardTheme
    {
        Light,
        Dark
    }
    
    public static class TextureUtility
    {
        private static readonly Dictionary<CheckerboardTheme, Texture2D> CheckerBoardTexture2X2S = new();
        
        public static readonly Dictionary<CheckerboardTheme, (Color, Color)> CheckerBoardThemeColors = new()
        {
            {
                CheckerboardTheme.Light,
                (
                    Color.white,
                    Color.HSVToRGB(0f, 0f, 0.8f
                    )
                )
            },
            {
                CheckerboardTheme.Dark,
                (
                    new Color(0.427451f, 0.427451f, 0.427451f), // #6D6D6D
                    new Color(0.5960785f, 0.5960785f, 0.5960785f) // #989898;
                )
            }
        };

        static TextureUtility()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                foreach (var texture in CheckerBoardTexture2X2S.Values)
                {
                    UnityEngine.Object.Destroy(texture);
                }
                CheckerBoardTexture2X2S.Clear();
            });
        }
        
        // ReSharper disable once InconsistentNaming
        public static Texture2D GetOrCreateCheckerboardTexture2x2(CheckerboardTheme theme)
        {
            if (!CheckerBoardTexture2X2S.TryGetValue(theme, out var texture))
            {
                CheckerBoardTexture2X2S[theme] = texture = CreateCheckerBoardTexture(new Vector2Int(2, 2), 1, CheckerBoardThemeColors[theme].Item1, CheckerBoardThemeColors[theme].Item2);
            }

            return texture;
        }
        
        public static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize)
            => CreateCheckerBoardTexture(size, gridSize, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));
        
        public static Texture2D CreateCheckerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
        {
            var tex = CreateTexture(size.x, size.y, TextureFormat.RGB24);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            var colorArray = ArrayPool<Color>.Shared.Rent(size.x * size.y);
            var colors = colorArray.AsSpan();

            for (var y = 0; y < size.y; y++)
            {
                var flagY = ((y / gridSize) % 2 == 0);
                for (var x = 0; x < size.x; x++)
                {
                    var flagX = ((x / gridSize) % 2 == 0);
                    colors[x + y * size.x] = (flagX ^ flagY) ? col0 : col1;
                }
            }

            tex.SetPixels(colorArray);
            ArrayPool<Color>.Shared.Return(colorArray);

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateTexture(int width, int height, TextureFormat format = TextureFormat.RGBA32)
        {
            return new Texture2D(width, height, format, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
        }
    }
}