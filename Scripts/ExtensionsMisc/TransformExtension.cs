using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformExtension
{
	public static Ray GetForwardRay(this Transform _tr)
	{
		return new UnityEngine.Ray(_tr.position, _tr.forward);
	}
}