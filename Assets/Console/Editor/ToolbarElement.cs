using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public interface IToolbarElement
{
	GUIContent Content { get; set; }

	int Width { get; }

	int Margin { get; }

	void Update(ConsoleWindow window);

	void Draw ();
}


public abstract class BaseToolbarElement : IToolbarElement
{
	protected GUIContent content; 
	protected int width;
	protected GUIStyle style;
	public int Margin { get; set; }

	public int Width
	{
		get { return width; }
	}

	public GUIContent Content
	{
		get
		{
			return content;
		}
		set
		{ 
			content = value;
			width = -1;
		}
	}

	protected virtual void UpdateStyle (ConsoleStyles styles)
	{
		style = styles.ToolbarButton;
	}

	public virtual void Update (ConsoleWindow window)
	{
		UpdateStyle (window.Styles);
	}
		
	public void Draw ()
	{
		
		if (width == -1)
		{
			width = Mathf.CeilToInt (style.CalcSize(content).x);
		}
		//GUILayout.Space(Margin);
		DrawInternal ();
		GUILayout.Space (Margin);
	}

	protected abstract void DrawInternal ();
}

public class ToolbarButton : BaseToolbarElement
{
	public Action Action { get; set; }

	public ToolbarButton (GUIContent content, Action action, int margin)
	{
		Content = content;
		Margin = margin;
		Action = action;        
	}

	protected override void DrawInternal ()
	{        
		if (GUILayout.Button (content, style, GUILayout.Width (width)) && Action != null)
		{
			Action ();
		}
	}
}

public class ToolbarToggle : BaseToolbarElement
{
	public Action<bool> Action { get; set; }
	protected bool value;

	public ToolbarToggle (GUIContent content, Action<bool> action, int margin, bool startValue)
	{
		Content = content;
		Action = action;
		Margin = margin;
		value = startValue;
	}

	protected override void DrawInternal()
	{		
		bool tempVal = GUILayout.Toggle (value, content, style, GUILayout.Width (width));
		if (tempVal != value && Action != null)
		{
			value = tempVal;
			Action (value);
		}
	}
}

public class ToolbarTypeToggle : ToolbarToggle
{
	public LogType Type {get; private set;}

	private static Material grayscale = null;
	private Texture2D texture;
	private const int padding = 18;
	private const int margin = 3;
	private static readonly RectOffset border = new RectOffset (margin, margin, margin, margin);
	public ToolbarTypeToggle (LogType filterType, Action<bool> action, bool enabled) : base(new GUIContent("0"), action, 0, enabled)
	{
		Type = filterType;
	}

	public override void Update (ConsoleWindow window)
	{
		UpdateStyle (window.Styles);
	}

	protected override void UpdateStyle (ConsoleStyles styles)
	{		
		base.UpdateStyle (styles);
		texture = Type == LogType.Log ? styles.LogIcon : Type == LogType.Warning ? styles.WarningIcon : styles.ErrorIcon;
		texture.filterMode = FilterMode.Trilinear;
		grayscale = styles.Grayscale;
	}

	protected override void DrawInternal ()
	{
		var alignment = style.alignment;
		style.alignment = TextAnchor.MiddleRight;

		bool tempVal = GUILayout.Toggle (value, content, style, GUILayout.Width (padding + width));
		if (tempVal != value && Action != null)
		{
			value = tempVal;
			Action (value);
		}
		style.alignment = alignment;

		var lastRect = border.Add(GUILayoutUtility.GetLastRect ());
		lastRect.width -= width;
		lastRect.x += margin;

		int count = int.Parse (content.text);
		Graphics.DrawTexture(lastRect, texture, count == 0 ? grayscale : null);
	}
		
}
