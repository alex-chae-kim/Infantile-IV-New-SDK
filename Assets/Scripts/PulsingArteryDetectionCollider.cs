using UnityEngine;

/// <summary>
/// Invisible, non-pulsing collider for a ProceduralPulsingArtery, used only for needle
/// entry detection and angle/depth measurement.
///
/// ProceduralPulsingArtery disables the source artery's collider at runtime and swaps
/// between several radially offset copies every heartbeat, so a raycast target is never
/// stable. This builds one static mesh offset halfway through the pulse range
/// (maxOffset * midpointFraction) from the same source mesh, so it always sits between
/// the smallest and largest visual meshes.
///
/// Place this on a GameObject that is NOT a child of the pulsing artery (the pulsing
/// script duplicates its whole GameObject, children included). It has no renderer and
/// should sit on a layer other systems don't raycast against (e.g. Ignore Raycast).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshCollider))]
public class PulsingArteryDetectionCollider : MonoBehaviour
{
    [Tooltip("The visual pulsing artery this collider stands in for.")]
    public ProceduralPulsingArtery sourceArtery;

    [Tooltip("Where in the pulse range to place the surface. 0 = smallest mesh, 1 = largest, 0.5 = midpoint.")]
    [Range(0f, 1f)]
    public float midpointFraction = 0.5f;

    [Header("Read only")]
    [SerializeField] private float appliedOffset_mm;

    private Mesh detectionMesh;

    void OnEnable() => Rebuild();

    void OnValidate()
    {
        // Defer: mesh/collider changes aren't allowed inside OnValidate.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => { if (this != null && isActiveAndEnabled) Rebuild(); };
#endif
    }

    [ContextMenu("Rebuild Detection Mesh")]
    public void Rebuild()
    {
        if (sourceArtery == null) return;
        var sourceFilter = sourceArtery.GetComponent<MeshFilter>();
        if (sourceFilter == null || sourceFilter.sharedMesh == null) return;

        Mesh src = sourceFilter.sharedMesh;
        appliedOffset_mm = sourceArtery.maxOffset * midpointFraction;

        // Match the source's pose so the offset mesh lines up exactly.
        Transform s = sourceArtery.transform;
        transform.SetPositionAndRotation(s.position, s.rotation);
        transform.localScale = DivideScale(s.lossyScale, transform.parent != null ? transform.parent.lossyScale : Vector3.one);

        // Same offset method ProceduralPulsingArtery uses: move vertices along their normals.
        Vector3[] verts = src.vertices;
        Vector3[] normals = src.normals;
        for (int i = 0; i < verts.Length; i++)
            verts[i] += normals[i] * appliedOffset_mm;

        if (detectionMesh == null)
        {
            detectionMesh = new Mesh { name = src.name + "_Detection", hideFlags = HideFlags.DontSave };
        }
        detectionMesh.Clear();
        detectionMesh.indexFormat = src.indexFormat;
        detectionMesh.vertices = verts;
        detectionMesh.normals = normals;
        detectionMesh.triangles = src.triangles;
        detectionMesh.RecalculateBounds();

        var col = GetComponent<MeshCollider>();
        col.sharedMesh = null; // force the physics mesh to rebake
        col.sharedMesh = detectionMesh;
    }

    private static Vector3 DivideScale(Vector3 a, Vector3 b) =>
        new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

    void OnDestroy()
    {
        if (detectionMesh != null)
        {
            if (Application.isPlaying) Destroy(detectionMesh);
            else DestroyImmediate(detectionMesh);
        }
    }
}
