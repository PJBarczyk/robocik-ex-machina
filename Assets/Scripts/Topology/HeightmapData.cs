using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

static class TopologyData
{
    private static Texture2D _heightmap;

    public static Texture2D heightmap
    {
        get => _heightmap;
        set
        {
            _heightmap = value;
            
            var pixels = value.GetPixels();
            vertexCount = pixels.Length;
            submergedVertexCount = pixels.Count(c => c.r > 0);
        }
    }
    public static float area = 500;
    public static float depth = 1;

    public static int vertexCount;
    public static int submergedVertexCount;

    public static Texture2D Grayscalify(this Texture2D texture)
    {
        var grayscale = new Texture2D(texture.width, texture.height);
        grayscale.SetPixels(texture.GetPixels().Select(LuminosityGrayscale).ToArray());
        grayscale.Apply();

        return grayscale;
    }


    private static Func<Color, Color> LuminosityGrayscale => c =>
    {
        var value = .2126f * c.r + .7152f * c.g + .0722f * c.b;
        return new Color(value, value, value);
    };

    public static bool LoadFromBytes(byte[] data)
    {
        var raw = new Texture2D(1, 1);
        if (!raw.LoadImage(data)) return false;

        Debug.Log("Heightmap loaded! " + raw.width + " x " + raw.height);

        _heightmap = Grayscalify(raw);
        _heightmap.Apply();

        var pixels = _heightmap.GetPixels();
        vertexCount = pixels.Length;
        submergedVertexCount = pixels.Count(c => c.r > 0);
        
        return true;
    }
}