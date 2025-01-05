using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [Header("Tracer Settings")]
    [SerializeField] private float tracerDuration = 0.2f; // Duration of the tracer
    [SerializeField] private Color tracerColor = Color.red; // Color of the tracer
    [SerializeField] private float tracerWidth = 0.2f; // Width of the tracer line
    [SerializeField] private LineRenderer lineRenderer; // LineRenderer component
    private float tracerTimer;

    private void Start()
    {
        // Set the initial properties of the LineRenderer
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = tracerWidth;
        lineRenderer.endWidth = tracerWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = tracerColor;
        lineRenderer.endColor = tracerColor;

        // Disable the tracer at the start
        lineRenderer.enabled = false;
        tracerTimer = tracerDuration;
    }

    public void FireTracer(Vector3 startPosition, Vector3 targetPosition)
    {
        // Enable the tracer line and set its positions
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, targetPosition);
        
        // Reset the timer for the tracer's duration
        tracerTimer = tracerDuration;
    }

    private void Update()
    {
        // Decrease the timer
        tracerTimer -= Time.deltaTime;

        // Disable the tracer after the duration
        if (tracerTimer <= 0)
        {
            lineRenderer.enabled = false;
        }
    }
}
