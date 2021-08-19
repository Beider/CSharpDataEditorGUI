using Godot;
using System;
using System.Collections.Generic;

public static class Utils
{
    private static Dictionary<string, Texture> ImageLookup = new Dictionary<string, Texture>();

    public static Texture LoadTextureFromFile(string path)
    {
        if (!ImageLookup.ContainsKey(path))
        {
            StreamTexture streamTexture = (StreamTexture)ResourceLoader.Load(path);
            ImageLookup.Add(path, streamTexture);
        }
        return ImageLookup[path];
    }
}
