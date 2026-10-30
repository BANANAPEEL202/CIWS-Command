using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Validate the stylized ship and ocean shaders and scene connections.</summary>
public static class GraphicsValidation
{
    private const string Demo = "Assets/GunTurrets2/Demo/";

    [MenuItem("CIWS/Validate Graphics")]
    public static void Validate()
    {
        string path = Demo + "Materials/ToonLit.shader";
        foreach (string shaderPath in new[] { path, Demo + "Water/ToonWater.shader", Demo + "Materials/ToonSky.shader", Demo + "Materials/DioramaPost.shader" })
        {
            AssetDatabase.ImportAsset(shaderPath, ImportAssetOptions.ForceUpdate);
            Shader checkedShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            Require(checkedShader != null && !ShaderUtil.ShaderHasError(checkedShader), "Shader import failed: " + shaderPath);
        }
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        Scene active = SceneManager.GetActiveScene();
        try
        {
            foreach (string name in new[] { "Start Screen.unity", "Game.unity" })
            {
                string scenePath = Demo + name;
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                bool opened = !scene.IsValid() || !scene.isLoaded;
                if (opened) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    ShipVisualStyle style = scene.GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<ShipVisualStyle>(true)).SingleOrDefault();
                    Require(style != null && style.enabled, "Missing ship style in " + name);
                    var data = new SerializedObject(style);
                    Require(data.FindProperty("shipShader").objectReferenceValue == shader, "Ship shader not connected in " + name);
                    Require(data.FindProperty("hull").objectReferenceValue != null, "Hull not connected in " + name);
                }
                finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
            }
        }
        finally { if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active); }
        Debug.Log("CIWS graphics validation passed (ship, ocean, both scenes).");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
