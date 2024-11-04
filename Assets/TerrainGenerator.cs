using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int dimension = 160;
    public int resolution = 8;
    public float patternRadius = 5f;
    public AnimationCurve deformationCurve;
    public AnimationCurve deformationCurve2;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Vector3[] originalVertices;
    public Vector3 hitPoint; 
    private int[] triangles;
    private Vector3[] vertices;
    public float deformationIntensity = 1f;
    public int currentPatternIndex = 0;
    public AnimationCurve[] patterns;
    public GameObject terrainPrefab;
    public int extensionSize = 300;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        GenerateTerrain();

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = meshFilter.mesh;
        }
        else
        {
            Debug.LogError("MeshCollider component not found.");
        }

        patterns = new AnimationCurve[2];
        patterns[0] = deformationCurve;
        patterns[1] = deformationCurve2;
    }

    void GenerateTerrain()
    {
        int numVertices = resolution * resolution;
        int numTriangles = 2 * (resolution - 1) * (resolution - 1);

        vertices = new Vector3[numVertices];
        triangles = new int[3 * numTriangles];

        float stepSize = (float)dimension / (resolution - 1);

        int vertexIndex = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float xPos = x * stepSize;
                float zPos = z * stepSize;
                vertices[vertexIndex] = new Vector3(xPos, 0f, zPos);
                vertexIndex++;
            }
        }

        originalVertices = vertices.Clone() as Vector3[];

        int triangleIndex = 0;
        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int topLeft = z * resolution + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * resolution + x;
                int bottomRight = bottomLeft + 1;

                triangles[triangleIndex] = topLeft;
                triangles[triangleIndex + 1] = bottomLeft;
                triangles[triangleIndex + 2] = topRight;

                triangles[triangleIndex + 3] = topRight;
                triangles[triangleIndex + 4] = bottomLeft;
                triangles[triangleIndex + 5] = bottomRight;

                triangleIndex += 6;
            }
        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = triangles;
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            deformationIntensity--;
        }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            deformationIntensity++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus)) 
        {
            patternRadius++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            patternRadius--;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            currentPatternIndex = (currentPatternIndex + 1) % patterns.Length;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AddTerrainExtension(Vector3.forward);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AddTerrainExtension(Vector3.back);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                AddTerrainExtension(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                AddTerrainExtension(Vector3.right);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            HighlightTerrainChunks();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SubdivideTerrain();
        }
    }

    public void ModifyTerrain(RaycastHit hit, float intensity)
    {
        int nearestVertexIndex = GetNearestVertexIndex(hit);

        bool isOnBorder = IsVertexOnBorder(nearestVertexIndex);

        if (isOnBorder)
        {
            List<int> neighborVertices = GetNeighborVertices(nearestVertexIndex);

            float averageHeight = 0f;
            foreach (var index in neighborVertices)
            {
                averageHeight += vertices[index].y;
            }
            averageHeight /= neighborVertices.Count;
            foreach (int index in neighborVertices)
            {
                Vector3 vertex = vertices[index];
                float distance = Vector3.Distance(vertex, vertices[nearestVertexIndex]);
                float force = patterns[currentPatternIndex].Evaluate(distance / patternRadius) * intensity;
                float targetHeight = averageHeight + force;
                vertex.y += (targetHeight - vertex.y) * 0.5f; 
                vertices[index] = vertex;
            }
        }
        else
        {
            List<int> neighborVertices = GetNeighborVertices(nearestVertexIndex);

            foreach (int index in neighborVertices)
            {
                Vector3 vertex = vertices[index];
                float distance = Vector3.Distance(vertex, vertices[nearestVertexIndex]);
                float force = patterns[currentPatternIndex].Evaluate(distance / patternRadius) * intensity;
                vertex.y += force; 
                vertices[index] = vertex;
            }
        }

        meshFilter.mesh.vertices = vertices;
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    void AddTerrainExtension(Vector3 direction)
    {
        Vector3 spawnPosition = terrainPrefab.transform.position + direction * extensionSize;
        GameObject newTerrain = Instantiate(terrainPrefab, spawnPosition, terrainPrefab.transform.rotation);

        newTerrain.name = "Terrain_" + spawnPosition.ToString();
    }

    void HighlightTerrainChunks()
    {
        
    }


    int GetNearestVertexIndex(RaycastHit hit)
    {
        int V1 = triangles[hit.triangleIndex * 3];
        int V2 = triangles[hit.triangleIndex * 3 + 1];
        int V3 = triangles[hit.triangleIndex * 3 + 2];

        float d1 = Vector3.Distance(vertices[V1], hit.point);
        float d2 = Vector3.Distance(vertices[V2], hit.point);
        float d3 = Vector3.Distance(vertices[V3], hit.point);

        if (d1 < d2 && d1 < d3)
            return V1;
        if (d2 < d3)
            return V2;
        else
            return V3;
    }

    List<int> GetNeighborVertices(int vertexIndex)
    {
        List<int> neighbors = new List<int>();
        Vector3 vertex = vertices[vertexIndex];
        Debug.Log("Vertex: " + vertex);
        for (int i = 0; i < vertices.Length; i++)
        {
            if (i != vertexIndex && Vector3.Distance(vertex, vertices[i]) < patternRadius)
            {
                Debug.Log("Neighbor found: " + vertices[i] + ", Distance: " + Vector3.Distance(vertex, vertices[i]) + ", Pattern Radius: " + patternRadius);
                neighbors.Add(i);
            }
        }
        return neighbors;
    }


    public List<TerrainGenerator> GetNeighborTerrains()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, patternRadius);
        List<TerrainGenerator> neighborTerrains = new List<TerrainGenerator>();

        foreach (var collider in colliders)
        {
            TerrainGenerator neighborTerrain = collider.GetComponent<TerrainGenerator>();
            if (neighborTerrain != null && neighborTerrain != this)
            {
                Debug.Log("Neighbor terrain found: " + neighborTerrain.name);
                neighborTerrains.Add(neighborTerrain);
            }
        }

        return neighborTerrains;
    }

    bool IsVertexOnBorder(int vertexIndex)
    {
        List<int> trianglesContainingVertex = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (triangles[i] == vertexIndex || triangles[i + 1] == vertexIndex || triangles[i + 2] == vertexIndex)
            {
                trianglesContainingVertex.Add(i);
            }
        }

        foreach (int triangleIndex in trianglesContainingVertex)
        {
            int terrainID1 = triangles[triangleIndex];
            int terrainID2 = triangles[triangleIndex + 1];
            int terrainID3 = triangles[triangleIndex + 2];

            if (terrainID1 != terrainID2 || terrainID1 != terrainID3 || terrainID2 != terrainID3)
            {
                Debug.Log("Vertex " + vertexIndex + " is on border.");
                return true;
            }
        }

        return false;
    }

    void SubdivideTerrain()
    {
        int originalNumVertices = vertices.Length;
        int originalNumTriangles = triangles.Length / 3;
        int newResolution = resolution * 2;
        resolution = newResolution;

        List<Vector3> newVertices = new List<Vector3>(originalNumVertices + originalNumTriangles * 3);
        newVertices.AddRange(vertices);

        Dictionary<(int, int), int> edgeToVertexIndex = new Dictionary<(int, int), int>();

        List<int> newTriangles = new List<int>();

        for (int i = 0; i < originalNumTriangles; i++)
        {
            int baseIndex = i * 3;
            int[] baseVertices = new int[3]
            {
            triangles[baseIndex],
            triangles[baseIndex + 1],
            triangles[baseIndex + 2]
            };

            int[] midIndices = new int[3];
            for (int j = 0; j < 3; j++)
            {
                int v1 = baseVertices[j];
                int v2 = baseVertices[(j + 1) % 3];

                var edge = (Mathf.Min(v1, v2), Mathf.Max(v1, v2));

                if (!edgeToVertexIndex.TryGetValue(edge, out midIndices[j]))
                {
                    var newVertex = (vertices[v1] + vertices[v2]) * 0.5f;
                    newVertices.Add(newVertex);
                    midIndices[j] = newVertices.Count - 1;
                    edgeToVertexIndex.Add(edge, midIndices[j]);
                }
            }

            newTriangles.AddRange(new int[]
            {
            baseVertices[0], midIndices[0], midIndices[2],
            midIndices[0], baseVertices[1], midIndices[1],
            midIndices[2], midIndices[1], baseVertices[2],
            midIndices[0], midIndices[1], midIndices[2]
            });
        }

        vertices = newVertices.ToArray();
        triangles = newTriangles.ToArray();

        meshFilter.mesh.Clear();
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = triangles;
        meshCollider.sharedMesh = meshFilter.mesh;
    }
}