﻿using System;
using System.Collections.Generic;
using Aseprite.Utils;
using AsepriteImporter.Data;
using UnityEditor;
using UnityEngine;

namespace AsepriteImporter
{
    public class SpriteAtlasBuilder
    {
        private readonly Vector2Int spriteSize;
        private int padding = 1;

        
        public SpriteAtlasBuilder()
        {
            spriteSize = new Vector2Int(16, 16);
        }

        public SpriteAtlasBuilder(Vector2Int spriteSize, int padding = 1)
        {
            this.spriteSize = spriteSize;
            this.padding = padding;
        }

        public SpriteAtlasBuilder(int width, int height, int padding = 1)
        {
            spriteSize = new Vector2Int(width, height);
            this.padding = padding;
        }

        public Texture2D GenerateAtlas(Texture2D[] sprites, out AseFileSpriteImportData[] spriteImportData, bool baseTwo = true)
        {
            int cols, rows;

            CalculateColsRows(sprites.Length, spriteSize, out cols, out rows);

            return GenerateAtlas(sprites, cols, rows, out spriteImportData, baseTwo);
        }

        public Texture2D GenerateAtlas(Texture2D[] sprites, int cols, int rows, out AseFileSpriteImportData[] spriteImportData, bool baseTwo = false)
        {
            spriteImportData = new AseFileSpriteImportData[sprites.Length];

            var width = cols * spriteSize.x;
            var height = rows * spriteSize.y;

            width += padding * 2;
            height += padding * 2;

            if (baseTwo)
            {
                var baseTwoValue = CalculateNextBaseTwoValue(Math.Max(width, height));
                width = baseTwoValue;
                height = baseTwoValue;
            }

            var atlas = Texture2DUtil.CreateTransparentTexture(width, height);
            var index = 0;

            for (var row = 0; row < rows; ++row)
            {
                for (var col = 0; col < cols; ++col)
                {
                    Rect spriteRect = new Rect(col * spriteSize.x + padding, atlas.height - ((row + 1) * spriteSize.y) - padding, spriteSize.x, spriteSize.y);
                    Color[] colors = sprites[index].GetPixels();
                    atlas.SetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height, sprites[index].GetPixels());
                    atlas.Apply();

                    List<Vector2[]> outline = GenerateRectOutline(spriteRect);
                    spriteImportData[index] = CreateSpriteImportData(index.ToString(), spriteRect, outline);

                    index++;
                    if (index >= sprites.Length)
                        break;
                }
                if (index >= sprites.Length)
                    break;
            }

            return atlas;
        }

        public static List<Vector2Int[]> GenerateRectOutline(RectInt rect)
        {
            List<Vector2Int[]> outline = new List<Vector2Int[]>();

            outline.Add(new Vector2Int[] { new Vector2Int(rect.x, rect.y), new Vector2Int(rect.x, rect.yMax) });
            outline.Add(new Vector2Int[] { new Vector2Int(rect.x, rect.yMax), new Vector2Int(rect.xMax, rect.yMax) });
            outline.Add(new Vector2Int[] { new Vector2Int(rect.xMax, rect.yMax), new Vector2Int(rect.xMax, rect.y) });
            outline.Add(new Vector2Int[] { new Vector2Int(rect.xMax, rect.y), new Vector2Int(rect.x, rect.yMax) });

            return outline;
        }
        
        public static List<Vector2[]> GenerateRectOutline(Rect rect)
        {
            List<Vector2[]> outline = new List<Vector2[]>();

            outline.Add(new Vector2[] { new Vector2(rect.x, rect.y), new Vector2(rect.x, rect.yMax) });
            outline.Add(new Vector2[] { new Vector2(rect.x, rect.yMax), new Vector2(rect.xMax, rect.yMax) });
            outline.Add(new Vector2[] { new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.y) });
            outline.Add(new Vector2[] { new Vector2(rect.xMax, rect.y), new Vector2(rect.x, rect.yMax) });

            return outline;
        }

        private AseFileSpriteImportData CreateSpriteImportData(string name, Rect rect, List<Vector2[]> outline)
        {
            return new AseFileSpriteImportData()
            {
                alignment = SpriteAlignment.Center,
                border = Vector4.zero,
                name = name,
                outline = outline,
                pivot = new Vector2(0.5f, 0.5f),
                rect = rect,
                spriteID = GUID.Generate().ToString(),
                tessellationDetail = 0
            };
        }

        private static void CalculateColsRows(int spritesCount, Vector2 spriteSize, out int cols, out int rows)
        {
            float area = spriteSize.x * spriteSize.y * spritesCount;
            float sqrt = Mathf.Sqrt(area);

            cols = Mathf.CeilToInt(sqrt / spriteSize.x);
            rows = Mathf.CeilToInt(sqrt / spriteSize.y);
        }

        private static int CalculateNextBaseTwoValue(int value)
        {
            var exponent = 0;
            var baseTwo = 0;

            while (baseTwo < value)
            {
                baseTwo = (int)Math.Pow(2, exponent);
                exponent++;
            }

            return baseTwo;
        }
    }
}