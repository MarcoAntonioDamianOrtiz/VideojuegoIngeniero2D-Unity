// TECHNOPOLIS Nivel 3 - ajustes automaticos de sprites
// Compatible con Unity 6 / 6000.3.x.
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public sealed class TechnopolisNivel3ImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string ruta = assetPath.Replace('\\', '/');
        bool esPaquete = ruta.StartsWith("Assets/Technopolis_Nivel3/Sprites/", StringComparison.OrdinalIgnoreCase) ||
                         ruta.StartsWith("Assets/Technopolis/nivel3/Sprites/", StringComparison.OrdinalIgnoreCase);
        if (!esPaquete) return;

        var t = (TextureImporter)assetImporter;
        bool isGround = ruta.Contains("/Terreno/") || ruta.Contains("/Marca_");

        t.textureType = TextureImporterType.Sprite;
        t.spriteImportMode = SpriteImportMode.Single;
        t.spritePixelsPerUnit = 96;
        t.filterMode = FilterMode.Point;
        t.mipmapEnabled = false;
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.wrapMode = TextureWrapMode.Clamp;
        t.alphaIsTransparency = true;

        // TextureImporter.spriteAlignment NO existe en Unity 6000.3.
        // Esta propiedad pertenece a TextureImporterSettings.
        var spriteSettings = new TextureImporterSettings();
        t.ReadTextureSettings(spriteSettings);
        spriteSettings.spriteAlignment = isGround ? (int)SpriteAlignment.Center : (int)SpriteAlignment.Custom;
        spriteSettings.spritePivot = isGround ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);
        t.SetTextureSettings(spriteSettings);
    }
}
#endif
