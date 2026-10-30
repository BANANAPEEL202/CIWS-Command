using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Square console key for one module: dark panel, status lamp and a five-cell repair bar.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public class ShipModuleButton : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    public enum State { Online, Offline, Repairing, Paused }

    public const int Cells = 5;
    public const float CellWidth = 5, CellGap = 1.5f, BarLeft = -33, BarY = -31;
    private const float Lamp = 6;
    private static readonly Color Panel = new Color(.025f, .065f, .095f, .75f);
    private static readonly Color Alert = new Color(.4f, .1f, .08f, .82f);

    private State state;
    private float progress;
    private bool hovered;

    /// <summary>The border, lamp and lit cells use the status colour.</summary>
    public void SetStatus(State status, float repairProgress, Color tint)
    {
        repairProgress = status == State.Online ? 1 : repairProgress;
        if (state == status && progress == repairProgress && color == tint) return;
        state = status; progress = repairProgress; color = tint;
        SetVerticesDirty();
    }

    public void OnPointerEnter(PointerEventData eventData) { hovered = true; SetVerticesDirty(); }
    public void OnPointerExit(PointerEventData eventData) { hovered = false; SetVerticesDirty(); }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = rectTransform.rect;
        Color panel = state == State.Offline || state == State.Paused ? Alert : Panel;
        if (hovered) panel.a = Mathf.Min(1, panel.a + .15f);
        Box(vh, rect, panel);
        Color border = color;
        border.a = hovered ? 1 : .55f;
        float w = hovered ? 1.5f : 1;
        Box(vh, new Rect(rect.xMin, rect.yMin, rect.width, w), border);
        Box(vh, new Rect(rect.xMin, rect.yMax - w, rect.width, w), border);
        Box(vh, new Rect(rect.xMin, rect.yMin + w, w, rect.height - w * 2), border);
        Box(vh, new Rect(rect.xMax - w, rect.yMin + w, w, rect.height - w * 2), border);

        Box(vh, new Rect(rect.xMax - 6 - Lamp, rect.yMax - 6 - Lamp, Lamp, Lamp), color);

        // Lit cells show repair progress; a healthy module is fully lit.
        int lit = Mathf.Clamp(Mathf.FloorToInt(progress * Cells + .001f), 0, Cells);
        Color empty = color; empty.a = .22f;
        Vector2 origin = new Vector2(rect.center.x + BarLeft, rect.center.y + BarY);
        for (int i = 0; i < Cells; i++)
            Box(vh, new Rect(origin.x + i * (CellWidth + CellGap), origin.y - 3.5f, CellWidth, 7), i < lit ? color : empty);
    }

    private static void Box(VertexHelper vh, Rect rect, Color tint)
    {
        int start = vh.currentVertCount;
        vh.AddVert(new Vector2(rect.xMin, rect.yMin), tint, Vector2.zero);
        vh.AddVert(new Vector2(rect.xMin, rect.yMax), tint, Vector2.zero);
        vh.AddVert(new Vector2(rect.xMax, rect.yMax), tint, Vector2.zero);
        vh.AddVert(new Vector2(rect.xMax, rect.yMin), tint, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }
}
