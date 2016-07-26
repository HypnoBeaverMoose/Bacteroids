using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ConsoleWindow : EditorWindow
{
	private const int ToolbarHeight = 18;
	private const float ConsoleListStartHeight = 0.8f;
	private const int BottomListMinHeight = 50;
	private readonly RectOffset mainRectOffset = new RectOffset(0,0,0,10);
	public Material Grayscale;
	public ConsoleStyles Styles = null;

	[MenuItem("Window/CustomConsole")]
    public static void ShowWindow()
    {
		EditorWindow.GetWindow<ConsoleWindow> ("Console");
    }

	[MenuItem("Assets/Save Editor Skin")]
	static public void SaveEditorSkin()
	{
		GUISkin skin = Instantiate(GUI.skin);
		AssetDatabase.CreateAsset(skin, "Assets/Main.guiskin");
	}

	private bool needsRepaint = false;
	[SerializeField]
    private bool clearOnPlay = false;
	[SerializeField]
    private bool errorPause = false;

	private bool stylesValid = false;

    private List<IToolbarElement> leftToolbar = new List<IToolbarElement>();
	private List<IToolbarElement> filterToolbar = new List<IToolbarElement>();

	[SerializeField]
	private ConsoleSearchBar searchBar = null;
	[SerializeField]
	private ConsoleList messageList = null;
	[SerializeField]
	private SplitterElement splitter = null;
	[SerializeField]
	private TraceList traceList = null;

	public ConsoleWindow()
	{
		LogHandler.OnMessageLogged += AddMessage;
	}

	private void OnEnable()
	{
		if (messageList == null)
		{
			messageList = new ConsoleList ();
		}

		if (clearOnPlay)
		{
			messageList.Clear ();
		}

		if (searchBar == null)
		{
			searchBar = new ConsoleSearchBar ();
		}

		if (traceList == null)
		{
			traceList = new TraceList ();
		}

		if (splitter == null)
		{
			splitter = new SplitterElement (messageList, null, 0.9f, 0.1f, 30, 30);
		}

		splitter.SetChildren (messageList, traceList);
		searchBar.TextChanged += messageList.TextFilter;

		leftToolbar = new List<IToolbarElement>();
		leftToolbar.Add(new ToolbarButton(new GUIContent("Clear", "Clear Console"), Clear, 5));
		leftToolbar.Add(new ToolbarToggle(new GUIContent("Collapse", "Collapse equal rows"), (bool val) => { messageList.Collapse = val; }, 0, messageList.Collapse));
		leftToolbar.Add(new ToolbarToggle(new GUIContent("Clear on Play", "Clear on play"), (bool val) => { clearOnPlay = val; }, 0, clearOnPlay));
		leftToolbar.Add(new ToolbarToggle(new GUIContent("Error Pause", "Stop playback on error"), (bool val) => { errorPause = val; }, 0, errorPause));

		filterToolbar = new List<IToolbarElement>();
		filterToolbar.Add(new ToolbarTypeToggle(LogType.Log, (bool val) =>  messageList.TypeFilter( LogType.Log, val), messageList.IsFilterSet(LogType.Log)));
		filterToolbar.Add(new ToolbarTypeToggle(LogType.Warning, (bool val) =>  messageList.TypeFilter( LogType.Warning, val), messageList.IsFilterSet(LogType.Warning)));
		filterToolbar.Add(new ToolbarTypeToggle(LogType.Error, (bool val) =>  messageList.TypeFilter( LogType.Error, val), messageList.IsFilterSet(LogType.Error)));
		//hideFlags = HideFlags.HideAndDontSave;
		stylesValid = false;
	}

	void OnDestroy()
	{
		Debug.Log ("Destroyed");
	}

	public void AddMessage(LogHandler.LogInfo info)
	{
		messageList.AddMessage (info);
		EditorApplication.isPaused |= errorPause && (info.Type == LogType.Error || info.Type == LogType.Exception);
		//Repaint ();

	}

	public void Clear()
	{
		messageList.Clear ();
	}

	public void ToggleClearOnPlay(bool enable)
	{
		clearOnPlay = enable;
	}

	public void TogglePauseOnError(bool enable)
	{
		errorPause = enable;
	}

    private void Update ()
	{	
		traceList.Trace = ConsoleMessage.Selected != null ? ConsoleMessage.Selected.Trace : "";
		messageList.Update ();
		filterToolbar [0].Content = new GUIContent (messageList.MessageCount (LogType.Log).ToString());
		filterToolbar [1].Content = new GUIContent (messageList.MessageCount (LogType.Warning).ToString());
		filterToolbar [2].Content = new GUIContent (messageList.MessageCount (LogType.Error).ToString());
		//Repaint ();
	}

	private void OnGUI()
	{

		Event current = Event.current;
		stylesValid = Styles != null;
		needsRepaint = needsRepaint || current.type == EventType.ScrollWheel;

		if (!stylesValid) {
			SetupStyles ();
		}
		try 
		{
			GUILayout.BeginArea (new Rect (0, 0, this.position.width, this.position.height), "", Styles.Box);
			{
				EditorGUILayout.BeginVertical ();
				{				
					GUILayout.BeginHorizontal (Styles.ToolbarStyle, GUILayout.Height (ToolbarHeight));
					{
						foreach (var element in leftToolbar) {
							element.Update (this);
							element.Draw ();
						}

						searchBar.Draw (Styles, current);

						foreach (var element in filterToolbar) {
							element.Update (this);
							element.Draw ();
						}
					}
					GUILayout.EndHorizontal ();  

					var rect = EditorGUILayout.BeginVertical (GUILayout.ExpandHeight (true));

					splitter.Draw (this, mainRectOffset.Add (rect), current);

					EditorGUILayout.EndVertical ();
				}
				EditorGUILayout.EndVertical ();
			}
			GUILayout.EndArea ();
			Repaint ();
		} 
		catch (Exception ex) 
		{
			LogHandler.DefaultHandler.LogException (ex, null);
		}
	}

	private void SetupStyles()
	{
		Styles = new ConsoleStyles ();
		Styles.WarningIcon = GUI.skin.GetStyle ("CN EntryWarn").normal.background;
		Styles.ErrorIcon = GUI.skin.GetStyle ("CN EntryError").normal.background;
		Styles.LogIcon = GUI.skin.GetStyle ("CN EntryInfo").normal.background;
		Styles.Message = new GUIStyle(GUI.skin.GetStyle ("CN Message"));
		Styles.MessageBackgroundEven = new GUIStyle(GUI.skin.GetStyle ("CN EntryBackEven"));
		Styles.MessageBackgroundOdd = new GUIStyle(GUI.skin.GetStyle ("CN EntryBackOdd"));
		Styles.MessageEntryError = new GUIStyle(GUI.skin.GetStyle ("CN EntryError"));
		Styles.MessageEntryWarning = new GUIStyle(GUI.skin.GetStyle ("CN EntryWarn"));
		Styles.MessageEntryLog = new GUIStyle(GUI.skin.GetStyle ("CN EntryInfo"));
		Styles.ToolbarSearchField = new GUIStyle(GUI.skin.GetStyle ("ToolbarSeachTextField"));
		Styles.ToolbarCancelSearchButton = new GUIStyle(GUI.skin.GetStyle ("ToolbarSeachCancelButton"));
		Styles.Box = new GUIStyle(GUI.skin.GetStyle ("CN Box"));
		Styles.CountBadge = new GUIStyle(GUI.skin.GetStyle ("CN CountBadge"));
		Styles.ToolbarStyle = new GUIStyle(EditorStyles.toolbar);
		Styles.ToolbarButton = new GUIStyle(EditorStyles.toolbarButton);
		Styles.Grayscale = Grayscale;
		stylesValid = true;	
	}		


	void OnDisable()
	{
		LogHandler.OnMessageLogged -= AddMessage;
	}
}