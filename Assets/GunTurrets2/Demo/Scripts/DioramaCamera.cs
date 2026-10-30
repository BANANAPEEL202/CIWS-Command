using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Miniature look for the game camera: tilt-shift blur, a bright cartoon grade and a soft vignette.</summary>
[RequireComponent(typeof(Camera))]
public class DioramaCamera : MonoBehaviour
{
    [Range(0, 1)] public float focusCenter = .5f;
    [Range(0, .5f)] public float focusWidth = .2f;
    [Range(.01f, .6f)] public float focusFalloff = .3f;
    [Range(0, 1)] public float tiltStrength = .8f;
    [Range(0, 2)] public float saturation = 1.04f;
    [Range(.5f, 1.5f)] public float contrast = 1.06f;
    public Color lift = new Color(0, .008f, .02f);
    public Color gain = Color.white;
    [Range(0, 1)] public float vignette = .22f;
    public Color vignetteColor = new Color(.55f, .58f, .7f);

    private Material material;

    // Added to the main camera of every scene so no scene wiring is needed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += (scene, mode) => Attach();
        Attach();
    }

    private static void Attach()
    {
        Camera camera = Camera.main;
        if (camera != null && camera.GetComponent<DioramaCamera>() == null) camera.gameObject.AddComponent<DioramaCamera>();
    }

    private void OnEnable()
    {
        Material source = Resources.Load<Material>("DioramaPost");
        if (source == null || !source.shader.isSupported) { enabled = false; return; }
        material = new Material(source);
    }

    private void OnDisable()
    {
        if (material != null) Destroy(material);
        material = null;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null) { Graphics.Blit(source, destination); return; }
        int width = Mathf.Max(1, source.width / 2), height = Mathf.Max(1, source.height / 2);
        RenderTexture a = RenderTexture.GetTemporary(width, height, 0, source.format);
        RenderTexture b = RenderTexture.GetTemporary(width, height, 0, source.format);
        Graphics.Blit(source, a);
        for (int i = 0; i < 2; i++)
        {
            material.SetVector("_BlurDirection", new Vector2(1 + i, 0));
            Graphics.Blit(a, b, material, 0);
            material.SetVector("_BlurDirection", new Vector2(0, 1 + i));
            Graphics.Blit(b, a, material, 0);
        }
        material.SetTexture("_BlurTex", a);
        material.SetFloat("_FocusCenter", focusCenter);
        material.SetFloat("_FocusWidth", focusWidth);
        material.SetFloat("_FocusFalloff", focusFalloff);
        material.SetFloat("_TiltStrength", tiltStrength);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Contrast", contrast);
        material.SetColor("_Lift", lift);
        material.SetColor("_Gain", gain);
        material.SetFloat("_Vignette", vignette);
        material.SetColor("_VignetteColor", vignetteColor);
        Graphics.Blit(source, destination, material, 1);
        RenderTexture.ReleaseTemporary(a);
        RenderTexture.ReleaseTemporary(b);
    }
}
