using UnityEngine;

public class TowerShooter : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float maxDragDistance = 6f; 
    public float minShootForce = 0.1f;
    public float maxShootForce = 6f; 

    private Vector3 dragStartPos;
    private bool isDragging = false;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragStartPos.z = 0;
            isDragging = true;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, arrowSpawnPoint.position);
                lineRenderer.SetPosition(1, arrowSpawnPoint.position);
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPos.z = 0;
            Vector3 dragVector = dragStartPos - currentPos;

            // Clamp drag
            float dragLength = Mathf.Min(dragVector.magnitude, maxDragDistance);
            Vector3 clampedDragVector = dragVector.normalized * dragLength;

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, arrowSpawnPoint.position);
                lineRenderer.SetPosition(1, arrowSpawnPoint.position + clampedDragVector);
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector3 dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragEndPos.z = 0;

            Vector2 shootDirection = (dragStartPos - dragEndPos).normalized;
            float dragDistance = Vector2.Distance(dragStartPos, dragEndPos);
            float clampedDistance = Mathf.Min(dragDistance, maxDragDistance);

            // Simple linear curve
            float normalizedDrag = clampedDistance / maxDragDistance;
            float shootForce = Mathf.Lerp(minShootForce, maxShootForce, normalizedDrag);

            ShootArrow(shootDirection, shootForce);

            isDragging = false;
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
    }

    void ShootArrow(Vector2 direction, float force)
    {
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

            if (arrow.GetComponent<ArrowRotation>() == null)
                arrow.AddComponent<ArrowRotation>();
        }
    }
}
