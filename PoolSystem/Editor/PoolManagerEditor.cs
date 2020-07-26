using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoolsManager))]
public class PoolManagerEditor : Editor
{
	private PoolsManager poolManager { get { return (PoolsManager)target; } }

	private GUIStyle background;
	private GUIStyle poolBackground;
	private GUIStyle dropBox;

	private string searchStr = "";

	private void OnEnable()
	{
		background = new GUIStyle();
		poolBackground = new GUIStyle();
		dropBox = new GUIStyle();

		background.normal.background = CustomEditorUtils.MakeTex(new Color(0.5f, 0.5f, 0.5f, 0.5f));
		poolBackground.normal.background = CustomEditorUtils.MakeTex(new Color(0.3f, 0.3f, 0.3f, 0.5f));
		dropBox.normal.background = CustomEditorUtils.MakeTex(new Color(1, 1, 1, 0.5f));

		poolBackground.margin = new RectOffset(2, 2, 2, 2);
		dropBox.margin = new RectOffset(4, 4, 4, 4);

		dropBox.alignment = TextAnchor.MiddleCenter;

		dropBox.fontSize = 14;

		dropBox.normal.textColor = Color.black;
	}

	public override void OnInspectorGUI()
	{
		Undo.RecordObject(poolManager, "poolmanager");
		Toolbar();
	}

	private void Toolbar()
	{
		GUILayout.Space(10f);
		DropArea();
		GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.ExpandWidth(true));

		string txt = "Pools (" + poolManager.Pools.Count + ")";

		if (Application.isPlaying)
		{
			int total = 0;
			int spawned = 0;
			poolManager.Pools.ForEach((x) =>
			{
				spawned += x.SpawnedCount;
				total += x.TotalCount;
			});

			txt += "     ( " + spawned + "/" + total + " )";
		}

		GUILayout.Label(txt);
		SearchField();

		if (GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.Width(65)))
			poolManager.Pools.ForEach(x => x.Foldout = true);

		if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.Width(71)))
			poolManager.Pools.ForEach(x => x.Foldout = false);

		GUILayout.EndHorizontal();
		GUILayout.Space(5);
		GUILayout.BeginVertical();

		var result = searchStr == "" ? poolManager.Pools : poolManager.Pools.Where(x => x.Prefab.name.Contains(searchStr)).ToList();

		GUILayout.EndVertical();
		for (int i = 0; i < result.Count; i++)
		{
			Pool pool = result[i];
			GUILayout.BeginVertical();
			PoolArea(pool);
			GUILayout.EndVertical();
		}
	}

	private void PoolArea(Pool pool)
	{
		GUILayout.BeginVertical(poolBackground);
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.Space(10f);

		pool.Foldout = EditorGUILayout.Foldout(pool.Foldout, pool.PoolName);
		GUILayout.FlexibleSpace();

		if (Application.isPlaying)
			GUILayout.Label("Spawned: " + pool.SpawnedCount + "/" + pool.TotalCount);

		if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(40)))
			pool.ClearAndDestroy();

		if (GUILayout.Button("Preinstantiate", EditorStyles.toolbarButton, GUILayout.Width(80)))
			pool.Initialize();

		if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(15)))
		{
			pool.ClearAndDestroy();
			if (pool.Root != null)
				GameObject.DestroyImmediate(pool.Root.gameObject);
			poolManager.Pools.Remove(pool);
		}

		GUILayout.EndHorizontal();

		if (pool.Foldout)
		{
			//TODO should fix the field. Later Aligator
			/*pool.Prefab*/ var Obj = EditorGUILayout.ObjectField("Prefab: ", pool.Prefab, typeof(GameObject), false) as GameObject;
			pool.Size = EditorGUILayout.IntField("Pool size: ", pool.Size);
			GUILayout.BeginHorizontal();
			pool.AllowGrowth = EditorGUILayout.Toggle("Allow grow: ", pool.AllowGrowth);
			pool.SpawnDespawnMessages = EditorGUILayout.Toggle("OnSpawn & OnDespawn messages: ", pool.SpawnDespawnMessages);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

	}

	private void DropArea()
	{
		if (Application.isPlaying)
			return;

		GUILayout.Box("Drop prefabs here", dropBox, GUILayout.ExpandWidth(true), GUILayout.Height(35));

		Event currentEvent = Event.current;
		EventType currentEventType = Event.current.type;
		bool isAccepted = false;

		if (currentEventType == EventType.DragExited)
			DragAndDrop.PrepareStartDrag();

		if (!GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			return;

		if (currentEventType == EventType.DragUpdated || currentEventType == EventType.DragPerform)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			if (currentEventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				isAccepted = true;
			}
			Event.current.Use();
		}

		if (isAccepted)
		{
			var pools = DragAndDrop.objectReferences
				.Where(x => x is GameObject)
				.Cast<GameObject>()
				.Where(x => PrefabUtility.GetPrefabAssetType(x) != PrefabAssetType.NotAPrefab)
				.Except(poolManager.Prefabs)
				.Select(x => new Pool(x));

			poolManager.Pools.AddRange(pools);
		}
	}

	private void SearchField()
	{
		searchStr = GUILayout.TextField(searchStr, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true), GUILayout.MinWidth(150));
		if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
		{
			searchStr = "";
			GUI.FocusControl(null);
		}
	}
}