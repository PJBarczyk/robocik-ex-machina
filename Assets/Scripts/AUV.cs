using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

[DisallowMultipleComponent]
public class AUV : MonoBehaviour
{
    [Header("Models")] 
    public ThrottleToThrustModel throttleToThrustModel;
    public PhysicsModel physicsModel;

    private List<Action> _lateUpdateActions = new List<Action>();

    [SerializeField] private Topology _topology;
    private Rigidbody _rigidbody;

    #region Sensors

    public Sonar[] sonars;
    
    public RaycastHit GetSonarRaycastHit(Sonar sonar)
    {
        RaycastHit hit;
        var auvTrans = transform;
        var auvRot = auvTrans.rotation;
        
        Physics.Raycast(
            auvRot * sonar.localPosition + auvTrans.position,
            auvRot * sonar.direction, 
            out hit);

        return hit;
    }

    public float GetSonarReading(Sonar sonar) => GetSonarRaycastHit(sonar).distance;

    public float hydrophoneRadius = 1;
    [SerializeField] private LineRenderer hydrophoneRender;
    private float _hydrophoneReadingLength = GameData.auvHydrophoneRadius;


    private void DrawHydrophoneReading()
    {
        
        var dir = DirectionToNearestTarget();
        if (dir != Vector3.zero)
        {
            hydrophoneRender.enabled = true;

            var pos = transform.position;
            hydrophoneRender.SetPosition(0, pos);
            hydrophoneRender.SetPosition(1, pos + dir * _hydrophoneReadingLength);
        }
        else
        {
            hydrophoneRender.enabled = false;
        }
    }
    
    public Vector3 DirectionToNearestTarget()
    {
        var activeTargets = GameObject.FindGameObjectsWithTag("ActiveTarget");
        
        if (!activeTargets.Any()) return Vector3.zero;
        
        var auvPosition = transform.position;

        // Create a set of tuples {target, it's position, it's distance to AUV}
        var dataSet =
            activeTargets.Select(src =>
            {
                var position = src.transform.position;
                return new {obj = src, pos = position, dst = (position - auvPosition).magnitude};
            });
        
        // Pick the one with the shortest distance
        var nearest = dataSet.Aggregate((a, b) => a.dst > b.dst ? b : a);

        if (nearest.dst > hydrophoneRadius)
        {
            return Vector3.zero;
        }
        else
        {
            return (nearest.pos - auvPosition).normalized;
        }

    }

    
    
    private Dictionary<Sonar, LineRenderer> _sonarRenderers = new Dictionary<Sonar, LineRenderer>();

    #endregion

    #region Throttle

    private float _forwardThrottle;
    private float _sideThrottle;
    private float _verticalThrottle;
    private float _angularThrottle;

    public Vector3 combinedLinearThrottle => new Vector3(_sideThrottle, _verticalThrottle, _forwardThrottle);

    public float forwardThrottle
    {
        get => _forwardThrottle;
        set => _forwardThrottle = Mathf.Clamp(value, -1, 1);
    }
    public float sideThrottle
    {
        get => _sideThrottle;
        set => _sideThrottle = Mathf.Clamp(value, -1, 1);
    }
    public float verticalThrottle
    {
        get => _verticalThrottle;
        set => _verticalThrottle = Mathf.Clamp(value, -1, 1);
    }
    public float angularThrottle
    {
        get => _angularThrottle;
        set => _angularThrottle = Mathf.Clamp(value, -1, 1);
    }

    #endregion

    #region Movement&Collision

    private Vector3 _velocity;
    private float _angularVelocity;

    [SerializeField] private VisualEffect collisionFX;
    [SerializeField] private AudioSource collisionSFX;
    [SerializeField] private float collisionPushCoefficient = 1;
    private void OnCollisionEnter(Collision collision)
    {
        // Stop all movement and try to move AUV out of the wall/ground
        transform.position -= _velocity * Time.deltaTime * collisionPushCoefficient;
        _velocity = Vector3.zero;
        
        transform.eulerAngles -= new Vector3(0, _angularVelocity * Time.deltaTime * collisionPushCoefficient, 0);
        _angularVelocity = 0;

        // Audio-visual cue
        if (collisionFX != null) collisionFX.Play();
        if (collisionSFX != null) collisionSFX.Play();
        
        onAUVCollison.Invoke();
    }
    
    public UnityAction onAUVCollison; // Useful to, e.g. stop simulation, when testing AI algorithms.

    #endregion
    
    private IEnumerator<bool> CheckIfPlacementValid()
    {
        const float safeDistance = .2f;
        _rigidbody.SweepTest(Vector3.down, out var hit);
        yield return hit.distance > safeDistance;
    }

    private void PlaceInit()
    {
        var random = new System.Random();
        var candidates = new Queue<Vector3>(
            _topology.VerticesBelowDepth().OrderBy(v => random.Next()));
        
        const float projectionOffset = 1;
        const float safeDistance = .2f;
        const int maxTries = 1000;
        
        var halfExtends = GetComponent<MeshFilter>().sharedMesh.bounds.extents;

        for (int i = 0; i < maxTries; i++)
        {
            Debug.Log("Rolling the dice");
            
            var vert = candidates.Dequeue();
            
            Physics.BoxCast(
                new Vector3(vert.x, projectionOffset, vert.z),
                halfExtends,
                Vector3.down, out var hit);

            if (hit.distance > safeDistance + projectionOffset)
            {
                transform.position = new Vector3(vert.x, 0, vert.z);
                return;
            }
        }

        SceneManager.LoadScene(0);
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        // GetComponent<Rigidbody>().detectCollisions = false;
        
        _topology.onMeshBuilt += PlaceInit;
    }

    private void Start()
    {
        var renderable = sonars.Where(s => s.renderAsLine).ToArray();
        if (renderable.Length > 0)
        {
            var container = new GameObject("SonarRenderers");

            foreach (var sonar in renderable)
            {
                var g = new GameObject(sonar.name + " Renderer");
                g.transform.parent = container.transform;
                var rendr = g.AddComponent<LineRenderer>();

                rendr.colorGradient = sonar.gradient;
                rendr.widthCurve = sonar.widthCurve;
                rendr.widthMultiplier = sonar.widthMult;
                rendr.sharedMaterial = sonar.material;
                    
                _sonarRenderers.Add(sonar, rendr);
            }
        }
        
        collisionFX.Stop();
    }

    private void Update()
    {
        var trans = transform;
        
        // Calculate velocities...
        physicsModel.ApplyDrag(ref _velocity);
        var localThrust =  throttleToThrustModel.GetLocalThrust(combinedLinearThrottle) * Time.deltaTime;
        _velocity += trans.rotation * localThrust;
        
        physicsModel.ApplyAngularDrag(ref _angularVelocity);
        _angularVelocity += throttleToThrustModel.GetAngularThrust(_angularThrottle) * Time.deltaTime;
        
        // ...and apply them
        trans.position += _velocity * Time.deltaTime;
        trans.Rotate(Vector3.up, _angularVelocity * Time.deltaTime);

        // Keep AUV underwater
        if (trans.position.y > 0)
        {
            var yNullifier = new Vector3(1, 0, 1);
            _velocity.Scale(yNullifier);

            var position = trans.position;
            position.Scale(yNullifier);
            trans.position = position;
        }
    }

    private void LateUpdate()
    {
        var lateTrans = transform;
        foreach (var entry in _sonarRenderers)
        {
            var sonar = entry.Key;
            var rendr = entry.Value;

            var hit = GetSonarRaycastHit(sonar);

            if (hit.distance > 0)
            {
                rendr.enabled = true;
                rendr.SetPosition(0, lateTrans.position + lateTrans.rotation * sonar.localPosition);
                rendr.SetPosition(1, hit.point);
            }
            else rendr.enabled = false;
        }
        
        DrawHydrophoneReading();
    }
}
