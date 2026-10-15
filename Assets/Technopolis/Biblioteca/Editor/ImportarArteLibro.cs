#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class ImportarArteLibro : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath != "Assets/Resources/Biblioteca/libro_antiguo.png") return;
        var textura = (TextureImporter)assetImporter;
        textura.textureType = TextureImporterType.Sprite;
        textura.spriteImportMode = SpriteImportMode.Single;
        textura.filterMode = FilterMode.Point;
        textura.mipmapEnabled = false;
        textura.alphaIsTransparency = true;
        textura.textureCompression = TextureImporterCompression.Uncompressed;
        textura.maxTextureSize = 2048;
    }
}
#endif
