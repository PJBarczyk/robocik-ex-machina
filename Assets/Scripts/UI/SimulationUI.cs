using System;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SimulationUI : MonoBehaviour
{
    private VisualElement _root;
    
    public CameraMaster cameraMaster;
    public AUV auv;
    public Topology topology;
    public TargetPlacer targetPlacer;
    
    private List<UnityAction> _lateUpdateActions = new List<UnityAction>();

    private class LabelCounter<T>
    {
        public LabelCounter(T init, Func<T, T> clickFunc, Label label, Func<T, string> toString)
        {
            _value = init;
            _clickFunc = clickFunc;
            _label = label;
            _toString = toString;
            
            UpdateLabel();
        }

        private T _value;
        private readonly Func<T, T> _clickFunc;
        private readonly Label _label;
        private readonly Func<T, string> _toString;

        private void UpdateLabel() => _label.text = _toString(_value);
        public void Click()
        {
            _value = _clickFunc(_value);
            UpdateLabel();
        }
    }

    private void Start()
    {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        var heatmapGenerator = topology.GetComponent<HeatmapGenerator>();
        
        // Initialise control wrappers:
        var targetsLeft = new LabelCounter<int>(
            GameData.initialTargetCount,
            i => i - 1,
            _root.Q<Label>("targets-left"),
            i => $"Targets left: {i}");

        var crashes = new LabelCounter<int>(
            0,
            i => i + 1,
            _root.Q<Label>("crashes"),
            i => $"Crashes: {i}");
        
        // Set initial values:
        _root.Q<Slider>("sampling-interval-slider").value = heatmapGenerator.samplingInterval;
        _root.Q<Slider>("heatmap-refresh-interval-slider").value = heatmapGenerator.heatmapRefreshInterval;

        // Register callbacks:
        _root.Q<Toggle>("camera-lock-rotation").RegisterValueChangedCallback(cameraMaster.OnRotationLock);
        _root.Q<Slider>("camera-zoom").RegisterValueChangedCallback(cameraMaster.SetCameraZoom);
        
        _root.Q<Slider>("sampling-interval-slider").RegisterValueChangedCallback(heatmapGenerator.OnSamplingIntervalChanged);
        _root.Q<Slider>("heatmap-refresh-interval-slider").RegisterValueChangedCallback(heatmapGenerator.OnHeatmapRefreshIntervalChanged);

        // Create actions scheduled for LateUpdate():
        _lateUpdateActions.Add(() => _root.Q<Slider>("camera-zoom").value = cameraMaster.zoom);

        // Add actions:
        _root.Q<Button>("back-to-menu").clicked += OnBackToMenu;
        _root.Q<Button>("reset-heatmap").clicked += heatmapGenerator.ResetVertexData;
        _root.Q<Button>("toggle-heatmap").clicked += () =>
            {
                var stateWhenClicked = heatmapGenerator.enabled;
                heatmapGenerator.enabled = !stateWhenClicked;
                if (stateWhenClicked == false) heatmapGenerator.OnEnableByUI();
            };
        
        targetPlacer.targetFound += targetsLeft.Click;
        auv.onAUVCollison += crashes.Click;

        // Generate procedural controls:
        CreateSensorReadingLabels(_root.Q("sonar-readings"));
    }
    
    private string FormatSensorReading(float reading) => $"{reading:0.00} [m]";
    private void CreateSensorReadingLabels(VisualElement root)
    {
        foreach (Sonar sonar in auv.sonars)
        {
            var container = new VisualElement();
            var label = new Label($"{sonar}:");
            var reading = new Label();

            container.style.flexDirection = FlexDirection.Row;

            root.Add(container);
            container.Add(label);
            container.Add(reading);

            _lateUpdateActions.Add(() => reading.text = FormatSensorReading(auv.GetSonarReading(sonar)));
        }
    }

    private void LateUpdate()
    {
        _lateUpdateActions.ForEach(action => action());
    }
    
    private void OnBackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
