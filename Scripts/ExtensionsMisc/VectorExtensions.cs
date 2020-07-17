using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 ChangeX(this Vector3 v3, float value)
	{
		v3.x = value;
		return v3;
	}

	public static Vector3 ChangeY(this Vector3 v3, float value)
	{
		v3.y = value;
		return v3;
	}

	public static Vector3 ChangeZ(this Vector3 v3, float value)
	{
		v3.z = value;
		return v3;
	}

	public static float RandomRange(this Vector2 v2)
	{
		return UnityEngine.Random.Range(v2.x, v2.y);
	}

	public static Vector3Int ClampLengths(this Vector3Int v3, int value = 1)
	{
		v3.x = Mathf.Clamp(v3.x, -value, value);
		v3.y = Mathf.Clamp(v3.y, -value, value);
		v3.z = Mathf.Clamp(v3.z, -value, value);
		return v3;
	}

	public static Vector2Int RoundToV2Int(this Vector2 v2)
	{
		return new Vector2Int(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
	}

	public static Vector3Int RoundToV3Int(this Vector3 v3)
	{
		return new Vector3Int(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.y), Mathf.RoundToInt(v3.z));
	}

	public static int GetAbsSum(this Vector2Int v2)
	{
		int result = 0;
		result += Mathf.Abs(v2[0]);
		result += Mathf.Abs(v2[1]);
		return result;
	}

	public static int GetAbsSum(this Vector3Int v3)
	{
		int result = 0;
		result += Mathf.Abs(v3[0]);
		result += Mathf.Abs(v3[1]);
		result += Mathf.Abs(v3[2]);
		return result;
	}
}
