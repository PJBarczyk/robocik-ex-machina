using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using SimpleFileBrowser;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SetupScreenManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private StyleSheet _styleSheet;
    private EventSystem _eventSystem;
    private VisualElement _root;

    private VisualElement _heightmapPreview;
    private VisualElement _sampleHeightmapsContainer;
    
    public Texture2D backup;

    private float _area = TopologyData.area;
    private float _depth = TopologyData.depth;
    private float _targetCount = GameData.initialTargetCount;

    private Label _performanceWarningDensity;
    private Label _performanceWarningSize;

    #region Text

    private bool _isTextEn = true;
    private const string _howToPlayTextEn =
@"The goal of the game is finding targets placed on the bed of the water body. Your main tool to help you is a hydrophone, that shows the direction to the closest target in its range using using a yellow arrow.

On the technical front, this application is not only suited to be a player input based game, but also a visualisation medium for testing pathfinding algorithms. For this cause an array of tools is available for the user - input indicators in form of arrows, heatmap of drone's position or graphical and text excerpts of sonars' readings.

<b>  Controls</b>:
<b>W, S or Up, Down arrows</b> - Forward, backward thrust.
<b>A, D or Left, Right arrows</b> - Strafe-like, leftward, rightward thrust.
<b>Space, Shift</b> - Upward, downward thrust.
<b>Q, E</b> - Counterclockwise, clockwise turning thrust.";

    private const string _howToPlayTextPl =
        @"Celem gry jest znalezienie celów ukrytych na dnie zbiornika. Podstawowym narzędziem jest hydrofon, wskazujący żółtą strzałką kierunek do najbliższego celu obecnego w zasięgu sensora.

Od strony technicznej, program jest przystosowany nie tylko do gry porzez ręczne sterowanbie, lecz również osadzenia w niej algorytmu sterującego dronem. Z tego powodu przygotowano zestaw narzędzi mających na celu wizualizację parametrów jak wartości przepustnic (kolorowe strzałki wychodzące z drona), mapa ciepła położenia czy tekstowe oraz graficzne przedstawienia odczytów sonarów.

<b>  Sterowanie</b>:
<b>W, S lub strzałki w Górę, w Dół</b> - Przedni, tylny ciąg.
<b>A, D lub strzałki w Lewo, w Prawo</b> - Boczny - lewy, prawy ciąg.
<b>Space, Shift</b> - Ciąg w Górę, w Dół.
<b>Q, E</b> - Ciąg skręcający zgodnie bądź przeciwnie do kierunku wskazówek zegara.";

    #endregion

    private void Start()
    {
        _uiDocument = gameObject.GetComponent<UIDocument>();
        _eventSystem = gameObject.GetComponent<EventSystem>();
        _root = _uiDocument.rootVisualElement;
        
        // Save some queried VisualElements to variables:
        _heightmapPreview = _root.Q<VisualElement>("heightmap-preview");
        _sampleHeightmapsContainer = _root.Q<VisualElement>("sample-heightmaps");

        var areaInput = _root.Q<TextField>("area-input");
        var depthInput = _root.Q<TextField>("depth-input");
        var targetCountInput = _root.Q<TextField>("target-count-input");
        
        var howToPlayLabel = _root.Q<Label>("how-to-play-label");

        _performanceWarningDensity = _root.Q<Label>("performance-warning-density");
        _performanceWarningSize = _root.Q<Label>("performance-warning-size");

        // Assign actions
        _root.Q<Button>("select-heightmap-button").clicked += OpenHeightmapFileBrowser;
        _root.Q<Button>("start-button").clicked += OnStartSimulation;
        _root.Q<Button>("how-to-play-language-switch").clicked += () => {
            _isTextEn = !_isTextEn;
            howToPlayLabel.text = _isTextEn ? _howToPlayTextEn : _howToPlayTextPl;
        };
        
        // Set text and initial values
        howToPlayLabel.text = _isTextEn ? _howToPlayTextEn : _howToPlayTextPl;

        areaInput.value = _area.ToString();
        depthInput.value = _depth.ToString();
        targetCountInput.value = _targetCount.ToString();

        // Register callbacks
        _root.Q<TextField>("area-input").RegisterValueChangedCallback(evn =>
        {
            _area = float.TryParse(evn.newValue, out var f) ? f : _area;
            UpdatePerformanceWarnings();
        });
        depthInput.RegisterValueChangedCallback(evn =>
        {
            _area = float.TryParse(evn.newValue, out var f) ? f : _depth;
            UpdatePerformanceWarnings();
        });
        targetCountInput.RegisterValueChangedCallback(evn =>
            _area = int.TryParse(evn.newValue, out var i) ? i : _targetCount);
        
        // Create procedural controls:
        CreateSampleHeightmapsPanel();
        
        // Call updates
        UpdatePerformanceWarnings();
    }

    private void OpenHeightmapFileBrowser()
    {
        FileBrowser.SetFilters(false, ".jpg", ".png");
        FileBrowser.ShowLoadDialog(
            OnHeightmapSelected,
            () => _eventSystem.enabled = true,
            FileBrowser.PickMode.Files);
    }

    private void OnHeightmapSelected(string[] files)
    {
        TopologyData.LoadFromBytes(FileBrowserHelpers.ReadBytesFromFile(files[0]));
        UpdateHeightmapPreview();
        UpdatePerformanceWarnings();
    }

    private void UpdateHeightmapPreview()
    {
        var heightmap = TopologyData.heightmap;
        
        _heightmapPreview.style.backgroundImage = heightmap;

        float width = heightmap.width;
        float height = heightmap.height;

        const float relativeScale = 50f; // Container percentage

        if (width > height)
        {
            _heightmapPreview.style.width = new StyleLength(Length.Percent(relativeScale));
            _heightmapPreview.style.paddingBottom = new StyleLength(Length.Percent(height / width * relativeScale));
        }
        else
        {
            _heightmapPreview.style.width = new StyleLength(Length.Percent(width / height * relativeScale));
            _heightmapPreview.style.paddingBottom = new StyleLength(Length.Percent(relativeScale));
        }
    }

    private void OnStartSimulation()
    {
        if (Application.isEditor && TopologyData.heightmap == null) TopologyData.heightmap = backup;
        if (TopologyData.heightmap == null) return;

        // Cancel start if parameters are invalid
        if (!float.TryParse(_root.Q<TextField>("area-input").value, out var area) || area <= 0) return;
        if (!float.TryParse(_root.Q<TextField>("depth-input").value, out var depth) || depth <= 0) return;
        if (!int.TryParse(_root.Q<TextField>("target-count-input").value, out var targetCount) || targetCount <= 0) return;
        
        TopologyData.area = area;
        TopologyData.depth = depth;
        GameData.initialTargetCount = targetCount;

        SceneManager.LoadScene(1);
    }

    private void CreateSampleHeightmapsPanel()
    {
        
        var samples = Resources.LoadAll("Sample Heightmaps", typeof(Texture2D)).
            Select(o => (Texture2D) o).
            Select(t => TopologyData.Grayscalify(t)).
            Where(o => o.isReadable).
            ToArray();
        
        foreach (var sample in samples)
        {
            var preview = new Button();
                
            preview.AddToClassList("sample-heightmap-preview");
    
            preview.style.backgroundImage = sample;
    
            preview.clicked += () =>
            {
                TopologyData.heightmap = sample;
                UpdateHeightmapPreview();
                UpdatePerformanceWarnings();
            };
            
            _sampleHeightmapsContainer.Add(preview);
        }
    }

    private void SetVisible(VisualElement visualElement, bool visible) =>
        visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    
    private void UpdatePerformanceWarnings()
    {
        if (TopologyData.heightmap == null)
        {
            SetVisible(_performanceWarningDensity, false);
            SetVisible(_performanceWarningSize, false);
        }
        else
        {
            const float safeDensity = 20000;
            SetVisible(_performanceWarningDensity, TopologyData.submergedVertexCount / _area > safeDensity);

            const int safeSize = 500 * 500;
            SetVisible(_performanceWarningSize, TopologyData.vertexCount > safeSize);
        }

        
    }
}
