using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class MapEditorWindow : EditorWindow
{
	private bool Enabled = false;
	private bool extraSettings;
	private Vector3 planeNormal = Vector3.up;
	private Vector3 planePoint = Vector3.zero;
	private bool snapping = true;
	private float snappingDist = 1;
	private bool canDestroyNonPrefabs = false;
	private Transform parent = null;
	private Vector3 lastPoint = Vector3.zero;
	private bool autoParentSelection = true;
	private int currentRotation = 0;
	private List<GameObject> lastCreatedInstances = new List<GameObject>();

	// Add menu named "My Window" to the Window menu
	[MenuItem("Tools/Map Editor Window")]
	static void Open()
	{
		// Get existing open window or if none, make a new one:
		MapEditorWindow window = (MapEditorWindow)EditorWindow.GetWindow(typeof(MapEditorWindow));
		window.Show();
	}

	void OnEnable()
	{
		SceneView.beforeSceneGui += OnScene;
	}

	void OnDisable()
	{
		SceneView.beforeSceneGui -= OnScene;
	}

	void OnScene(SceneView sceneView)
	{
		//Vector3 mousePosition = Event.current.mousePosition;
		//var camera = SceneView.currentDrawingSceneView.camera;
		//mousePosition.y = camera.pixelHeight - mousePosition.y; // Flip y
		//Ray ray = camera.ScreenPointToRay(mousePosition);
		//Debug.Log($"asd {mousePosition}");
		if (Event.current.type == EventType.KeyDown && Event.current.modifiers == EventModifiers.Control)
		{
			if (Event.current.keyCode == KeyCode.E)
			{
				currentRotation++;
				currentRotation %= 4;
				foreach (var i in lastCreatedInstances)
				{
					if (i != null)
					{
						i.transform.rotation = Quaternion.Euler(planeNormal * 90 * currentRotation);
						Undo.RecordObject(i.transform, "rotate");
					}
				}

				Event.current.Use();
			}
			if (Event.current.keyCode == KeyCode.Q)
			{
				currentRotation--;
				currentRotation %= 4;
				Event.current.Use();
				foreach (var i in lastCreatedInstances)
				{
					if (i != null)
					{
						i.transform.rotation = Quaternion.Euler(planeNormal * 90 * currentRotation);
						Undo.RecordObject(i.transform, "rotate");
					}
				}
			}
		}

		if (!Enabled || !(Event.current.button == 0 || Event.current.button == 1) || Event.current.modifiers != EventModifiers.Control)
			return;
		// convert GUI coordinates to screen coordinates
		Vector3 screenPosition = Event.current.mousePosition;
		screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
		Ray ray = Camera.current.ScreenPointToRay(screenPosition);
		Plane plane = new Plane(planeNormal, planePoint);
		if (!plane.Raycast(ray, out float dist))
		{
			Debug.LogWarning("Not pointed on the plane");
		}
		Vector3 point = ray.GetPoint(dist);
		if (snapping)
		{
			float s = 1f / snappingDist;
			point.x = Mathf.Round(point.x * s) / s;
			point.y = Mathf.Round(point.y * s) / s;
			point.z = Mathf.Round(point.z * s) / s;
		}
		Debug.DrawRay(point, Vector3.up, Color.red);
		bool newPos = (point - lastPoint).sqrMagnitude > 0.01f;
		bool mouseEvent = (Event.current.type == EventType.MouseDown || (newPos && Event.current.type == EventType.MouseDrag));
		if (!mouseEvent)
		{
			return;
		}

		if (Event.current.type == EventType.MouseDown)
		{
			lastCreatedInstances.Clear();
		}

		if (autoParentSelection && parent == null)
		{
			GameObject go = GameObject.Find("Map Root (Auto)");
			if (go == null)
			{
				go = new GameObject("Map Root (Auto)");
			}
			parent = go.transform;
		}

		Transform transformOnPoint = null;
		if (transformOnPoint == null && snapping)
		{
			var children = new List<Transform>();
			if (parent)
			{
				foreach (Transform o in parent)
				{
					children.Add(o);
				}
			}
			else
			{
				children.AddRange(SceneManager.GetActiveScene().GetRootGameObjects().Select(x => x.transform));
			}

			var closest = children.OrderBy(x => Vector3.SqrMagnitude(x.position - point)).FirstOrDefault();
			if (closest != null)
			{
				float closestDist = Vector3.Distance(closest.position, point);
				if (closestDist < snappingDist)
				{
					transformOnPoint = closest;
				}
			}
		}

		//destroy target
		if (transformOnPoint)
		{
			var prefabType = PrefabUtility.GetPrefabAssetType(transformOnPoint.gameObject);
			if (canDestroyNonPrefabs || prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Model)
			{
				Undo.DestroyObjectImmediate(transformOnPoint.gameObject);
				transformOnPoint = null;
			}
			else
			{
				Debug.LogWarning("Can't destroy non prefab");
			}
		}

		//instantiate stuff
		if (transformOnPoint == null && Selection.gameObjects.Length != 0 && Event.current.button == 0)
		{
			var prefab = Selection.gameObjects[Random.Range(0, Selection.gameObjects.Length)];
			GameObject go = null;
			var prefabType = PrefabUtility.GetPrefabAssetType(prefab);
			if (prefabType == PrefabAssetType.NotAPrefab)
			{
				go = Instantiate(prefab);
			}
			else
			{
				prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab);
				go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			}
			go.transform.SetParent(parent);
			go.transform.rotation = Quaternion.Euler(planeNormal * 90 * currentRotation);
			go.transform.position = point;
			go.name = prefab.name;
			lastCreatedInstances.Add(go);
			Undo.RegisterCreatedObjectUndo(go, "created by map editor");
		}

		lastPoint = point;
		Event.current.Use();
	}


	void OnGUI()
	{
		GUILayout.Space(20);
		if (Enabled)
		{
			if (GUILayout.Button("Enabled"))
			{
				Enabled = false;
				Repaint();
			}
		}
		else
		{
			if (GUILayout.Button("Disabled"))
			{
				Enabled = true;
				Repaint();
			}
		}
		GUILayout.Space(20);
		parent = EditorGUILayout.ObjectField("parent", parent, typeof(Transform), true) as Transform;
		if (EditorUtility.IsPersistent(parent))
		{
			parent = null;
			Debug.LogWarning("Parent only must be a scene object");
		}
		else if (autoParentSelection && Selection.gameObjects.Length > 0 && !EditorUtility.IsPersistent(Selection.gameObjects[0]) && Selection.gameObjects[0].transform.parent != null)
		{
			parent = Selection.gameObjects[0].transform.parent;
		}

		autoParentSelection = EditorGUILayout.ToggleLeft("auto parent selection", autoParentSelection);
		GUILayout.Space(20);
		GUILayout.Label("Current Selection:", EditorStyles.boldLabel);
		StringBuilder sb = new StringBuilder();
		foreach (var go in Selection.gameObjects)
		{
			sb.AppendLine(go.name);
		}
		EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);

		GUILayout.Space(20);
		snapping = EditorGUILayout.BeginToggleGroup("Snapping", snapping);
		if (snapping)
		{
			snappingDist = EditorGUILayout.FloatField("Snapping distance", snappingDist);
		}
		EditorGUILayout.EndToggleGroup();

		extraSettings = EditorGUILayout.BeginToggleGroup("Extra Settings", extraSettings);
		if (extraSettings)
		{
			planeNormal = EditorGUILayout.Vector3Field("plane normal", planeNormal);
			planePoint = EditorGUILayout.Vector3Field("plane point", planePoint);
			canDestroyNonPrefabs = EditorGUILayout.Toggle("can destroy non prefabs", canDestroyNonPrefabs);
			//myBool = EditorGUILayout.Toggle("Toggle", myBool);
			//myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
		}
		EditorGUILayout.EndToggleGroup();
		StringBuilder infoSb = new StringBuilder();
		infoSb.AppendLine("Ctrl + Mouse1 => Instantiate");
		if (snapping)
		{
			infoSb.AppendLine("Ctrl + Mouse2 => Destroy");
		}
		else if (!snapping)
		{
			infoSb.AppendLine("Destroy only works if snapping is enabled");
		}

		infoSb.AppendLine($"Current Rot: {currentRotation * 90}");
		infoSb.AppendLine($"Ctrl + Q => -90 | Ctrl + E => +90");
		EditorGUILayout.HelpBox(infoSb.ToString(), MessageType.Info);
	}

	public void OnInspectorUpdate()
	{
		// This will only get called 10 times per second.
		Repaint();
	}
}