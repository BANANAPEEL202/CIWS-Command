using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Damage-control console: one square key per module, pressed by click or number key.</summary>
public class ShipModuleHUD : MonoBehaviour
{
    private ShipModules modules;
    private GameOver gameOver;
    private GameObject canvasObject;
    private readonly ShipModuleButton[] buttons = new ShipModuleButton[4];
    private readonly TMP_Text[] statuses = new TMP_Text[4];
    private readonly TMP_Text[] keys = new TMP_Text[4];
    private readonly TMP_Text[] names = new TMP_Text[4];
    private static readonly string[] Names = { "FWD CIWS", "AFT CIWS", "ENGINES", "RADAR" };
    private static readonly Color Online = Color.white;
    private static readonly Color Offline = new Color(1f, .54f, .48f);
    private static readonly Color Repairing = new Color(1f, .81f, .38f);
    private static readonly Color Ink = Color.white;
    private const float Size = 78, Gap = 6;
    private const string Mono = "<mspace=0.7em>";

    public void Initialize(ShipModules owner)
    {
        modules = owner;
        ShipController ship = owner.GetComponent<ShipController>();
        gameOver = ship != null ? ship.gameOverController : null;
        canvasObject = new GameObject("Damage Control HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1118, 644);
        scaler.matchWidthOrHeight = .5f;
        var root = Rect("Module keys", canvasObject.transform, new Vector2(0, 14), new Vector2(Size * 4 + Gap * 3, Size));
        root.anchorMin = root.anchorMax = new Vector2(.5f, 0);
        root.pivot = new Vector2(.5f, 0);
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            var slot = Rect(Names[i], root, new Vector2((i - 1.5f) * (Size + Gap), 0), new Vector2(Size, Size));
            buttons[i] = slot.gameObject.AddComponent<ShipModuleButton>();
            var button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = buttons[i]; button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => modules.SelectRepair(index));
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;

            keys[i] = Text(Mono + "[" + (i + 1) + "]", slot, new Vector2(-20, 29), new Vector2(30, 12), 9, TextAlignmentOptions.Left);
            names[i] = Text("<cspace=0.08em>" + Names[i], slot, new Vector2(0, 2), new Vector2(Size - 6, 16), 11, TextAlignmentOptions.Center);
            // Status text sits to the right of the five-cell bar the key graphic draws.
            statuses[i] = Text(Mono + "OK", slot, new Vector2(18, -31), new Vector2(34, 12), 9, TextAlignmentOptions.Right);
        }
    }

    private void Update()
    {
        if (modules == null) return;
        // The repair keys disappear with the game-over screen.
        bool over = gameOver != null && gameOver.gameOver;
        if (canvasObject.activeSelf == over) canvasObject.SetActive(!over);
        if (over) return;
        for (int i = 0; i < 4; i++)
        {
            bool online = modules.IsOperational(i), active = modules.ActiveRepair == i;
            float progress = modules.RepairProgress(i);
            Color color = online ? Online : active ? Repairing : Offline;
            statuses[i].text = Mono + (online ? "OK" : active || progress > 0 ?
                Mathf.CeilToInt(modules.RemainingSeconds(i)) + "s" : "DOWN");
            statuses[i].color = color;
            keys[i].color = color;
            names[i].color = online ? Ink : color;
            // The active repair breathes gently so the crew's assignment reads at a glance.
            names[i].canvasRenderer.SetAlpha(active ? .6f + .4f * Mathf.Cos(Time.unscaledTime * 4f) : 1f);
            buttons[i].SetStatus(online ? ShipModuleButton.State.Online : active ? ShipModuleButton.State.Repairing :
                progress > 0 ? ShipModuleButton.State.Paused : ShipModuleButton.State.Offline, progress, color);
        }
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
        return rect;
    }

    /// <summary>Rect positioned from its parent's centre, for laying out inside a square key.</summary>
    private static RectTransform Centered(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = Rect(name, parent, position, size);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        return rect;
    }

    private static TMP_Text Text(string value, Transform parent, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        var text = Centered(value, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value; text.fontSize = fontSize; text.alignment = alignment;
        text.outlineColor = new Color(.025f, .065f, .095f, 1); text.outlineWidth = .15f;
        text.color = Ink; text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private void OnDestroy() { if (canvasObject != null) Destroy(canvasObject); }
}
