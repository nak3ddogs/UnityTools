using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : Singleton<CoroutineHelper>
{
	protected override bool IsAutoCreateOnReference => true;

	public static new Coroutine StartCoroutine(IEnumerator enumerator)
	{
		return (INSTANCE as MonoBehaviour).StartCoroutine(enumerator);
	}
}