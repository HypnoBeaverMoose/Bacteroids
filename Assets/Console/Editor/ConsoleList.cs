using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[Serializable]
public class ConsoleList : IDrawable, ISerializationCallbackReceiver
{
	private const float minMessagePadding = 5;
	private const float minHeight = 50;

	private Dictionary<LogType, int> messageCounter = new Dictionary<LogType, int>();
	private Dictionary<string, ConsoleMessage> collapsedMessages = new Dictionary<string, ConsoleMessage> ();

	[SerializeField]
	private List<ConsoleMessage> allMessages = new List<ConsoleMessage>();
	[SerializeField]
	private List<ConsoleMessage> filteredMessages = new List<ConsoleMessage> ();
	[SerializeField]
	private List<ConsoleMessage> collapsedValues = new List<ConsoleMessage> ();
	[SerializeField]
	private List<string> collapsedKeys = new List<string> ();
	[SerializeField]
	private List<LogType> counterKeys = new List<LogType>();
	[SerializeField]
	private List<int> counterValues = new List<int>();
	[SerializeField]
	private List<LogType> typeFilter = new List<LogType>();

	private string textFilter = "";
	private Object objectFilter = null;

	private float spaceBefore = 0;
	private float spaceAfter = 0;
	private int messageOffset = 0;
	private int messageCount = 0;
	private bool needsUpdate = false;

	public bool Collapse { get {return collapse; } set {collapse = value; needsUpdate = true; } }
	private Vector2 scrollPos;
	private Rect viewRect;
	[SerializeField]
	private bool collapse;
	private bool resizing;
	private bool autoscroll = true;
	private int addedMessages = 0;
	private int messagePadding = 100;
	private int contentHeight = 0;
	private string id;
	public ConsoleList()
	{
		messageCounter.Add (LogType.Log, 0);
		messageCounter.Add (LogType.Warning, 0);
		messageCounter.Add (LogType.Error, 0);
		needsUpdate = true;
	}

	public void AddMessage(LogHandler.LogInfo info)
	{		
		addedMessages++;
		messageCounter [info.Type]++;
		ConsoleMessage message = new ConsoleMessage (info);
		allMessages.Add (message);

		if (!collapse && FilterMessage(message)) 
		{
			filteredMessages.Add (message);
		}

		id = allMessages [allMessages.Count - 1].UniqueID ();
		bool isNew = false;
		if (collapsedMessages.TryGetValue (id, out message)) 
		{
			message.Count++;
		} 
		else 
		{		
			message = new ConsoleMessage (allMessages [allMessages.Count - 1]);
			message.Count = 1;
			collapsedMessages.Add (id, message);
			isNew = true;
		}

		if (collapse && isNew && FilterMessage(message)) 
		{
			filteredMessages.Add (message);
		}

		scrollPos.y = autoscroll ? filteredMessages.Count * ConsoleMessage.MessageHeight : scrollPos.y;
	}

	public void Clear()
	{
		messageCounter.Clear ();
		messageCounter.Add (LogType.Log, 0);
		messageCounter.Add (LogType.Warning, 0);
		messageCounter.Add (LogType.Error, 0);

		allMessages = new List<ConsoleMessage>();
		filteredMessages = new List<ConsoleMessage> ();
		collapsedMessages = new Dictionary<string, ConsoleMessage> ();
		addedMessages = 0;
		needsUpdate = true;
	}

	public bool IsFilterSet(LogType type)
	{
		return typeFilter.Contains (type);
	}

	public int MessageCount(LogType type)
	{
		return messageCounter.ContainsKey(type) ? messageCounter [type]: 0;
	}

	public void TypeFilter(LogType type, bool enabled)
	{
		if (enabled)
		{
			typeFilter.Add(type);
		}
		else
		{
			typeFilter.Remove(type);
		}
		needsUpdate = true;
	}

	public void TextFilter(string text)
	{
		textFilter = text.Trim();
		needsUpdate = true;
	}

	public void ObjectFilter(Object obj)
	{
		objectFilter = obj;
		needsUpdate = true;
	}

	public void Draw (EditorWindow window, Rect rect, Event current)
	{				

		if (current.type == EventType.Repaint) 
		{
			viewRect = rect;
		}
		scrollPos = EditorGUILayout.BeginScrollView (scrollPos, (window as ConsoleWindow).Styles.Box);
		{
			GUILayout.BeginVertical ();
			{
				if (spaceBefore > 0)
					GUILayout.Space (spaceBefore);
				
				for (int i = 0; i < messageCount; i++) {
					int index = messageOffset + i;
					if (index < filteredMessages.Count) {
						filteredMessages [index].Draw (index, (window as ConsoleWindow).Styles, current);
					}
				}

				if (spaceAfter > 0)
					GUILayout.Space (spaceAfter);
			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndScrollView ();

	}
	public void Update()
	{
		UpdateFilters ();
		UpdateSpaces (viewRect);
	}

	private void UpdateFilters()
	{
		if(needsUpdate)
		{
			Predicate<ConsoleMessage> pred = mess => typeFilter.Contains (mess.Data.Type);
			filteredMessages.Clear ();
			filteredMessages.AddRange( Collapse == true ?  collapsedMessages.Values.ToList().FindAll(pred) : allMessages.FindAll(pred));

			if (textFilter.Length > 0)
			{
				filteredMessages.RemoveAll(mess => !mess.Text.Contains(textFilter));
			}
			if (objectFilter != null)
			{
				filteredMessages.RemoveAll(mess => !mess.Data.Context.Equals(objectFilter));
			}
			scrollPos.y = filteredMessages.Count  * ConsoleMessage.MessageHeight + viewRect.height;
			needsUpdate = false;
		}
	}

	private void UpdateSpaces(Rect rect)
	{
		contentHeight = filteredMessages.Count * ConsoleMessage.MessageHeight;

		bool before = autoscroll;
		autoscroll = (contentHeight - scrollPos.y) <= (viewRect.height + ConsoleMessage.MessageHeight);
		if (before && !autoscroll) 
		{
			LogHandler.DefaultHandler.LogFormat (LogType.Log, null, "{0}", (contentHeight - scrollPos.y) - (viewRect.height + ConsoleMessage.MessageHeight));
		}
		messagePadding = 2 * (int)Mathf.Max (minMessagePadding, addedMessages);
		messageOffset = Mathf.Max ((int)(scrollPos.y / ConsoleMessage.MessageHeight) - messagePadding, 0);
		messageCount = Mathf.Min ((int)(rect.height / ConsoleMessage.MessageHeight) + 2 * messagePadding, filteredMessages.Count);
		spaceBefore = messageOffset * ConsoleMessage.MessageHeight;
		spaceAfter = (filteredMessages.Count - (messageCount + messageOffset)) * ConsoleMessage.MessageHeight;
		addedMessages = 0;

		scrollPos.y = autoscroll || scrollPos.y > filteredMessages.Count * ConsoleMessage.MessageHeight ? filteredMessages.Count * ConsoleMessage.MessageHeight + viewRect.height : scrollPos.y;
	}

	private bool FilterMessage(ConsoleMessage message)
	{
		if (textFilter.Length > 0 && !message.Text.Contains(textFilter))
		{
			return false;	
		}
		if (objectFilter != null && !message.Data.Context.Equals(objectFilter))
		{
			return false;
		}

		return typeFilter.Contains (message.Data.Type);
	}


	public void OnBeforeSerialize()
	{
		counterKeys .Clear ();
		counterValues.Clear ();
		foreach (var counter in messageCounter) 
		{
			counterKeys.Add (counter.Key);	
			counterValues.Add (counter.Value);
		}
		collapsedKeys.Clear ();
		collapsedValues.Clear ();
		foreach (var message in collapsedMessages) 
		{
			collapsedKeys.Add (message.Key);	
			collapsedValues.Add (message.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		collapsedMessages = new Dictionary<string, ConsoleMessage> ();
		for(int i = 0; i < collapsedKeys.Count; i++)
		{
			collapsedMessages.Add (collapsedKeys [i], collapsedValues [i]);
		}
		messageCounter = new Dictionary<LogType, int> ();
		for(int i = 0; i < counterKeys.Count; i++)
		{
			messageCounter.Add (counterKeys [i], counterValues [i]);
		}
		collapsedKeys = new List<string> ();
		collapsedValues = new List<ConsoleMessage> ();
	}
}

