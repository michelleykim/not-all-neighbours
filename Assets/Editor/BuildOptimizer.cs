using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Build optimizer that configures Unity settings for faster development builds
/// </summary>
public class BuildOptimizer
{
    /// <summary>
    /// Configure settings for fastest possible development builds
    /// </summary>
    public static void OptimizeForDevBuild()
    {
        Debug.Log("Optimizing build settings for fast development builds...");

        // Disable shader stripping for faster builds (less analysis needed)
        PlayerSettings.stripEngineCode = false;

        // Enable managed code stripping at minimal level for dev builds
        PlayerSettings.SetManagedStrippingLevel(
            BuildTargetGroup.Standalone,
            ManagedStrippingLevel.Minimal
        );

        // Disable graphics jobs for faster builds
        PlayerSettings.graphicsJobs = false;

        // Use faster script optimization
        PlayerSettings.SetScriptingBackend(
            BuildTargetGroup.Standalone,
            ScriptingImplementation.Mono2x  // Faster than IL2CPP
        );

        // Disable unnecessary build options
        PlayerSettings.bakeCollisionMeshes = false;
        PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.Standalone, false);

        Debug.Log("Dev build optimization complete!");
    }

    /// <summary>
    /// Configure settings for optimized production builds
    /// </summary>
    public static void OptimizeForProductionBuild()
    {
        Debug.Log("Optimizing build settings for production builds...");

        // Enable stripping for smaller production builds
        PlayerSettings.stripEngineCode = true;

        // Enable aggressive managed code stripping
        PlayerSettings.SetManagedStrippingLevel(
            BuildTargetGroup.Standalone,
            ManagedStrippingLevel.High
        );

        // Enable graphics jobs for better runtime performance
        PlayerSettings.graphicsJobs = true;

        // Use IL2CPP for better runtime performance (slower build)
        PlayerSettings.SetScriptingBackend(
            BuildTargetGroup.Standalone,
            ScriptingImplementation.IL2CPP
        );

        // Enable incremental IL2CPP for subsequent builds
        PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.Standalone, true);

        // Bake collision meshes for runtime performance
        PlayerSettings.bakeCollisionMeshes = true;

        Debug.Log("Production build optimization complete!");
    }

    /// <summary>
    /// Optimize shader compilation for dev builds
    /// </summary>
    public static void ConfigureShaderStripping(bool stripForSpeed)
    {
        if (stripForSpeed)
        {
            // Minimal shader stripping for dev builds
            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.lightsUseColorTemperature = false;
            GraphicsSettings.logWhenShaderIsCompiled = false;
        }
        else
        {
            // Full quality for production
            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.lightsUseColorTemperature = true;
        }
    }
}
