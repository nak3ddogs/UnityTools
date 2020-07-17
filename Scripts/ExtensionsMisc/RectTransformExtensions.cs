using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectTransformExtensions
{
	public static bool ContainsPoint(this RectTransform rt, Vector2 point)
	{
		var rect = rt.rect;
		rect.center += rt.anchoredPosition;
		return rect.Contains(point);
	}
}
