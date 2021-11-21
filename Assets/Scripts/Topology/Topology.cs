using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using SimpleFileBrowser;
using UnityEngine.Events;
using UnityEngine.Experimental.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Topology : MonoBehaviour
{
    public int xVerticesCount { get; private set; }
    public int zVerticesCount { get; private set; }

    [Min(0)] public float edgeLength;
    public Texture2D heightMap;
    [SerializeField] private Texture2D backupHeightMap;

    public Vector3[] vertices;

    private MeshFilter _meshFilter;
    private MeshRenderer _renderer;
    private MeshCollider _collider;
    
    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<MeshCollider>();

        if (Application.isEditor)
        {
            heightMap = TopologyData.heightmap != null ? TopologyData.heightmap : backupHeightMap;
        }
        else
        {
            heightMap = TopologyData.heightmap;
        }
        
        edgeLength = GetEdgeLength(heightMap, TopologyData.area);

        xVerticesCount = heightMap.width;
        zVerticesCount = heightMap.height;
        
        BuildMesh();
    }

    private float GetEdgeLength(Texture2D heightmap, float area)
    {
        var submergedCount = heightmap.GetPixels().Select(c => c.r > 0).
            Aggregate(0, (sum, b) => sum + (b ? 1 : 0));

        var faceArea = area / submergedCount;
        return Mathf.Sqrt(faceArea);
    }

    private void BuildMesh()
    {
        var maxDepth = TopologyData.depth;
        var heightData = heightMap.GetPixels().Select(c => -c.r * maxDepth).ToArray();

        var xOrigin = xVerticesCount * edgeLength * 0.5f;
        var zOrigin = zVerticesCount * edgeLength * 0.5f;

        vertices = new Vector3[xVerticesCount * zVerticesCount];
        for (int z = 0; z < zVerticesCount; z++)
        {
            for (int x = 0; x < xVerticesCount; x++)
            {
                var i = z * xVerticesCount + x;
                vertices[i] = new Vector3(
                    xOrigin - x * edgeLength,
                    heightData[i],
                    zOrigin - z * edgeLength);
            }
        }

        var triangles = new int[(xVerticesCount - 1) * (zVerticesCount - 1) * 6];
        for (int z = 0; z < zVerticesCount - 1; z++)
        {
            for (int x = 0; x < xVerticesCount - 1; x++)
            {
                var ogVert = z * xVerticesCount + x;
                var i = 6 * (z * (xVerticesCount - 1) + x);
                
                triangles[  i  ] = ogVert;
                triangles[i + 1] = ogVert + 1 + xVerticesCount;
                triangles[i + 2] = ogVert + 1;
                                     
                triangles[i + 3] = ogVert;
                triangles[i + 4] = ogVert + xVerticesCount;
                triangles[i + 5] = ogVert + xVerticesCount + 1;
            }
        }

        var uv = new Vector2[xVerticesCount * zVerticesCount];
        for (int x = 0; x < xVerticesCount; x++)
        {
            for (int y = 0; y < zVerticesCount; y++)
            {
                var i = x * zVerticesCount + y;
                uv[i] = new Vector2(
                    x / (float) xVerticesCount,
                    y / (float) zVerticesCount);
            }
        }

        var mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };
        mesh.RecalculateNormals();
        
        _meshFilter.mesh = mesh;
        
        const string shaderScaleProperty = "_Scale";
        _renderer.material.SetFloat(shaderScaleProperty, maxDepth);

        _collider.sharedMesh = mesh;
        
        onMeshBuilt.Invoke();
    }
    
    public UnityAction onMeshBuilt;

    public Vector3[] VerticesBelowDepth(float minRelativeDepth = 0) =>
        vertices.Where(v => v.y < minRelativeDepth * TopologyData.depth).ToArray();
    
}