using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;

public static class ConstruirPerfiladoNivel3
{
    public static void Ejecutar()
    {
        string output = Environment.GetEnvironmentVariable("NIVEL3_PROFILE_BUILD");
        if (string.IsNullOrEmpty(output)) throw new Exception("NIVEL3_PROFILE_BUILD missing");
        bool timing = PlayerSettings.enableFrameTimingStats;
        bool automatic = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64);
        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
        try
        {
            PlayerSettings.enableFrameTimingStats = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
            var opciones = new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/EscenaNivel3Silicio.unity", "Assets/Scenes/ESCENA NUEVA.unity" },
                locationPathName = Path.Combine(output, "TechnopolisPerfilado.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            BuildReport informe = BuildPipeline.BuildPlayer(opciones);
            if (informe.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + informe.summary.result);
        }
        finally
        {
            PlayerSettings.enableFrameTimingStats = timing;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, apis);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, automatic);
        }
    }
}
