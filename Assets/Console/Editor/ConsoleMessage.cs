using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.Text.RegularExpressions;

[Serializable]
public class ConsoleMessage
{
	public static ConsoleMessage Selected { get; private set; }
	private static readonly Regex regex = new Regex("at (?<Path>Assets((/\\w.)*)+/(?<File>\\w.+)\\.cs):(?<Line>[0-9]+)");
	public const int MessageHeight = 32;
	private const int DefaultSplitIndex = 2;

	public LogHandler.LogInfo Data;

	public string Text { get { return MessageText + Environment.NewLine + Trace; } }
	[SerializeField]
	private string trace = "";
	public string Trace { get { return trace; }}
	[SerializeField]
	private string messageText = "";
	public string MessageText { get { return messageText; }}
	[SerializeField]
	private int line = -1;
	public int  Line {get {return line; } }
	[SerializeField]
	private int count = 0;
	public int Count  { get {return count; } set { count = value;  collapsed = count > 0;  } }
	[SerializeField]
	private string filename = "";
	public string  Filename {get {return filename; } }
	[SerializeField]
	private string path = "";
	public string  Path{get {return path; } }
	[SerializeField]
	private bool collapsed = false;
	[SerializeField]
	private string firstLine = "";

	private Rect badgeRect;
	private Rect messageRect;

	public ConsoleMessage(LogHandler.LogInfo info)
	{
		Data = info;
		messageText = string.Format (Data.Format, Data.Args);
		firstLine = ExtractSpawnLine (Data.Trace);
	}

	public ConsoleMessage(ConsoleMessage message)
	{
		Data = message.Data;
		trace = message.trace;
		messageText = message.messageText;
		line = message.line;
		count = message.count;
		filename = message.filename;
		path = message.path;
		firstLine = message.firstLine;
		collapsed = message.collapsed;
	}


	public void Draw(int index, ConsoleStyles styles, Event current)
	{
		var style = index % 2 == 0 ? styles.MessageBackgroundOdd : styles.MessageBackgroundEven;
		GUILayout.Box ("", styles.Message, GUILayout.Height (MessageHeight), GUILayout.ExpandWidth (true));
		messageRect = GUILayoutUtility.GetLastRect ();
		GUI.Box (messageRect, "", style);
		if (GUI.Toggle (messageRect, Selected == this, "", styles.Message))
		{
			if (trace == "") 
			{
				trace = FormatTrace (Data.Trace);
			}
			Selected = this;
		}

		GUI.Toggle (messageRect, Selected == this, messageText + Environment.NewLine + firstLine, MessageStyle (styles));

		if (collapsed)
		{
			badgeRect.size = styles.CountBadge.CalcSize (new GUIContent (Count.ToString ()));
			badgeRect.center = new Vector2 (messageRect.max.x - badgeRect.width* 0.5f - (messageRect.height - badgeRect.height) * 0.5f, messageRect.y + messageRect.height * 0.5f);
			GUI.Label (badgeRect, Count.ToString (), styles.CountBadge);
		}
		if (current.clickCount == 1 && messageRect.Contains (current.mousePosition))
		{
			
			if (Data.Context != null) 
			{
				Selection.activeObject = Data.Context;
			}
		}
		if (current.clickCount == 2 && messageRect.Contains (current.mousePosition))
		{
			UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal (path, line);
		}
	}

	private GUIStyle MessageStyle(ConsoleStyles styles)
	{		
		return Data.Type == LogType.Log ? styles.MessageEntryLog : Data.Type == LogType.Warning ? styles.MessageEntryWarning : styles.MessageEntryError;
	}

	public string UniqueID()
	{				
		return string.Join("|", new [] { (Data.Context == null ? "" : Data.Context.ToString ()), Data.Type.ToString (), filename,  messageText, Data.Trace });
	}

	private string ExtractSpawnLine(string trace)
	{
		if (trace.Length == 0) 
		{
			return "";
		}
		int start = 0;
		for (int i = 0; i < DefaultSplitIndex; i++) 
		{
			int ind =  trace.IndexOf ('\n', start + 1);
			if (ind == -1) 
			{
				break;
			}
			start = ind;
		}
		int end = trace.IndexOf ('\n', start + 1);
		return trace.Substring (start + 1, end == -1 ? trace.Length : end - start);	
	}

	private string FormatTrace(string trace)
	{
		if (trace.Length == 0) {
			return "";
		}

		string tr = "";
		var split = new List<string> (trace.Split (new [] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
		bool filenameFound = false;
		int spawnIndex = Mathf.Min (DefaultSplitIndex, split.Count - 1);
		for (int i = spawnIndex; (i < split.Count) && !filenameFound; i++) {
			var stackLine = split [i].Replace ("\\", "/");
			Match match = regex.Match (stackLine);

			if (match.Success) {
				filename = match.Groups ["File"].Value;
				path = match.Groups ["Path"].Value;
				line = int.Parse (match.Groups ["Line"].Value);
				filenameFound = true;
			}

		}
		for (int i = spawnIndex; i < split.Count; i++) {
			tr += split [i] + Environment.NewLine;
		}
		return tr;
	}
	
}
