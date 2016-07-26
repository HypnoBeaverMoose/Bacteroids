using UnityEngine;
using UnityEditor;
using System.Collections;

public class TraceList : IDrawable
{

	public string Trace = "";

	private Vector2 scrollPos;
	public TraceList()
	{

	}

	public void Draw (EditorWindow window, Rect rect, Event current)
	{
		
		scrollPos = GUILayout.BeginScrollView (scrollPos, (window as ConsoleWindow).Styles.Box);

		EditorGUILayout.SelectableLabel (Trace, (window as ConsoleWindow).Styles.Message, new GUILayoutOption[]
		{
			GUILayout.ExpandHeight(true), 
			GUILayout.ExpandWidth(true)
		});

		GUILayout.EndScrollView ();
	}
}
