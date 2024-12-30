using UnityEditor;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build WebGL")]
    public static void Build()
    {
        BuildPipeline.BuildPlayer(
            new string[] { "Assets/GunTurrets2/Demo/BoatDemo.unity" }, // Path to your main scene(s)
            "Build/WebGL", // Output path
            BuildTarget.WebGL, 
            BuildOptions.None
        );
    }
}
