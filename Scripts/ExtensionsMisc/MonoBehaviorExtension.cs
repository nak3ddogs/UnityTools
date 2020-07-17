using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoBehaviorExtension
{
	public static void Invoke(this MonoBehaviour mb, Action action, float delay, bool useTimeScale = true)
	{
		IEnumerator InvokeIE()
		{
			if (useTimeScale)
			{
				yield return new WaitForSeconds(delay);
			}
			else
			{
				yield return new WaitForSecondsRealtime(delay);
			}
			action.Invoke();
		}

		mb.StartCoroutine(InvokeIE());
	}

	public static void Log(this MonoBehaviour mb, string message)
	{
		Debug.Log($"[{mb.GetType().Name}] {message}", mb);
	}

	public static void LogWarning(this MonoBehaviour mb, string message)
	{
		Debug.LogWarning($"[{mb.GetType().Name}] {message}", mb);
	}

	public static void LogError(this MonoBehaviour mb, string message)
	{
		Debug.LogError($"[{mb.GetType().Name}] {message}", mb);
	}
}