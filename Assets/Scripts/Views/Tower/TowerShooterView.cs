using UnityEngine;

/// <summary>
/// View component for tower shooter visual feedback.
/// Handles circles, pie chart, and line renderer visuals only.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TowerShooterView : MonoBehaviour
{
    [Header("Line Renderers")]
    public LineRenderer pullLine;
    public LineRenderer dragCircleLine;
    public LineRenderer grabCircleLine;

    [Header("Visual Settings")]
    public int circleSegments = 80;
    public float lineWidth = 0.035f;
    public bool showGrabRadius = false;

    [Header("Cooldown Pie")]
    public float cooldownRadius = 0.35f;
    public Vector2 cooldownOffset = new Vector2(0.9f, 1.2f);
    public Color pieColor = new Color(1f, 0.3f, 1f, 0.85f);
    public Color pieBgColor = new Color(1f, 1f, 1f, 0.20f);

    private GameObject _pieGO, _pieBgGO;
    private Mesh _pieMesh, _pieBgMesh;
    private MeshFilter _pieMF, _pieBgMF;
    private MeshRenderer _pieMR, _pieBgMR;

    private Vector3 _arrowSpawnPoint;
    private float _dragRadius;
    private float _grabRadius;

    void Awake()
    {
        if (pullLine == null) pullLine = GetComponent<LineRenderer>();
        SetupLine(pullLine, 2, loop: false);

        if (dragCircleLine == null)
            dragCircleLine = CreateChildLine("DragCircle", circleSegments, loop: true);
        if (grabCircleLine == null && showGrabRadius)
            grabCircleLine = CreateChildLine("GrabCircle", circleSegments, loop: true);

        CreatePieObjects();
        pullLine.enabled = false;
    }

    /// <summary>
    /// Initialize view with settings.
    /// </summary>
    public void Initialize(Vector3 arrowSpawnPoint, float dragRadius, float grabRadius)
    {
        _arrowSpawnPoint = arrowSpawnPoint;
        _dragRadius = dragRadius;
        _grabRadius = grabRadius;
        RedrawCircles();
    }

    /// <summary>
    /// Update pull line visualization.
    /// </summary>
    public void UpdatePullLine(Vector3 start, Vector3 end, bool enabled)
    {
        if (pullLine == null) return;

        pullLine.enabled = enabled;
        if (enabled)
        {
            pullLine.SetPosition(0, start);
            pullLine.SetPosition(1, end);
        }
    }

    /// <summary>
    /// Update cooldown pie visualization.
    /// </summary>
    public void UpdateCooldownPie(float remainingCooldown, float maxCooldown)
    {
        Vector3 center = _arrowSpawnPoint + (Vector3)cooldownOffset;

        if (_pieGO != null) _pieGO.transform.position = center;
        if (_pieBgGO != null) _pieBgGO.transform.position = center;

        float remaining = Mathf.Clamp01(remainingCooldown / Mathf.Max(0.01f, maxCooldown));

        // Background is always a full disk
        if (_pieBgMesh != null)
        {
            BuildFilledDiskLocal(_pieBgMesh, cooldownRadius, 1f);
        }

        // Foreground shows remaining slice
        if (_pieMesh != null)
        {
            if (remaining > 0f)
            {
                if (_pieGO != null) _pieGO.SetActive(true);
                BuildFilledDiskLocal(_pieMesh, cooldownRadius, remaining);
            }
            else
            {
                if (_pieGO != null) _pieGO.SetActive(false);
            }
        }
    }

    void Update()
    {
        RedrawCircles();
    }

    private void RedrawCircles()
    {
        if (dragCircleLine != null)
            DrawCircle(dragCircleLine, _arrowSpawnPoint, _dragRadius);
        if (showGrabRadius && grabCircleLine != null)
            DrawCircle(grabCircleLine, _arrowSpawnPoint, _grabRadius);
    }

    private void CreatePieObjects()
    {
        // Foreground pie
        _pieGO = new GameObject("CooldownPie");
        _pieGO.transform.SetParent(transform);
        _pieMF = _pieGO.AddComponent<MeshFilter>();
        _pieMR = _pieGO.AddComponent<MeshRenderer>();
        _pieMesh = new Mesh { name = "CooldownPieMesh" };
        _pieMF.sharedMesh = _pieMesh;
        _pieMR.material = new Material(Shader.Find("Sprites/Default")) { color = pieColor };
        _pieMR.sortingOrder = 20;

        // Background
        _pieBgGO = new GameObject("CooldownPieBg");
        _pieBgGO.transform.SetParent(transform);
        _pieBgMF = _pieBgGO.AddComponent<MeshFilter>();
        _pieBgMR = _pieBgGO.AddComponent<MeshRenderer>();
        _pieBgMesh = new Mesh { name = "CooldownPieBgMesh" };
        _pieBgMF.sharedMesh = _pieBgMesh;
        _pieBgMR.material = new Material(Shader.Find("Sprites/Default")) { color = pieBgColor };
        _pieBgMR.sortingOrder = 19;
    }

    private void BuildFilledDiskLocal(Mesh mesh, float radius, float fraction01, int maxSeg = 64)
    {
        float sweep = Mathf.Clamp01(fraction01) * 360f;
        int segments = Mathf.Max(3, Mathf.CeilToInt(maxSeg * Mathf.Clamp01(fraction01)));
        if (sweep >= 359.9f) segments = maxSeg;

        var v = new Vector3[segments + 2];
        v[0] = Vector3.zero;

        float startDeg = 90f;
        float endDeg = 90f - sweep;
        for (int i = 0; i <= segments; i++)
        {
            float t = (segments == 0) ? 0f : (float)i / segments;
            float a = Mathf.Deg2Rad * Mathf.Lerp(startDeg, endDeg, t);
            v[i + 1] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
        }

        int[] tris = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            int ti = i * 3;
            tris[ti + 0] = 0;
            tris[ti + 1] = i + 1;
            tris[ti + 2] = i + 2;
        }

        if (mesh == null) mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = v;
        mesh.triangles = tris;
        if (mesh.uv == null || mesh.uv.Length != v.Length)
        {
            var uv = new Vector2[v.Length];
            mesh.uv = uv;
        }
        mesh.RecalculateBounds();
    }

    private LineRenderer CreateChildLine(string name, int positions, bool loop)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var lr = go.AddComponent<LineRenderer>();
        SetupLine(lr, positions, loop);
        return lr;
    }

    private void SetupLine(LineRenderer lr, int positions, bool loop)
    {
        lr.positionCount = positions;
        lr.loop = loop;
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;
        lr.sortingOrder = 10;

        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.9f, 1f) }
        );
        lr.colorGradient = grad;
    }

    private void DrawCircle(LineRenderer lr, Vector3 center, float radius, int segments = 80)
    {
        if (!lr) return;
        lr.positionCount = segments;
        lr.loop = true;
        for (int i = 0; i < segments; i++)
        {
            float ang = 2 * Mathf.PI * i / segments;
            Vector3 pos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0) * radius;
            lr.SetPosition(i, center + pos);
        }
    }
}

