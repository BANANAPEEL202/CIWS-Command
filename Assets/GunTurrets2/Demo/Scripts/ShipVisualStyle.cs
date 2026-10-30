using System.Collections.Generic;
using UnityEngine;

/// <summary>Shares a small palette of toon materials across the existing ship meshes.</summary>
[DisallowMultipleComponent]
public class ShipVisualStyle : MonoBehaviour
{
    [SerializeField] private Shader shipShader;
    [SerializeField] private Transform hull;
    private readonly Dictionary<Material, Material> palette = new Dictionary<Material, Material>();
    private readonly List<Mesh> reshaped = new List<Mesh>();
    // The forward share of the hull that tapers to the stem, and the height below which hull and deck are reshaped.
    private const float BowFraction = .29f, DeckLevel = -8;

    private void Awake()
    {
        if (hull != null) SharpenBow();
        if (shipShader == null) return;
        foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>(true))
        {
            Material[] materials = mesh.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];
                if (source == null || source.renderQueue >= 2500) continue;
                if (!palette.TryGetValue(source, out Material styled))
                {
                    styled = new Material(shipShader) { name = source.name + " (Toon)" };
                    styled.color = source.HasProperty("_Color") ? source.color : Color.white;
                    palette.Add(source, styled);
                }
                materials[i] = styled;
            }
            mesh.sharedMaterials = materials;
            if (hull != null && mesh.transform.IsChildOf(hull))
            {
                var properties = new MaterialPropertyBlock();
                mesh.GetPropertyBlock(properties);
                properties.SetFloat("_ShipDetails", 1f);
                mesh.SetPropertyBlock(properties);
            }
        }
    }

    private void LateUpdate()
    {
        // One player ship; avoids updating every material separately.
        Shader.SetGlobalMatrix("_CIWSWorldToShip", transform.worldToLocalMatrix);
    }

    /// <summary>
    /// Replaces the curved bow taper with a straight wedge ending in a sharp stem, on runtime copies of the hull meshes.
    /// The imported model is split into sibling meshes per colour sharing one frame, so the shape is measured across all of them.
    /// </summary>
    private void SharpenBow()
    {
        var filters = new List<MeshFilter>();
        Transform frame = null;
        foreach (MeshFilter filter in hull.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh source = filter.sharedMesh;
            if (source == null || !source.isReadable || source.bounds.size.z < 200) continue;
            if (frame == null) frame = filter.transform.parent;
            if (filter.transform.parent == frame) filters.Add(filter);
        }
        if (filters.Count == 0) return;
        float minZ = float.MaxValue, tip = float.MinValue, minX = float.MaxValue, maxX = float.MinValue;
        var shells = new List<Vector3[]>();
        foreach (MeshFilter filter in filters)
        {
            Vector3[] vertices = filter.sharedMesh.vertices;
            shells.Add(vertices);
            foreach (Vector3 v in vertices)
            {
                if (v.y > DeckLevel) continue;
                minZ = Mathf.Min(minZ, v.z); tip = Mathf.Max(tip, v.z);
                minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x);
            }
        }
        float centre = (minX + maxX) * .5f, start = tip - (tip - minZ) * BowFraction;

        // Plan-view half beam along the bow, sampled every unit and kept non-increasing toward the stem.
        int bins = Mathf.CeilToInt(tip - start) + 1;
        var beam = new float[bins];
        foreach (Vector3[] vertices in shells)
        foreach (Vector3 v in vertices)
            if (v.y <= DeckLevel && v.z >= start)
            {
                int b = Mathf.Clamp(Mathf.RoundToInt(v.z - start), 0, bins - 1);
                beam[b] = Mathf.Max(beam[b], Mathf.Abs(v.x - centre));
            }
        for (int b = bins - 2; b >= 0; b--) beam[b] = Mathf.Max(beam[b], beam[b + 1]);
        float shoulder = beam[0];
        for (int f = 0; f < filters.Count; f++)
        {
            Mesh mesh = Instantiate(filters[f].sharedMesh);
            mesh.name = filters[f].sharedMesh.name + " (Sharp Bow)";
            Vector3[] vertices = shells[f];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                if (v.y > DeckLevel || v.z <= start) continue;
                float original = beam[Mathf.Clamp(Mathf.RoundToInt(v.z - start), 0, bins - 1)];
                float straight = shoulder * (1 - Mathf.Clamp01((v.z - start) / (tip - start)));
                if (original > .001f) v.x = centre + (v.x - centre) * straight / original;
                vertices[i] = v;
            }
            FlatShadeBow(mesh, vertices, start);
            filters[f].sharedMesh = mesh;
            reshaped.Add(mesh);
        }
    }

    // Bow triangles get their own vertices and face normals so the flat wedge sides read as crisp planes.
    private static void FlatShadeBow(Mesh mesh, Vector3[] vertices, float start)
    {
        Vector3[] normals = mesh.normals;
        var newVertices = new List<Vector3>(vertices);
        var newNormals = new List<Vector3>(normals);
        var uvs = new List<Vector2>(); mesh.GetUVs(0, uvs);
        var tangents = new List<Vector4>(); mesh.GetTangents(tangents);
        var colors = new List<Color>(); mesh.GetColors(colors);
        var submeshes = new int[mesh.subMeshCount][];
        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            int[] triangles = mesh.GetTriangles(s);
            for (int t = 0; t < triangles.Length; t += 3)
            {
                Vector3 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                if (a.z <= start || b.z <= start || c.z <= start || a.y > DeckLevel || b.y > DeckLevel || c.y > DeckLevel) continue;
                Vector3 face = Vector3.Cross(b - a, c - a).normalized;
                if (Vector3.Dot(face, normals[triangles[t]] + normals[triangles[t + 1]] + normals[triangles[t + 2]]) < 0) face = -face;
                for (int k = 0; k < 3; k++)
                {
                    int source = triangles[t + k];
                    newVertices.Add(vertices[source]);
                    newNormals.Add(face);
                    if (uvs.Count > 0) uvs.Add(uvs[source]);
                    if (tangents.Count > 0) tangents.Add(tangents[source]);
                    if (colors.Count > 0) colors.Add(colors[source]);
                    triangles[t + k] = newVertices.Count - 1;
                }
            }
            submeshes[s] = triangles;
        }
        if (newVertices.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(newVertices);
        mesh.SetNormals(newNormals);
        if (uvs.Count > 0) mesh.SetUVs(0, uvs);
        if (tangents.Count > 0) mesh.SetTangents(tangents);
        if (colors.Count > 0) mesh.SetColors(colors);
        for (int s = 0; s < submeshes.Length; s++) mesh.SetTriangles(submeshes[s], s);
        mesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        foreach (Mesh mesh in reshaped) Destroy(mesh);
        reshaped.Clear();
        foreach (Material material in palette.Values) Destroy(material);
        palette.Clear();
    }
}
