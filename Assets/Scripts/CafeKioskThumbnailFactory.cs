using System.Collections.Generic;
using UnityEngine;

public static class CafeKioskThumbnailFactory
{
    private static readonly Dictionary<string, Sprite> Sprites = new();

    public static Sprite Get(MenuItem item)
    {
        if (Sprites.TryGetValue(item.Name, out var sprite))
        {
            return sprite;
        }

        var texture = CreateTexture(item);
        texture.hideFlags = HideFlags.DontSave;
        texture.Apply();

        sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.DontSave;
        Sprites[item.Name] = sprite;
        return sprite;
    }

    private static Texture2D CreateTexture(MenuItem item)
    {
        const int width = 180;
        const int height = 92;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        var top = TopColor(item);
        var bottom = BottomColor(item);
        for (var y = 0; y < height; y++)
        {
            var t = y / (float)(height - 1);
            var color = Color.Lerp(bottom, top, t);
            for (var x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        DrawPlate(texture, width / 2, 24, 54, new Color(1f, 0.96f, 0.86f), new Color(0.74f, 0.66f, 0.56f));

        if (item.Category == "Coffee")
        {
            DrawCup(texture, 72, 30, new Color(0.36f, 0.2f, 0.11f), new Color(0.96f, 0.9f, 0.78f));
            DrawSteam(texture, 68, 60);
            DrawSteam(texture, 88, 58);
        }
        else if (item.Category == "Ade")
        {
            DrawGlass(texture, 72, 26, item.Name.Contains("레몬") ? new Color(0.98f, 0.86f, 0.24f) : new Color(0.96f, 0.38f, 0.28f));
            DrawCircle(texture, 106, 55, 12, new Color(1f, 0.95f, 0.7f));
        }
        else if (item.Category == "Dessert")
        {
            DrawCake(texture, 64, 25, item.Name.Contains("초콜릿") ? new Color(0.28f, 0.13f, 0.08f) : new Color(0.88f, 0.62f, 0.28f));
        }
        else
        {
            DrawSandwich(texture, 58, 28);
            DrawCircle(texture, 120, 43, 16, new Color(0.48f, 0.68f, 0.38f));
        }

        return texture;
    }

    private static Color TopColor(MenuItem item)
    {
        return item.Category switch
        {
            "Coffee" => new Color(0.7f, 0.48f, 0.3f),
            "Ade" => new Color(0.98f, 0.74f, 0.42f),
            "Dessert" => new Color(0.82f, 0.58f, 0.45f),
            "Food" => new Color(0.62f, 0.76f, 0.48f),
            _ => new Color(0.8f, 0.68f, 0.5f),
        };
    }

    private static Color BottomColor(MenuItem item)
    {
        return item.Category switch
        {
            "Coffee" => new Color(0.29f, 0.16f, 0.09f),
            "Ade" => new Color(0.46f, 0.67f, 0.7f),
            "Dessert" => new Color(0.5f, 0.3f, 0.22f),
            "Food" => new Color(0.28f, 0.43f, 0.28f),
            _ => new Color(0.4f, 0.32f, 0.24f),
        };
    }

    private static void DrawCup(Texture2D texture, int x, int y, Color coffeeColor, Color cupColor)
    {
        DrawRect(texture, x, y, 42, 34, cupColor);
        DrawRect(texture, x + 6, y + 25, 30, 7, coffeeColor);
        DrawRect(texture, x + 42, y + 10, 9, 18, cupColor);
    }

    private static void DrawGlass(Texture2D texture, int x, int y, Color drinkColor)
    {
        DrawRect(texture, x, y, 40, 44, new Color(0.92f, 0.98f, 1f, 0.86f));
        DrawRect(texture, x + 4, y + 6, 32, 28, drinkColor);
        DrawRect(texture, x + 13, y + 20, 7, 7, new Color(1f, 1f, 1f, 0.85f));
        DrawRect(texture, x + 24, y + 12, 7, 7, new Color(1f, 1f, 1f, 0.8f));
        DrawRect(texture, x + 30, y + 39, 4, 34, new Color(0.2f, 0.18f, 0.16f));
    }

    private static void DrawCake(Texture2D texture, int x, int y, Color cakeColor)
    {
        DrawRect(texture, x, y, 58, 32, cakeColor);
        DrawRect(texture, x, y + 21, 58, 8, new Color(1f, 0.84f, 0.64f));
        DrawRect(texture, x + 8, y + 9, 42, 5, new Color(0.96f, 0.72f, 0.52f));
        DrawCircle(texture, x + 46, y + 38, 7, new Color(0.82f, 0.12f, 0.12f));
    }

    private static void DrawSandwich(Texture2D texture, int x, int y)
    {
        DrawRect(texture, x, y, 70, 12, new Color(0.93f, 0.74f, 0.38f));
        DrawRect(texture, x + 4, y + 12, 62, 10, new Color(0.92f, 0.92f, 0.62f));
        DrawRect(texture, x + 8, y + 22, 54, 10, new Color(0.82f, 0.3f, 0.24f));
        DrawRect(texture, x + 2, y + 32, 66, 12, new Color(0.96f, 0.78f, 0.42f));
    }

    private static void DrawPlate(Texture2D texture, int cx, int cy, int radius, Color fill, Color shadow)
    {
        DrawEllipse(texture, cx + 4, cy - 2, radius, 16, shadow);
        DrawEllipse(texture, cx, cy, radius, 16, fill);
    }

    private static void DrawSteam(Texture2D texture, int x, int y)
    {
        for (var i = 0; i < 22; i++)
        {
            var px = x + Mathf.RoundToInt(Mathf.Sin(i * 0.45f) * 5f);
            DrawCircle(texture, px, y + i, 2, new Color(1f, 0.95f, 0.86f, 0.65f));
        }
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (var py = y; py < y + height; py++)
        {
            for (var px = x; px < x + width; px++)
            {
                SetPixelSafe(texture, px, py, color);
            }
        }
    }

    private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    SetPixelSafe(texture, cx + x, cy + y, color);
                }
            }
        }
    }

    private static void DrawEllipse(Texture2D texture, int cx, int cy, int radiusX, int radiusY, Color color)
    {
        for (var y = -radiusY; y <= radiusY; y++)
        {
            for (var x = -radiusX; x <= radiusX; x++)
            {
                var normalized = x * x / (float)(radiusX * radiusX) + y * y / (float)(radiusY * radiusY);
                if (normalized <= 1f)
                {
                    SetPixelSafe(texture, cx + x, cy + y, color);
                }
            }
        }
    }

    private static void SetPixelSafe(Texture2D texture, int x, int y, Color color)
    {
        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
        {
            return;
        }

        if (color.a < 1f)
        {
            color = Color.Lerp(texture.GetPixel(x, y), color, color.a);
            color.a = 1f;
        }

        texture.SetPixel(x, y, color);
    }
}
