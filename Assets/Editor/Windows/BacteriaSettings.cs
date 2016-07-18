using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class BacteriaSettings : EditorWindow {

    [MenuItem("Bacteria/Settings")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<BacteriaSettings>("Settings");
    }

    private const string nodePath = "Assets/Prefabs/node.prefab";
    private const string bacteriaPath = "Assets/Prefabs/bacteria2.prefab";

    private List<Node> _nodes = new List<Node>();
    private List<Bacteria> _bacteria = new List<Bacteria>();

    [SerializeField]
    private int _verticies;
    [SerializeField]
    private float _radius;
    [SerializeField]
    private Bacteria.CollisionType _collision;


    [SerializeField]
    private float _frequency;
    [SerializeField]
    private float _damping;
    [SerializeField]
    private bool _constrain;

    [SerializeField]
    private float _minDistance;
    [SerializeField]
    private float _maxDistance;

    [SerializeField]
    private float _mass;
    [SerializeField]
    private float _drag;

    [SerializeField]
    private float _pivotMass;
    [SerializeField]
    private float _pivotDrag;
    [SerializeField]
    private float _pivotAngularDrag;
    [SerializeField]
    private float _pivotFrequency;
    [SerializeField]
    private float _pivotDamping;
   
    [SerializeField]
    private bool _autoAnchor;
    [SerializeField]
    private bool _autoDistance;

    private bool _convergent = true;
    private float _lastUpdate;
    private float _upateTime = 1.0f;
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
        _minDistance = node.MinDistance;
        _maxDistance = node.MaxDistance;
        _maxDistance = node.MaxDistance;
        _mass = node.Body.mass;
        _drag = node.Body.drag;
        _pivotFrequency = node.PivotFrequency;
        _pivotDamping = node.PivotDamping;


        _verticies = bacteria.Vertices;
        _radius = bacteria.Radius;
        _autoAnchor = false;
        _autoDistance = false;
        _collision = bacteria.Collisions;
        _pivotMass = bacteria.GetComponent<Rigidbody2D>().mass;
        _pivotDrag = bacteria.GetComponent<Rigidbody2D>().drag;
        _pivotAngularDrag = bacteria.GetComponent<Rigidbody2D>().angularDrag;
        Repaint();
        serObj.Update();
    }

    private void SetNodeSettings(Node node)
    {
        node.Frequency = _frequency;
        node.Damping = _damping;
        node.MinDistance = _minDistance;
        node.MaxDistance = _maxDistance;
        node.MaxDistance = _maxDistance;
        node.Body.mass = _mass;
        node.Body.drag = _drag;
        node.PivotFrequency = _pivotFrequency;
        node.PivotDamping = _pivotDamping;
    }

    private void SetBacteriaSettings(Bacteria bacteria)
    {
        bacteria.GetComponent<Rigidbody2D>().mass = _pivotMass;
        bacteria.GetComponent<Rigidbody2D>().drag = _pivotDrag;
        bacteria.GetComponent<Rigidbody2D>().angularDrag = _pivotAngularDrag;
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
            foreach (var node in _nodes.FindAll( n=> n.GetComponent<Bacteria>() != null))
            {
                _bacteria.Add(node.GetComponent<Bacteria>());
            }

        }   
        foreach (var node in _nodes)
        {
            node.EditMode = true;
            if (_frequency != node.Frequency ||
                _damping != node.Damping ||
                _minDistance != node.MinDistance ||
                _maxDistance != node.MaxDistance ||
                _maxDistance != node.MaxDistance ||
                _mass != node.Body.mass ||
                _drag != node.Body.drag ||
                _pivotFrequency != node.PivotFrequency ||
                _pivotDamping != node.PivotDamping)
            {
                SetNodeSettings(node);
            }
        }

        foreach (var bacteria in _bacteria)
        {
            if (_pivotMass != bacteria.GetComponent<Rigidbody2D>().mass ||
                _pivotDrag != bacteria.GetComponent<Rigidbody2D>().drag ||
                _pivotAngularDrag != bacteria.GetComponent<Rigidbody2D>().angularDrag)
            {
                bacteria.GetComponent<Rigidbody2D>().mass = _pivotMass;
                bacteria.GetComponent<Rigidbody2D>().drag = _pivotDrag;
                bacteria.GetComponent<Rigidbody2D>().angularDrag = _pivotAngularDrag;
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
            EditorGUILayout.PropertyField(serObj.FindProperty("_radius"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_collision"));

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Node Settings"); 
            EditorGUILayout.PropertyField(serObj.FindProperty("_frequency"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_damping"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_minDistance"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_maxDistance"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_constrain"));

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Node Body Settings");  
            EditorGUILayout.PropertyField(serObj.FindProperty("_mass"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_drag"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_autoDistance"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_autoAnchor"));

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Pivot Body Settings");  
            EditorGUILayout.PropertyField(serObj.FindProperty("_pivotMass"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_pivotDrag"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_pivotAngularDrag"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_pivotFrequency"));
            EditorGUILayout.PropertyField(serObj.FindProperty("_pivotDamping"));
        }
        EditorGUILayout.EndVertical();

        serObj.ApplyModifiedProperties();
        _verticies = Mathf.Max(4, _verticies);
        _radius = Mathf.Max(0.01f, _radius);
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
        node.EditMode = false;
        var bacteria = AssetDatabase.LoadAssetAtPath<GameObject>(bacteriaPath).GetComponent<Bacteria>();
        SetBacteriaSettings(bacteria);
        SetNodeSettings(bacteria.gameObject.GetComponent<Node>());
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
                node.Frequency = _frequency;
                node.Damping = _damping;
                node.MinDistance = _minDistance;
                node.MaxDistance = _maxDistance;
                node.MaxDistance = _maxDistance;
                node.Body.mass = _mass;
                node.Body.drag = _drag;
                node.PivotFrequency = _pivotFrequency;
                node.PivotDamping = _pivotDamping;
                foreach (var joint in node.GetComponentsInChildren<SpringJoint2D>())
                {
                    joint.autoConfigureConnectedAnchor = _autoAnchor;
                    joint.autoConfigureDistance = _autoDistance;
                }
            }

            foreach (var bacteria in _bacteria)
            {
                bacteria.Radius = _radius;
                bacteria.Vertices = _verticies;
                bacteria.Collisions = _collision;
                bacteria.GetComponent<Rigidbody2D>().mass = _pivotMass;
                bacteria.GetComponent<Rigidbody2D>().drag = _pivotDrag;
                bacteria.GetComponent<Rigidbody2D>().angularDrag = _pivotAngularDrag;

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
