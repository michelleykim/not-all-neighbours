using UnityEditor;
using UnityEngine;
using System.Linq;

public class BuildCommand
{
    static string[] GetScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    // FAST LOCAL DEVELOPMENT BUILDS
    [MenuItem("Build/Dev Build Windows 64")]
    public static void BuildWindowsDevFast()
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Dev/NotAllNeighbours.exe";
        options.target = BuildTarget.StandaloneWindows64;

        // SPEED OPTIMIZATIONS FOR LOCAL BUILDS:
        options.options = BuildOptions.Development |           // Development mode
                         BuildOptions.AllowDebugging |        // Enable debugging
                         BuildOptions.CompressWithLz4;        // Fast compression (vs Lz4HC)

        Debug.Log("Building FAST development build...");
        BuildPipeline.BuildPlayer(options);
        Debug.Log("Dev build complete!");
    }

    [MenuItem("Build/Dev Build Linux 64")]
    public static void BuildLinuxDevFast()
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Dev/NotAllNeighbours.x86_64";
        options.target = BuildTarget.StandaloneLinux64;
        options.options = BuildOptions.Development |
                         BuildOptions.AllowDebugging |
                         BuildOptions.CompressWithLz4;

        Debug.Log("Building FAST Linux development build...");
        BuildPipeline.BuildPlayer(options);
        Debug.Log("Linux dev build complete!");
    }

    // PRODUCTION BUILDS (SLOWER BUT OPTIMIZED)
    [MenuItem("Build/Production Build Windows 64")]
    public static void BuildWindowsProduction()
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Production/NotAllNeighbours.exe";
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.CompressWithLz4HC;  // Better compression

        Debug.Log("Building production Windows build...");
        BuildPipeline.BuildPlayer(options);
        Debug.Log("Production build complete!");
    }

    [MenuItem("Build/Production Build Linux 64")]
    public static void BuildLinuxProduction()
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Production/NotAllNeighbours.x86_64";
        options.target = BuildTarget.StandaloneLinux64;
        options.options = BuildOptions.CompressWithLz4HC;

        Debug.Log("Building production Linux build...");
        BuildPipeline.BuildPlayer(options);
        Debug.Log("Linux production build complete!");
    }

    // Legacy method names for compatibility
    public static void BuildWindows64() => BuildWindowsDevFast();
    public static void BuildLinux64() => BuildLinuxDevFast();
}
