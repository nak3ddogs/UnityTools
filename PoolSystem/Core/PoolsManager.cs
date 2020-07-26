using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolsManager : MonoBehaviour
{
#if UNITY_EDITOR
	private void OnValidate()
	{
		if (gameObject.name != "[PoolsManager]")
			gameObject.name = "[PoolsManager]";
	}
#endif

	private static PoolsManager m_Instance = null;
	public static PoolsManager INSTANCE
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = FindObjectOfType<PoolsManager>();
			}
			if (m_Instance == null)
			{
				var go = new GameObject("[PoolsManager]");
				m_Instance = go.AddComponent<PoolsManager>();
			}
			return m_Instance;
		}
	}

	public List<Pool> Pools = new List<Pool>();

	public Pool this[string name]
	{
		get
		{
			for (int i = 0; i < Pools.Count; i++)
			{
				if (Pools[i].PoolName == name)
				{
					return Pools[i];
				}
			}
			return null;
		}
	}

	public Pool this[GameObject prefab]
	{
		get
		{
			for (int i = 0; i < Pools.Count; i++)
			{
				if (Pools[i].Prefab == prefab)
				{
					return Pools[i];
				}
			}
			return null;
		}
	}

	public List<GameObject> Prefabs
	{
		get
		{
			return Pools.Select(pool => pool.Prefab).ToList();
		}
	}

	private void Awake()
	{
		if (m_Instance != null)
		{
			Debug.LogError("There are more poolsmanagers");
			return;
		}
		m_Instance = this;
		gameObject.name = "[PoolsManager]";
		for (int i = 0; i < Pools.Count; i++)
		{
			Pools[i].Initialize();
		}
	}

	public static Pool CreatePool(GameObject prefab)
	{
		Pool pool = new Pool(prefab);
		pool.Initialize();
		RegisterPool(pool);
		return pool;
	}

	//spawning
	public static GameObject Spawn(string name, Vector3 pos, Quaternion rot)
	{
		Pool targetPool = INSTANCE[name];
		return Spawn(targetPool, pos, rot);
	}

	public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		Pool targetPool = INSTANCE[prefab];

		if (targetPool == null)
		{
			targetPool = CreatePool(prefab);
		}

		return Spawn(targetPool, pos, rot);
	}

	public static GameObject Spawn(Pool targetPool, Vector3 pos, Quaternion rot)
	{
		return targetPool.Spawn(pos, rot);
	}

	public static void Despawn(GameObject target)
	{
		//Pool targetPool = PoolsManager.INSTANCE.Pools.FirstOrDefault(pool => pool.Spawned.Contains(target))
		Pool targetPool = null;

		for (int i = 0; i < PoolsManager.INSTANCE.Pools.Count && targetPool == null; i++)
		{
			for (int y = 0; y < PoolsManager.INSTANCE.Pools[i].Spawned.Count; y++)
			{
				if (PoolsManager.INSTANCE.Pools[i].Spawned[y] == target)
				{
					targetPool = PoolsManager.INSTANCE.Pools[i];
					break;
				}
			}
		}

		if (targetPool == null)
		{
			Debug.LogWarning("[PoolsManager]: Despawn: targetPool is null \n TARGET IS DESTROYED");
			Destroy(target);
			return;
		}

		targetPool.Despawn(target);
	}

	public static void DespawnAll()
	{
		for (int i = 0; i < PoolsManager.INSTANCE.Pools.Count; i++)
			PoolsManager.INSTANCE.Pools[i].DespawnAll();
	}

	static Pool RegisterPool(Pool target)
	{
		if (!INSTANCE.Pools.Contains(target))
			INSTANCE.Pools.Add(target);
		return target;
	}

	public static void RemovePool(string name)
	{
		Pool pool = INSTANCE[name];
		if (pool != null)
			INSTANCE.Pools.Remove(pool);
	}
}
