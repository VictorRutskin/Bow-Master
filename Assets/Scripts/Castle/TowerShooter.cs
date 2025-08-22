using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowerShooter : MonoBehaviour
{
    [Header("Cooldown")]
    public float shootCooldown = 0.5f;     // total seconds
    private float lastShootTime = -999f;   // set on fire

    [Header("Refs")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    [Header("Radii (world units)")]
    public float grabRadius = 3.0f;
    public float dragRadius = 4.0f;
    public bool showGrabRadius = false;

    [Header("Power")]
    public float minShootForce = 0.1f;
    public float maxShootForce = 6f;

    [Header("Visuals")]
    public LineRenderer pullLine;
    public LineRenderer dragCircleLine;
    public LineRenderer grabCircleLine;
    public int circleSegments = 80;
    public float lineWidth = 0.035f;

    [Header("Cooldown Pie (world-space)")]
    public float cooldownRadius = 0.35f;                 // size of the pie
    public Vector2 cooldownOffset = new Vector2(0.9f, 1.2f); // offset from spawn
    public Color pieColor = new Color(1f, 0.3f, 1f, 0.85f);
    public Color pieBgColor = new Color(1f, 1f, 1f, 0.20f);

    // pie objects
    private GameObject pieGO, pieBgGO;
    private Mesh pieMesh, pieBgMesh;
    private MeshFilter pieMF, pieBgMF;
    private MeshRenderer pieMR, pieBgMR;

    private bool isDragging = false;

    void Awake()
    {
        // Pull line
        if (pullLine == null) pullLine = GetComponent<LineRenderer>();
        SetupLine(pullLine, 2, loop: false);

        // Range circles
        if (dragCircleLine == null)
            dragCircleLine = CreateChildLine("DragCircle", circleSegments, loop: true);
        if (grabCircleLine == null && showGrabRadius)
            grabCircleLine = CreateChildLine("GrabCircle", circleSegments, loop: true);

        // Cooldown meshes (filled pies)
        CreatePieObjects();
        pieMR.material = new Material(Shader.Find("Sprites/Default")) { color = pieColor };
        pieMR.sortingOrder = 200;  // high to be above background

        pieBgMR.material = new Material(Shader.Find("Sprites/Default")) { color = pieBgColor };
        pieBgMR.sortingOrder = 199;


        RedrawCircles();
        pullLine.enabled = false;
    }

    void Update()
    {
        // keep all visuals aligned with tower
        RedrawCircles();
        UpdateCooldownPie();

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            if (Vector2.Distance(mouseWorld, arrowSpawnPoint.position) <= grabRadius)
            {
                isDragging = true;
                pullLine.enabled = true;
                pullLine.SetPosition(0, arrowSpawnPoint.position);
                pullLine.SetPosition(1, arrowSpawnPoint.position);
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 pullVector = mouseWorld - arrowSpawnPoint.position;
            if (pullVector.magnitude > dragRadius)
                pullVector = pullVector.normalized * dragRadius;

            pullLine.SetPosition(0, arrowSpawnPoint.position);
            pullLine.SetPosition(1, arrowSpawnPoint.position + pullVector);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            // can shoot only when cooldown fully empty
            if (Time.time - lastShootTime >= shootCooldown)
            {
                Vector3 pullVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - arrowSpawnPoint.position;
                pullVector.z = 0;

                float pullLen = Mathf.Min(pullVector.magnitude, dragRadius);
                Vector2 shootDir = (-pullVector.normalized);
                float shootForce = Mathf.Lerp(minShootForce, maxShootForce, dragRadius > 0f ? pullLen / dragRadius : 0f);

                ShootArrow(shootDir, shootForce);
                lastShootTime = Time.time;   // resets pie to full
            }

            isDragging = false;
            pullLine.enabled = false;
        }
    }

    void ShootArrow(Vector2 direction, float force)
    {
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        if (arrow.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            if (arrow.GetComponent<ArrowRotation>() == null) arrow.AddComponent<ArrowRotation>();
        }
    }

    // ---------- Cooldown Pie ----------
    void CreatePieObjects()
    {
        // foreground pie
        pieGO = new GameObject("CooldownPie");
        pieGO.transform.SetParent(transform);
        pieMF = pieGO.AddComponent<MeshFilter>();
        pieMR = pieGO.AddComponent<MeshRenderer>();
        pieMesh = new Mesh { name = "CooldownPieMesh" };
        pieMF.sharedMesh = pieMesh;
        pieMR.material = new Material(Shader.Find("Sprites/Default")) { color = pieColor };
        pieMR.sortingOrder = 20;

        // background (full disk)
        pieBgGO = new GameObject("CooldownPieBg");
        pieBgGO.transform.SetParent(transform);
        pieBgMF = pieBgGO.AddComponent<MeshFilter>();
        pieBgMR = pieBgGO.AddComponent<MeshRenderer>();
        pieBgMesh = new Mesh { name = "CooldownPieBgMesh" };
        pieBgMF.sharedMesh = pieBgMesh;
        pieBgMR.material = new Material(Shader.Find("Sprites/Default")) { color = pieBgColor };
        pieBgMR.sortingOrder = 19;
    }

    void UpdateCooldownPie()
    {
        Vector3 center = arrowSpawnPoint.position + (Vector3)cooldownOffset;

        // move the pie objects; vertices stay local around (0,0)
        if (pieGO) pieGO.transform.position = center;
        if (pieBgGO) pieBgGO.transform.position = center;

        // remaining: 1 right after shot -> 0 when ready
        float remaining = Mathf.Clamp01((lastShootTime + shootCooldown - Time.time) / shootCooldown);

        // background is always a full disk (faint)
        BuildFilledDiskLocal(pieBgMesh, cooldownRadius, 1f);

        // foreground shows remaining slice; hide when ready
        if (remaining > 0f)
        {
            pieGO.SetActive(true);
            BuildFilledDiskLocal(pieMesh, cooldownRadius, remaining);
        }
        else
        {
            pieGO.SetActive(false);
        }
    }

    void BuildFilledDiskLocal(Mesh mesh, float radius, float fraction01, int maxSeg = 64)
    {
        float sweep = Mathf.Clamp01(fraction01) * 360f;
        int segments = Mathf.Max(3, Mathf.CeilToInt(maxSeg * Mathf.Clamp01(fraction01)));
        if (sweep >= 359.9f) segments = maxSeg; // full disk

        var v = new Vector3[segments + 2]; // center + arc
        v[0] = Vector3.zero;

        float startDeg = 90f;        // top
        float endDeg = 90f - sweep; // clockwise
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

    // Build a triangle‑fan pie (no rotation; starts at top and shrinks clockwise)
    void BuildFilledDisk(Mesh mesh, Vector3 center, float radius, float fraction01, int maxSeg = 64)
    {
        float sweep = Mathf.Clamp01(fraction01) * 360f;
        int segments = Mathf.Max(3, Mathf.CeilToInt(maxSeg * fraction01));
        if (sweep >= 359.9f) segments = maxSeg; // full disk

        // vertices
        Vector3[] v = new Vector3[segments + 2]; // center + arc points
        v[0] = center;

        float startDeg = 90f;          // top
        float endDeg = 90f - sweep;    // go clockwise (decreasing angle)
        for (int i = 0; i <= segments; i++)
        {
            float t = (segments == 0) ? 0f : (float)i / segments;
            float a = Mathf.Deg2Rad * Mathf.Lerp(startDeg, endDeg, t);
            v[i + 1] = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
        }

        // triangles
        int[] tris = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            int ti = i * 3;
            tris[ti + 0] = 0;
            tris[ti + 1] = i + 1;
            tris[ti + 2] = i + 2;
        }

        // uv not used, but keep valid
        Vector2[] uv = new Vector2[v.Length];
        for (int i = 0; i < v.Length; i++) uv[i] = Vector2.zero;

        mesh.Clear();
        mesh.vertices = v;
        mesh.triangles = tris;
        mesh.uv = uv;
        mesh.RecalculateBounds();
    }

    // ---------- Helpers you already had ----------
    void RedrawCircles()
    {
        DrawCircle(dragCircleLine, arrowSpawnPoint.position, dragRadius);
        if (showGrabRadius && grabCircleLine != null)
            DrawCircle(grabCircleLine, arrowSpawnPoint.position, grabRadius);

        // keep pie objects near the tower
        Vector3 center = arrowSpawnPoint.position + (Vector3)cooldownOffset;
        if (pieGO) pieGO.transform.position = center; // for editor visibility
        if (pieBgGO) pieBgGO.transform.position = center;
    }

    LineRenderer CreateChildLine(string name, int positions, bool loop)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var lr = go.AddComponent<LineRenderer>();
        SetupLine(lr, positions, loop);
        return lr;
    }

    void SetupLine(LineRenderer lr, int positions, bool loop)
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

    void DrawCircle(LineRenderer lr, Vector3 center, float radius, int segments = 80)
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
