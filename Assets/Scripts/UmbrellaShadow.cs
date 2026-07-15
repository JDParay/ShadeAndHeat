using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WrappingShadowProjection : MonoBehaviour
{
    [Header("References")]
    public Transform sunSource;       
    public Transform leftEdge;        
    public Transform rightEdge;       

    [Header("Raycast Settings")]
    public LayerMask groundLayer;     
    public float maxShadowLength = 50f;
    
    [Range(4, 20)]
    [Tooltip("Higher numbers make the shadow wrap around complex corners more perfectly.")]
    public int rayDensity = 10; 

    [Header("Visual Settings")]
    public Material shadowMaterial;   
    [Range(0f, 1f)] public float shadowOpacity = 0.5f; 

    private PolygonCollider2D shadowCollider;
    private MeshFilter meshFilter;
    private Mesh mesh;
    
    // Dynamic lists to hold our changing point array
    private List<Vector2> localPoints = new List<Vector2>();
    private List<Vector3> meshVertices = new List<Vector3>();
    private List<int> meshTriangles = new List<int>();
    private List<Color> meshColors = new List<Color>();

    void Start()
    {
        shadowCollider = GetComponent<PolygonCollider2D>();
        shadowCollider.isTrigger = true; 
        gameObject.layer = LayerMask.NameToLayer("Umbrella");

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (shadowMaterial != null) meshRenderer.material = shadowMaterial;
    }

    void LateUpdate()
    {
        if (sunSource == null)
        {
            GameObject foundSun = GameObject.Find("SunSource");
            if (foundSun != null) sunSource = foundSun.transform;
        }

        if (sunSource == null || leftEdge == null || rightEdge == null) return;

        // Clear out old calculations from the last frame
        localPoints.Clear();
        meshVertices.Clear();
        meshTriangles.Clear();
        meshColors.Clear();

        Vector2 sunPos = sunSource.position;

        // 1. Establish top boundary points (The top surface of the shadow)
        localPoints.Add(transform.InverseTransformPoint(leftEdge.position));
        localPoints.Add(transform.InverseTransformPoint(rightEdge.position));

        // 2. Loop through and sweep raycasts from right to left across the angle span
        // Change this line in your script to be fully solid opaque!
        Color shadowColor = new Color(0f, 0f, 0.1f, 1f);

        for (int i = 0; i <= rayDensity; i++)
        {
            // Linear interpolation to scan smoothly from the right tip back to the left tip
            float t = (float)i / rayDensity;
            Vector2 currentEdgeOrigin = Vector2.Lerp(rightEdge.position, leftEdge.position, t);
            Vector2 rayDir = (currentEdgeOrigin - sunPos).normalized;

            // Fire a raycast out into the world to see what it bumps into
            RaycastHit2D hit = Physics2D.Raycast(currentEdgeOrigin, rayDir, maxShadowLength, groundLayer);
            Vector2 hitPoint = hit.collider != null ? hit.point : currentEdgeOrigin + rayDir * maxShadowLength;

            // Save the hit point to wrap our collider shape!
            localPoints.Add(transform.InverseTransformPoint(hitPoint));
        }

        // 3. Update the physical trigger shape
        shadowCollider.points = localPoints.ToArray();

        // 4. Triangulate the polygon into a clean 2D mesh layout dynamically
        for (int i = 0; i < localPoints.Count; i++)
        {
            meshVertices.Add(new Vector3(localPoints[i].x, localPoints[i].y, 0f));
            meshColors.Add(shadowColor);
        }

        // Generate triangles using a fan layout originating from vertex 0 (Left Edge)
        for (int i = 1; i < localPoints.Count - 1; i++)
        {
            meshTriangles.Add(0);
            meshTriangles.Add(i);
            meshTriangles.Add(i + 1);
        }

        mesh.Clear();
        mesh.vertices = meshVertices.ToArray();
        mesh.colors = meshColors.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}