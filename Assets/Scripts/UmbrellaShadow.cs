using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UmbrellaShadow : MonoBehaviour
{
    [Header("References")]
    public Transform sunSource;       
    public Transform leftEdge;        
    public Transform rightEdge;       

    [Header("Raycast Settings")]
    public LayerMask groundLayer;     
    public float maxShadowLength = 50f;

    [Header("Visual Settings")]
    public Material shadowMaterial;   // Create a simple UI or Sprite material with transparency!

    private PolygonCollider2D shadowCollider;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector2[] colliderPoints = new Vector2[4];
    private Vector3[] meshVertices = new Vector3[4];
    private int[] meshTriangles = new int[6];

    void Start()
    {
        shadowCollider = GetComponent<PolygonCollider2D>();
        shadowCollider.isTrigger = true; 
        gameObject.layer = LayerMask.NameToLayer("Umbrella");

        // Set up the mesh components for rendering the visual shadow
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (shadowMaterial != null)
        {
            meshRenderer.material = shadowMaterial;
        }

        // Define the triangle layout for our 4-point polygon (2 triangles make a quad)
        meshTriangles[0] = 0;
        meshTriangles[1] = 1;
        meshTriangles[2] = 2;

        meshTriangles[3] = 0;
        meshTriangles[4] = 2;
        meshTriangles[5] = 3;
    }

    void LateUpdate()
    {
        if (sunSource == null)
        {
            GameObject foundSun = GameObject.Find("SunSource");
            if (foundSun != null) sunSource = foundSun.transform;
        }

        if (sunSource == null || leftEdge == null || rightEdge == null) return;

        Vector2 sunPos = sunSource.position;
        Vector2 leftDir = ((Vector2)leftEdge.position - sunPos).normalized;
        Vector2 rightDir = ((Vector2)rightEdge.position - sunPos).normalized;

        RaycastHit2D leftHit = Physics2D.Raycast(leftEdge.position, leftDir, maxShadowLength, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightEdge.position, rightDir, maxShadowLength, groundLayer);

        Vector2 leftBottom = leftHit.collider != null ? leftHit.point : (Vector2)leftEdge.position + leftDir * maxShadowLength;
        Vector2 rightBottom = rightHit.collider != null ? rightHit.point : (Vector2)rightEdge.position + rightDir * maxShadowLength;

        // 1. Update Physics Collider Points (Local Space)
        colliderPoints[0] = transform.InverseTransformPoint(leftEdge.position);
        colliderPoints[1] = transform.InverseTransformPoint(rightEdge.position);
        colliderPoints[2] = transform.InverseTransformPoint(rightBottom);
        colliderPoints[3] = transform.InverseTransformPoint(leftBottom);
        shadowCollider.points = colliderPoints;

        // 2. Update Visual Mesh Vertices (Must match collider points exactly)
        meshVertices[0] = colliderPoints[0];
        meshVertices[1] = colliderPoints[1];
        meshVertices[2] = colliderPoints[2];
        meshVertices[3] = colliderPoints[3];

        mesh.Clear();
        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}