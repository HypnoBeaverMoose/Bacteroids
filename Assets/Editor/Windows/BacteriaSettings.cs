using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BacteriaSettings : EditorWindow {

    [MenuItem("Bacteria/Settings")]
    public static void ShowWindow()
    {
        GetWindow<BacteriaSettings>("Settings");
    }

    private const string nodePath = "Assets/Prefabs/node.prefab";
    private const string bacteriaPath = "Assets/Prefabs/bacteria.prefab";

    private List<Node> _nodes = new List<Node>();
    private List<Bacteria> _bacteria = new List<Bacteria>();

    [SerializeField]
    private int _verticies;
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _frequency;
    [SerializeField]
    private float _damping;
    [SerializeField]
    private float _mass;
    [SerializeField]
    private float _drag;

    [SerializeField]
    private float _centerMass;
    [SerializeField]
    private float _centerDrag;
    [SerializeField]
    private float _centerAngularDrag;

    [SerializeField]
    private Bacteria.NodeConnection[] _connections;

    private float _lastUpdate;
    private float _upateTime = 0.2f;
    private bool _needsUpdate;
    [SerializeField]
    private bool useSelection = false;
    [SerializeField]
    private bool autoUpdate = false;

    private SerializedObject serObj;
    private GUIContent saveButton = new GUIContent("Save");
    private GUIContent loadButton = new GUIContent("Load");
    private GUIContent selectionToggle = new GUIContent("Selection");
    private GUIContent autoUpdateToggle = new GUIContent("Auto Update");
    private GUIContent updateButton = new GUIContent("Update");
    private GUIContent regenerateButton = new GUIContent("Regenerate");

    private void OnEnable()
    {        
        serObj = new SerializedObject(this);
        _lastUpdate = Time.time;
        LoadSettings(AssetDatabase.LoadAssetAtPath<GameObject>(nodePath).GetComponent<Node>(), AssetDatabase.LoadAssetAtPath<GameObject>(bacteriaPath).GetComponent<Bacteria>());
    }

    private void LoadSettings(Node node, Bacteria bacteria)
    {
        _frequency = node.Frequency;
        _damping = node.Damping;
        _mass = node.Body.mass;
        _drag = node.Body.drag;
        _verticies = bacteria.Vertices;
        _centerMass = bacteria.GetComponent<Rigidbody2D>().mass;
        _centerDrag = bacteria.GetComponent<Rigidbody2D>().drag;
        _centerAngularDrag = bacteria.GetComponent<Rigidbody2D>().angularDrag;
        _connections = bacteria.nodeConnections;
        _radius = bacteria.Radius;
        Repaint();
        serObj.Update();
    }

    private void SetNodeSettings(Node node)
    {
        node.Frequency = _frequency;
        node.Damping = _damping;
        node.Body.mass = _mass;
        node.Body.drag = _drag;
    }

    private void SetBacteriaSettings(Bacteria bacteria)
    {
        bacteria.GetComponent<Rigidbody2D>().mass = _centerMass;
        bacteria.GetComponent<Rigidbody2D>().drag = _centerDrag;
        bacteria.GetComponent<Rigidbody2D>().angularDrag = _centerAngularDrag;
        bacteria.nodeConnections = _connections;
        bacteria.Radius = _radius;
        bacteria.Vertices = _verticies;
        bacteria.Reconnect();
    }

    private void GatherAndApply()
    {
        if (useSelection)
        {
            var obj = Selection.activeGameObject;
            if (obj == null)
            {
                return;
            }
            _bacteria = new List<Bacteria>();
            _bacteria.Add(obj.GetComponent<Bacteria>());
            _nodes = new List<Node>(obj.GetComponentsInChildren<Node>());
            if (_bacteria.Count == 0 || _bacteria[0] == null || _nodes.Count == 0)
            {
                _bacteria.Clear();
                _nodes.Clear();
                return;
            }
        }
        else
        {
            _nodes = new List<Node>(FindObjectsOfType<Node>());
            _bacteria = new List<Bacteria>();
            foreach (var node in _nodes.FindAll(n => n.GetComponent<Bacteria>() != null))
            {
                _bacteria.Add(node.GetComponent<Bacteria>());
            }

        }
        foreach (var node in _nodes)
        {
            if (_frequency != node.Frequency ||
                _damping != node.Damping ||
                _mass != node.Body.mass ||
                _drag != node.Body.drag)
            {
                SetNodeSettings(node);
            }
        }

        foreach (var bacteria in _bacteria)
        {
            bool same = bacteria.nodeConnections.Length ==_connections.Length;
            if (same)
            {
                for (int i = 0; i < _connections.Length; i++)
                {
                    if (bacteria.nodeConnections[i].AutoDistance != _connections[i].AutoDistance ||
                        bacteria.nodeConnections[i].Damping != _connections[i].Damping ||
                        bacteria.nodeConnections[i].Frequency != _connections[i].Frequency ||
                        bacteria.nodeConnections[i].IndexType != _connections[i].IndexType)
                    {
                        same = false;
                        break;
                    }
                }
            }

            if (!same ||
                _centerMass != bacteria.GetComponent<Rigidbody2D>().mass ||
                _centerDrag != bacteria.GetComponent<Rigidbody2D>().drag ||
                _centerAngularDrag != bacteria.GetComponent<Rigidbody2D>().angularDrag)
            {
                SetBacteriaSettings(bacteria);
            }
        }
        serObj.Update();
    }

    private void OnGUI()
    {        
        EditorGUILayout.BeginVertical();
        {

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(updateButton, EditorStyles.toolbarButton, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(updateButton).x)))
            {
                _needsUpdate = true;
            }

            if (GUILayout.Button(regenerateButton, EditorStyles.toolbarButton, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(regenerateButton).x)))
            {
                Regenerate();
            }
            GUILayout.Space(7);
            bool before = useSelection;
            useSelection = GUILayout.Toggle(useSelection, selectionToggle, EditorStyles.toolbarButton, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(selectionToggle).x)); if (useSelection && !before)
            {
                Selection.selectionChanged += GatherAndApply;
                GatherAndApply();
            }
            else if (before && !useSelection)
            {
                Selection.selectionChanged -= GatherAndApply;
                GatherAndApply();
            }

            autoUpdate = GUILayout.Toggle(autoUpdate, autoUpdateToggle, EditorStyles.toolbarButton, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(autoUpdateToggle).x));

            EditorGUILayout.Space();

            if (GUILayout.Button(saveButton, EditorStyles.toolbarButton, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(saveButton).x)))
            {
                SaveSettings();
            }

            if (GUILayout.Button(loadButton, EditorStyles.toolbarButton, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(loadButton).x)))
            {
                LoadSettings(AssetDatabase.LoadAssetAtPath<GameObject>(nodePath).GetComponent<Node>(), AssetDatabase.LoadAssetAtPath<GameObject>(bacteriaPath).GetComponent<Bacteria>());
            }


            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Bacteria Settings");
            EditorGUILayout.PropertyField(serObj.FindProperty("_verticies"), true);
            EditorGUILayout.PropertyField(serObj.FindProperty("_radius"), true);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Node Settings"); 
            EditorGUILayout.PropertyField(serObj.FindProperty("_frequency"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_damping"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_mass"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_drag"));

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Pivot Body Settings");
            EditorGUILayout.PropertyField(serObj.FindProperty("_centerMass"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_centerDrag"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_centerAngularDrag"));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Connections");
            EditorGUILayout.PropertyField(serObj.FindProperty("_connections"),true);
        }

        EditorGUILayout.EndVertical();

        serObj.ApplyModifiedProperties();
        _verticies = Mathf.Max(Bacteria.MinVertexCount, _verticies);
        serObj.Update();
    }
    private void Regenerate()
    {
        foreach (var bacteria in _bacteria)
        {
            bacteria.Regenerate();
        }
    }

    public void SaveSettings()
    {
        var node = AssetDatabase.LoadAssetAtPath<GameObject>(nodePath).GetComponent<Node>();
        SetNodeSettings(node);
        var bacteria = AssetDatabase.LoadAssetAtPath<GameObject>(bacteriaPath).GetComponent<Bacteria>();
        SetNodeSettings(bacteria.gameObject.GetComponent<Node>());
        SetBacteriaSettings(bacteria);
        EditorUtility.SetDirty(node.gameObject);
        EditorUtility.SetDirty(bacteria.gameObject);
    }

    private void UpdateBacterias()
    {
        if (_nodes.Contains(null) || _bacteria.Contains(null))
        {
            _nodes.Clear();
            _bacteria.Clear();
        }

        if (_nodes != null)
        {
            foreach (var node in _nodes)
            {
                SetNodeSettings(node);
                //node.Frequency = _frequency;
                //node.Damping = _damping;
                //node.MinDistance = _minDistance;
                //node.MaxDistance = _maxDistance;
                //node.MaxDistance = _maxDistance;
                //node.Body.mass = _mass;
                //node.Body.drag = _drag;
                //node.PivotFrequency = _pivotFrequency;
                //node.PivotDamping = _pivotDamping;
            }

            foreach (var bacteria in _bacteria)
            {
                SetBacteriaSettings(bacteria);
            }
        }
    }



    // Update is called once per frame
	void Update () 
    {
        if (autoUpdate || _needsUpdate)
        {
            UpdateBacterias();
            _needsUpdate = false;
        }

        if (_lastUpdate + _upateTime < Time.time)
        {
            GatherAndApply();
            _lastUpdate = Time.time;
            Repaint();
        }
	}

    private void OnDisable()
    {
        _bacteria.Clear();
        _nodes.Clear();
    }
}
