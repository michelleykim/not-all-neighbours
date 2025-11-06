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

    static void DisableBurstForDevBuilds()
    {
        // Disable Burst compilation for faster dev builds
        EditorUserBuildSettings.SetPlatformSettings(
            BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneWindows64).ToString(),
            "EnableBurstCompilation",
            "false"
        );
        EditorUserBuildSettings.SetPlatformSettings(
            BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneLinux64).ToString(),
            "EnableBurstCompilation",
            "false"
        );
    }

    static void EnableBurstForProductionBuilds()
    {
        // Enable Burst compilation for production builds
        EditorUserBuildSettings.SetPlatformSettings(
            BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneWindows64).ToString(),
            "EnableBurstCompilation",
            "true"
        );
        EditorUserBuildSettings.SetPlatformSettings(
            BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneLinux64).ToString(),
            "EnableBurstCompilation",
            "true"
        );
    }

    // FAST LOCAL DEVELOPMENT BUILDS
    [MenuItem("Build/Dev Build Windows 64")]
    public static void BuildWindowsDevFast()
    {
        // Apply all dev build optimizations
        DisableBurstForDevBuilds();
        BuildOptimizer.OptimizeForDevBuild();
        BuildOptimizer.ConfigureShaderStripping(stripForSpeed: true);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Dev/NotAllNeighbours.exe";
        options.target = BuildTarget.StandaloneWindows64;

        // SPEED OPTIMIZATIONS FOR LOCAL BUILDS:
        options.options = BuildOptions.Development |           // Development mode
                         BuildOptions.AllowDebugging |        // Enable debugging
                         BuildOptions.CompressWithLz4 |       // Fast compression (vs Lz4HC)
                         BuildOptions.StrictMode;             // Skip sanity checks for speed

        Debug.Log("Building FAST development build (Burst disabled, optimizations applied)...");
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Dev build complete! Build size: {report.summary.totalSize / (1024*1024)}MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
    }

    [MenuItem("Build/Dev Build Linux 64")]
    public static void BuildLinuxDevFast()
    {
        // Apply all dev build optimizations
        DisableBurstForDevBuilds();
        BuildOptimizer.OptimizeForDevBuild();
        BuildOptimizer.ConfigureShaderStripping(stripForSpeed: true);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Dev/NotAllNeighbours.x86_64";
        options.target = BuildTarget.StandaloneLinux64;
        options.options = BuildOptions.Development |
                         BuildOptions.AllowDebugging |
                         BuildOptions.CompressWithLz4 |
                         BuildOptions.StrictMode;

        Debug.Log("Building FAST Linux development build (Burst disabled, optimizations applied)...");
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Linux dev build complete! Build size: {report.summary.totalSize / (1024*1024)}MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
    }

    // PRODUCTION BUILDS (SLOWER BUT OPTIMIZED)
    [MenuItem("Build/Production Build Windows 64")]
    public static void BuildWindowsProduction()
    {
        // Apply production optimizations
        EnableBurstForProductionBuilds();
        BuildOptimizer.OptimizeForProductionBuild();
        BuildOptimizer.ConfigureShaderStripping(stripForSpeed: false);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Production/NotAllNeighbours.exe";
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.CompressWithLz4HC;  // Better compression

        Debug.Log("Building production Windows build (Burst enabled, full optimizations)...");
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Production build complete! Build size: {report.summary.totalSize / (1024*1024)}MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
    }

    [MenuItem("Build/Production Build Linux 64")]
    public static void BuildLinuxProduction()
    {
        // Apply production optimizations
        EnableBurstForProductionBuilds();
        BuildOptimizer.OptimizeForProductionBuild();
        BuildOptimizer.ConfigureShaderStripping(stripForSpeed: false);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = GetScenes();
        options.locationPathName = "Builds/Production/NotAllNeighbours.x86_64";
        options.target = BuildTarget.StandaloneLinux64;
        options.options = BuildOptions.CompressWithLz4HC;

        Debug.Log("Building production Linux build (Burst enabled, full optimizations)...");
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Linux production build complete! Build size: {report.summary.totalSize / (1024*1024)}MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
    }

    // Legacy method names for compatibility
    public static void BuildWindows64() => BuildWindowsDevFast();
    public static void BuildLinux64() => BuildLinuxDevFast();
}
