using System;
using UnityEngine;
using UnityEditor;
using System.Collections;

[Serializable]
public class SplitterElement : IDrawable
{
	private const int cursorRectHeight = 10;
	[SerializeField]
	private float[] percentage = { 0.5f, 0.5f };
	[SerializeField]
	private int[] min = { 50, 50};
	[SerializeField]
	private float[] realheight = new[] { 0.0f, 0.0f };
	[SerializeField]
	private IDrawable[] children = new IDrawable[] {null, null}; 

	private Rect rect;
	private Rect subRect;
	private Rect cursorRect;
	private bool resizing = false;

	public SplitterElement(IDrawable first, IDrawable second, float firstSize, float secondSize, int minSizeFirst, int minSizeSecond)
	{
		children = new[] { first, second };
		percentage = new[] { firstSize, secondSize };
		min = new[] { minSizeFirst, minSizeSecond };
		realheight = new[] { 0.0f, 0.0f };
	}	

	public void SetChildren(IDrawable first, IDrawable second)
	{
		children = new IDrawable[] { first, second };
	}

	public void Draw(EditorWindow window, Rect rect, Event current)
	{		
		resizing |= current.type == EventType.MouseDown && cursorRect.Contains (current.mousePosition);
		resizing &= current.type != EventType.MouseUp;

		if (resizing && (current.type == EventType.MouseDrag)) 
		{			
			realheight [0] = Mathf.Clamp (current.mousePosition.y - rect.y, min [0], rect.height - min [1]);
			percentage [0] = realheight [0] / rect.height;
			percentage [1] = 1.0f - percentage [0];
			cursorRect.y = current.mousePosition.y;
		}

		if (current.type != EventType.used && current.type != EventType.Layout) 
		{
			for (int i = 0; i < percentage.Length; i++) 
			{
				realheight [i] = (rect.height * percentage [i]);
			}
		}

		EditorGUIUtility.AddCursorRect (cursorRect, MouseCursor.SplitResizeUpDown);

		for (int i = 0; i < percentage.Length; i++)
		{			
			subRect = EditorGUILayout.BeginVertical ((window as ConsoleWindow).Styles.Box, GUILayout.MaxHeight (realheight [i]), GUILayout.ExpandHeight (true));

			if (children != null && children [i] != null)
			{
				children [i].Draw (window, subRect, current);
			} 
			else
			{
				GUILayout.Box ("", (window as ConsoleWindow).Styles.Box, GUILayout.ExpandHeight (true), GUILayout.ExpandWidth (true));
			}

			EditorGUILayout.EndVertical ();
		}



		if (current.type != EventType.Layout && !resizing) 
		{
			cursorRect = subRect;
			cursorRect.height = cursorRectHeight;
			cursorRect.y -= cursorRectHeight * 0.5f;
		}
	}
}
