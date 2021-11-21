using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[RequireComponent(typeof(Topology))]
public class HeatmapGenerator : MonoBehaviour
{
    [SerializeField] private AUV auv;
    private Topology _topology;

    [Min(0)] public float samplingInterval = .1f;
    [Min(0)] public float heatmapRefreshInterval = .5f;
    public Vector3[] vertices => _topology.vertices;

    [SerializeField] private Gradient gradient;
    [SerializeField] private Color colorWhenDisabled = Color.gray;
    
    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _topology = GetComponent<Topology>();
        _topology.onMeshBuilt += OnMeshBuilt;
    }

    private void OnMeshBuilt()
    {
        _vertexData = new float[_topology.vertices.Length];
        
        StartCoroutine(ContinuousVertexDataUpdateInAUVRange(vertex =>
            InRangePredicate(auv.transform.position, auv.hydrophoneRadius)(vertex) ? samplingInterval : 0));
        StartCoroutine(ContinuousVertexColorUpdate(standardDeviationNormalisationProvider));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _meshFilter.mesh.colors = vertices.Select(c => colorWhenDisabled).ToArray();
    }

    public void OnEnableByUI() => OnMeshBuilt();


    #region VertexData

    private float[] _vertexData;
    public void ResetVertexData() => _vertexData = new float[_topology.vertices.Length];

    private IEnumerator ContinuousVertexDataUpdate(Func<Vector3, float> func)
    {
        while(enabled)
        {
            UpdateVertexData(func);
            yield return new WaitForSeconds(samplingInterval);
        }
    }
    private void UpdateVertexData(Func<Vector3, float> func)
    {
        var verts = vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            _vertexData[i] += func(verts[i]);
        }
    }
    
    private IEnumerator ContinuousVertexDataUpdateInAUVRange(Func<Vector3, float> func)
    {
        while(enabled)
        {
            UpdateVertexDataInAUVRange(func);
            yield return new WaitForSeconds(samplingInterval);
        }
    }
    private void UpdateVertexDataInAUVRange(Func<Vector3, float> func)
    {
        const int margin = 1;

        var edgeLength = _topology.edgeLength;
        var vertRadius = (int) Mathf.Ceil(auv.hydrophoneRadius / edgeLength);
        var auvPos = auv.transform.position;

        var r = vertRadius + margin;
        
        var xVertCount = _topology.xVerticesCount;
        var zVertCount = _topology.zVerticesCount;

        int XClamp(int x) => Mathf.Clamp(x, 0, xVertCount);
        int ZClamp(int z) => Mathf.Clamp(z, 0, zVertCount);

        var auvApproxX = (int) (-auvPos.x / edgeLength + xVertCount * .5f);
        var auvApproxZ = (int) (-auvPos.z / edgeLength + zVertCount * .5f);

        for (int z = ZClamp(auvApproxZ - r); z < ZClamp(auvApproxZ + r); z++)
        {
            for (int x = XClamp(auvApproxX - r); x < XClamp(auvApproxX + r); x++)
            {
                var i = z * xVertCount + x;
                _vertexData[i] += func(vertices[i]);
            }
        }
    }

    #endregion

    #region VertexColor

    private IEnumerator ContinuousVertexColorUpdate(Func<Func<float, Color>> funcSupplier)
    {
        while(enabled)
        {
            UpdateVertexColors(funcSupplier());
            yield return new WaitForSeconds(heatmapRefreshInterval);
        }
    }

    private void UpdateVertexColors(Func<float, Color> func)
    {
        _meshFilter.mesh.colors = _vertexData.Select(func).ToArray();
    }

    private Func<Func<float, Color>> minMaxNormalisationProvider => () =>
    {
        var max = _vertexData.Max();
        return x => gradient.Evaluate(x / max);
    };

    private Func<Func<float, Color>> standardDeviationNormalisationProvider => () =>
    {
        const float magicNumber = 2;
        var avg = _vertexData.Average();
        var stdDeviation = Mathf.Sqrt(_vertexData.Select(x => x * x).Sum() / _vertexData.Length - avg * avg);
    
        if (stdDeviation == 0) return x => gradient.Evaluate(avg == 0 ? 0 : .5f);
        
        return x => gradient.Evaluate(
            (x - avg) / (magicNumber * stdDeviation) + 0.5f / magicNumber);
    };
    
    private static Func<Vector3, bool> InRangePredicate(Vector3 refPos, float radius) =>
        x => (x - refPos).magnitude < radius;

    #endregion

    #region UIIntegration

    public void OnSamplingIntervalChanged(ChangeEvent<float> evn) => samplingInterval = evn.newValue;
    public void OnHeatmapRefreshIntervalChanged(ChangeEvent<float> evn) => heatmapRefreshInterval = evn.newValue;

    #endregion
}
