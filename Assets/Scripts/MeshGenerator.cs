using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public float unitLength;    // Minimum length of a spiral part
    public float thickness;     // The thickness of the spiral
    public Vector3 startingEulerAngles; // The transform of this object is rotated at the start to look more realistic
    public Vector3 rotationEulerAngles; // The mesh will be rotated this much at each update
    public Vector3 spiralVelocity;  // When the scraping stops, the spiral is accelerated to this velocity
    public float spiralLifeSpan;    // The spiral will be destroyed after this many seconds
    
    float tZero;    // The time at the start 
    float tEnd;     // The time when the scraping is stopped
    bool scrape;    // True if the scraper is on the ground, False after it is lifted
    Quaternion rotation;    // The quaternion that is used to rotate the mesh at each update


    // Start is called before the first frame update
    void Start()
    {
        transform.eulerAngles = startingEulerAngles;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
        UpdateMesh();
        
        scrape = true;
        tZero = Time.time;

        rotation = new Quaternion();
        rotation.eulerAngles = rotationEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if(scrape) 
        {
            // Rotate the existing mesh
            Rotate(rotation, Vector3.zero);

            // Shift the existing mesh upward, shift increasingly more with time passed
            Shift(new Vector3(0, unitLength + (Time.time - tZero)/10, 0));

            // Extrude another part of the spiral to enlarge it
            ExtrudeSpiral();

            // Update mesh data and recalculate normals
            UpdateMesh();
            
        }
        else
        {
            // In the game Spiral Roll, I have noticed that the spiral
            // accelerates forward for some time after the scraping is stopped.
            // This could be handled here

            // Destroy this game object after it has been free for some time
            if(Time.time - tEnd > spiralLifeSpan)
            {
                Destroy(gameObject);
            }
        }
    }

    void CreateMesh() 
    // Creates the inner "tip" of the spiral (initial vertices and triangles)
    // This tip is a rectangular prism, with size (1, unitLength, thickness)
    {
        vertices = new Vector3[]
        {
            new Vector3(0,unitLength,0),
            new Vector3(1,unitLength,0),
            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(0,unitLength,thickness),
            new Vector3(1,unitLength,thickness),
            new Vector3(0,0,thickness),
            new Vector3(1,0,thickness)
        };

        triangles = new int[] {3,2,0, 3,0,1, 7,4,6, 7,5,4};
    }

    void Shift(Vector3 offset)
    // Shifts the entire mesh (all the vertices)
    {
        int count = vertices.Length;
        for(int i = 0; i < count; i++)
        {
            vertices[i] = vertices[i] + offset;
        }
    }

    void Rotate(Quaternion rotation, Vector3 center)
    // Rotates the entire mesh around the given center
    {
        int count = vertices.Length;
        for(int i = 0; i < count; i++)
        {
            vertices[i] = rotation * (vertices[i] - center) + center;
        }
    }

    void UpdateMesh() 
    // Updates the mesh's arrays to match the local vertices and triangles arrays
    // And recalculates normals for better lighting
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


    void ExtrudeSpiral() 
    // Adds another "piece" to the spiral
    {
        int vertexCount = vertices.Length;
        int triangleCount = triangles.Length;
        
        // We need to lengthen the vertices and the triangles arrays
        Vector3[] newVertices = new Vector3[vertexCount + 4];
        int[] newTriangles = new int[triangleCount + 12];
        
        // TODO This might not be the best way to lengthen arrays
        // Copy the existing elements of the vertices and the triangles arrays to the new arrays
        for(int i = 0; i < vertexCount; i++)
        {
            newVertices[i] = vertices[i];
        }
        for(int i = 0; i < triangleCount; i++)
        {
            newTriangles[i] = triangles[i];
        }

        // Add 4 new points, forming a rectangle at the base
        newVertices[vertexCount] = new Vector3(0,0,0);
        newVertices[vertexCount + 1] = new Vector3(1,0,0);
        newVertices[vertexCount + 2] = new Vector3(0,0,thickness);
        newVertices[vertexCount + 3] = new Vector3(1,0,thickness);

        // Triangles on the outside of the spiral
        newTriangles[triangleCount] = vertexCount;
        newTriangles[triangleCount + 1] = vertexCount - 3;
        newTriangles[triangleCount + 2] = vertexCount + 1;
        newTriangles[triangleCount + 3] = vertexCount;
        newTriangles[triangleCount + 4] = vertexCount - 4;
        newTriangles[triangleCount + 5] = vertexCount - 3;

        // Triangles on the inside of the spiral
        newTriangles[triangleCount + 6] = vertexCount + 2;
        newTriangles[triangleCount + 7] = vertexCount + 3;
        newTriangles[triangleCount + 8] = vertexCount - 1;
        newTriangles[triangleCount + 9] = vertexCount + 2;
        newTriangles[triangleCount + 10] = vertexCount - 1;
        newTriangles[triangleCount + 11] = vertexCount - 2;

        // TODO We might want to add triangles to the "sides" of the spiral
        // But I don't think it is necessary for this demo

        // Update the arrays
        vertices = newVertices;
        triangles = newTriangles;
    }

    public void StopScraping()
    // This is called when the scraper is lifted from the ground
    {
        // Stop scraping and following the scraper
        scrape = false;
        GetComponent<FollowScraper>().enabled = false;

        // Record the scraping ending time
        tEnd = Time.time;

        // Set the CapsuleCollider's center and radius, then enable it
        // I have used CapsuleCollider since it is a good approximation of the spiral's shape
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        
        if(vertices.Length < 50)
        {
            // if the spiral is very short, center is simply the middle of the first 2 vertices
            col.center = (vertices[0] + vertices[1]) / 2;
        }
        else
        {
            // if the spiral is a little longer, the center is better estimated
            col.center = (vertices[0] + vertices[49]) / 2;
        }
        
        col.radius = Vector3.Distance(col.center, vertices[vertices.Length - 5]) - 0.05f;
        col.enabled = true;

        // Set the velocity of the rigidbody
        GetComponent<Rigidbody>().velocity = spiralVelocity;
    }
}
