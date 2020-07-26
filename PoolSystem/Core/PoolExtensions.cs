using System.Collections;
using UnityEngine;

public static class PoolExtensions
{
	public static GameObject Spawn(this GameObject prefabToSpawn, Vector3 pos, Quaternion rot = default)
	{
		return PoolsManager.Spawn(prefabToSpawn, pos, rot);
	}

	public static GameObject Spawn(this GameObject prefabToSpawn)
	{
		return PoolsManager.Spawn(prefabToSpawn, prefabToSpawn.transform.position, prefabToSpawn.transform.rotation);
	}

	public static void Despawn(this GameObject objToDespawn)
	{
		PoolsManager.Despawn(objToDespawn);
	}

	public static void Despawn(this Transform objToDespawn)
	{
		objToDespawn.gameObject.Despawn();
	}
}
