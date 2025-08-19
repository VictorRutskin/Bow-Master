using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowerShooter : MonoBehaviour
{
    [Header("Cooldown")]
    public float shootCooldown = 1f;   // seconds between shots
    private float lastShootTime = -999f;


    [Header("Refs")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    [Header("Radii (world units)")]
    public float grabRadius = 3.0f;   // must click within this to start dragging
    public float dragRadius = 4.0f;   // max pull circle (also your visible circle)
    public bool showGrabRadius = false;

    [Header("Power")]
    public float minShootForce = 0.1f;
    public float maxShootForce = 6f;

    [Header("Visuals")]
    public LineRenderer pullLine;        // line that shows the pull vector
    public LineRenderer dragCircleLine;  // visible circle in game
    public LineRenderer grabCircleLine;  // optional inner circle
    public int circleSegments = 80;
    public float lineWidth = 0.035f;

    private bool isDragging = false;


    void Awake()
    {
        // Ensure pull line exists (can be on this object)
        if (pullLine == null) pullLine = GetComponent<LineRenderer>();
        SetupLine(pullLine, 2, loop: false);

        // Create or setup circle lines
        if (dragCircleLine == null)
            dragCircleLine = CreateChildLine("DragCircle", circleSegments, loop: true);
        if (grabCircleLine == null && showGrabRadius)
            grabCircleLine = CreateChildLine("GrabCircle", circleSegments, loop: true);

        // Draw circles once (they follow spawn point in Update)
        RedrawCircles();
        pullLine.enabled = false;
    }

    void Update()
    {
        // Keep circles centered on the spawn point (in case tower moves)
        RedrawCircles();

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            float distToSpawn = Vector2.Distance(mouseWorld, arrowSpawnPoint.position);
            if (distToSpawn <= grabRadius)
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
            // cooldown check
            if (Time.time - lastShootTime >= shootCooldown)
            {
                Vector3 pullVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - arrowSpawnPoint.position;
                pullVector.z = 0;

                float pullLen = pullVector.magnitude;
                if (pullLen > dragRadius)
                {
                    pullVector = pullVector.normalized * dragRadius;
                    pullLen = dragRadius;
                }

                Vector2 shootDir = (-pullVector).normalized;
                float normalized = dragRadius > 0f ? pullLen / dragRadius : 0f;
                float shootForce = Mathf.Lerp(minShootForce, maxShootForce, normalized);

                ShootArrow(shootDir, shootForce);
                lastShootTime = Time.time;
            }

            isDragging = false;
            pullLine.enabled = false;
        }
    }

    void ShootArrow(Vector2 direction, float force)
    {
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        var rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            if (arrow.GetComponent<ArrowRotation>() == null) arrow.AddComponent<ArrowRotation>();
        }
    }

    // --- helpers ---
    void RedrawCircles()
    {
        DrawCircle(dragCircleLine, arrowSpawnPoint.position, dragRadius);
        if (showGrabRadius && grabCircleLine != null)
            DrawCircle(grabCircleLine, arrowSpawnPoint.position, grabRadius);
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

        // If you have a material, assign it; otherwise Unity’s default line material works.
        // Optional color:
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.9f, 1f) }
        );
        lr.colorGradient = grad;
        lr.sortingOrder = 10; // keep above ground
    }

    void DrawCircle(LineRenderer lr, Vector3 center, float radius, int segments = 80)
    {
        lr.positionCount = segments;
        lr.loop = true;

        for (int i = 0; i < segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            lr.SetPosition(i, center + pos);
        }
    }

}
