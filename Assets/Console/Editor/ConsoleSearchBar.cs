using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ConsoleSearchBar
{
	private const int margin = 10;
	private const string controlName = "searchText";

	public event Action<string> TextChanged;
	public string SearchText;

	private readonly Dictionary<string, System.Action<TextEditor>> commandActions = new Dictionary<string, System.Action<TextEditor>>();
	private string tempText = "";   

	public ConsoleSearchBar()
    {
		SearchText = "";
		commandActions.Add ("Copy", te => te.Copy ());
		commandActions.Add ("Paste",  te => te.Paste ());
		commandActions.Add ("Cut", te => te.Cut ());
    }
		
	public void Draw(ConsoleStyles styles, Event current)
    {		
		GUILayout.Space (margin);

		GUI.SetNextControlName (controlName);
		tempText = GUILayout.TextField(SearchText, styles.ToolbarSearchField, GUILayout.MaxWidth(1000));
		EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect (), MouseCursor.Text);

		if (GUI.GetNameOfFocusedControl() == controlName)
		{
			if (current.type == EventType.ValidateCommand && commandActions.ContainsKey(current.commandName))
			{
				current.Use ();
			}

			if (current.type == EventType.ExecuteCommand)
			{
				var textEditor = GUIUtility.GetStateObject (typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
				commandActions [current.commandName] (textEditor);
				tempText = textEditor.text;
			}
				
		}
		if (SearchText != tempText)
		{
			SearchText = tempText;
			if (TextChanged != null)
			{
				TextChanged (SearchText);
			}
		}
		if (GUILayout.Button("", styles.ToolbarCancelSearchButton))
        {            
            Clear();
        }
		GUILayout.Space (margin);
    }

	public void Clear()
	{
		SearchText = "";
	}

}
