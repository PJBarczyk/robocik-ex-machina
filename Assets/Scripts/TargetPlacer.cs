using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TargetPlacer : MonoBehaviour
{
    private int _targetCount;
    public Target targetPrefab;

    public Topology topology;
    
    private void Awake()
    {
        topology.onMeshBuilt += OnTopologyBuilt;
        _targetCount = GameData.initialTargetCount;
    }

    private void OnTopologyBuilt()
    {
        var possiblePositions = topology.VerticesBelowDepth(GameData.minTargetSubmersion);
        
        // Shuffle:
        var random = new System.Random();
        possiblePositions = possiblePositions.OrderBy(v => random.Next()).ToArray();
        
        int index = 0;
        for (int i = 0; i < _targetCount; i++)
        {
            var target = Instantiate(targetPrefab, transform);
            target.tag = "ActiveTarget";
            target.name = $"Target{i}";
            
            target.transform.position = possiblePositions[index++];

            // Check for nasty, but inconceivable, situation where theres not enough vertices to place all targets:
            if (index == possiblePositions.Length) index = 0;
        }
    }

    public UnityAction targetFound;
    private void TargetFound() => targetFound.Invoke();

}
