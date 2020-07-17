using UnityEngine;
using System.Collections;

public static class MyMath
{

	public static float Clamp(float value, float minMax)
	{
		return Mathf.Clamp(value, -minMax, minMax);
	}

	public static int Clamp(int value, int minMax)
	{
		return Mathf.Clamp(value, -minMax, minMax);
	}

}
public static class RRandom
{
	public static bool Bool
	{
		get
		{
			return UnityEngine.Random.value > 0.5f;
		}
	}

	public static float Value_Neg1_To_Pos1
	{
		get
		{
			return (UnityEngine.Random.value - 0.5f) * 2f;
		}
	}

	public static float Value_Neg1_Or_Pos1
	{
		get
		{
			return Mathf.Sign(Value_Neg1_To_Pos1);
		}
	}

	public static Vector3 insideUnitCircle
	{
		get
		{
			Vector3 ans = UnityEngine.Random.insideUnitCircle;
			ans.z = ans.y;
			ans.y = 0f;
			return ans;
		}
	}

	public static float RangeRounded(float min, float max, float unit)
	{
		return Mathf.RoundToInt(UnityEngine.Random.Range(min, max) / unit) * unit;
	}
}