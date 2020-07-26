using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
#if UNITY_EDITOR
	/// for custom inspector
	public bool Foldout;
#endif
	[SerializeField] [HideInInspector] private GameObject m_prefab = null;

	public GameObject Prefab
	{
		get
		{
			return m_prefab;
		}
	}

	public List<GameObject> Despawned = new List<GameObject>();
	public List<GameObject> Spawned = new List<GameObject>();

	public Transform Root = null;
	public bool AllowGrowth = true;
	public int Size = 0;
	public int SpawnedCount => Spawned.Count;
	public int TotalCount => Spawned.Count + Despawned.Count;
	public string PoolName => m_prefab == null ? "None" : m_prefab.name;
	public string RootName => $"{PoolName} [Pool]";
	public bool Empty => Despawned.Count == 0;
	public bool SpawnDespawnMessages = false;
	public string SpawnMessageMethodName = "OnSpawn";
	public string DeSpawnMessageMethodName = "OnDespawn";

	//Constructors
	private Pool() { }

	public Pool(GameObject _prefab)
	{
		if (!_prefab)
			Debug.LogError("Prefab is null");
		m_prefab = _prefab;
		Root = new GameObject(RootName).transform;
		Root.SetParent(PoolsManager.INSTANCE.transform);
	}

	public void Initialize()
	{
		if (m_prefab == null)
			return;
		if (Root == null)
		{

			Root = new GameObject(RootName).transform;
			Root.SetParent(PoolsManager.INSTANCE.transform);
		}

		for (int i = 0; i < Size && TotalCount < Size; i++)
		{
			AddNewObject();
		}
	}

	private GameObject AddNewObject()
	{
		GameObject go = GameObject.Instantiate(m_prefab, Root.position, Root.rotation) as GameObject;
#if UNITY_EDITOR
		if (!Application.isPlaying)
			UnityEditor.Undo.RegisterCreatedObjectUndo(go, "[PoolSystem]: instantiated_obj");
#endif
		Despawned.Add(go);
		go.transform.SetParent(Root);
		go.SetActive(false);
		return go;
	}

	public GameObject Spawn(Vector3 pos, Quaternion rot)
	{
		GameObject go = Pop();
		go.transform.position = pos;
		go.transform.rotation = rot;
		go.SetActive(true);

		if (SpawnDespawnMessages)
			go.SendMessage(SpawnMessageMethodName, SendMessageOptions.DontRequireReceiver);

		return go;
	}

	public void Despawn(GameObject target)
	{
		if (!Spawned.Contains(target))
		{
			Debug.LogError("Target is not spawned", target);
			return;
		}
		Push(target);
		if (SpawnDespawnMessages)
			target.SendMessage(DeSpawnMessageMethodName, SendMessageOptions.DontRequireReceiver);
	}

	private GameObject Pop()
	{
		GameObject go = null;
		while (Despawned.Count > 0 && go == null)
		{
			go = Despawned[0];
			Despawned.RemoveAt(0);
		};

		if (go == null)
		{
			if (AllowGrowth)
			{
				go = AddNewObject();
				Size++;
				Despawned.Remove(go);
			}
			else
			{
				return null;
			}
		}
		Spawned.Add(go);
		go.transform.SetParent(null);
		return go;
	}

	private void Push(GameObject obj)
	{
		if (Despawned.Contains(obj) || !Spawned.Contains(obj))
			return;
		Spawned.Remove(obj);
		if (!obj)
			return;
		Despawned.Add(obj);
		obj.SetActive(false);
		obj.transform.SetParent(Root);
	}

	public void ClearAndDestroy()
	{
		for (int i = 0; i < Despawned.Count; i++)
		{
			Object.DestroyImmediate(Despawned[i]);
		}
		Despawned.Clear();
	}

	public void DespawnAll()
	{
		for (int i = 0; i < Spawned.Count; i++)
		{
			Push(Spawned[i]);
		}
	}
}