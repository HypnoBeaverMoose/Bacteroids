using UnityEngine;
using UnityEditor;
using System.Collections;

public interface IDrawable
{
	void Draw (EditorWindow window, Rect rect, Event current);
}
