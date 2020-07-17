using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected virtual bool IsPersistentSingleton => false;
	protected virtual bool IsAutoCreateOnReference => false;
	private static bool IsApplicationQuitting = false;
	public static bool IsCreated { get { return _instance; } }
	private static string PreferedName => $"[{typeof(T).Name}]";
	public static bool IsInstantiated => _instance;

	public static T INSTANCE
	{
		get
		{
			if (_instance == null && IsApplicationQuitting)
			{
				return null;
			}
			if (!_instance)
			{
				_instance = FindObjectOfType<T>();
			}
			if (!_instance)
			{
				var tempInstance = System.Activator.CreateInstance<T>();
				if ((tempInstance as Singleton<T>).IsAutoCreateOnReference && !IsApplicationQuitting)
				{
					Create();
					Debug.Log($"{PreferedName} Is auto created");
				}
				else
				{
					Debug.LogWarning($"{PreferedName} Auto creation is disabled");
				}
#pragma warning disable
#pragma warning enable
				GameObject.DestroyImmediate(tempInstance);
			}
			return _instance;
		}
	}

	public static T Create()
	{
		if (IsApplicationQuitting)
		{
			Debug.LogWarning($"{PreferedName} Application is quitting");
			return null;
		}

		if (_instance)
		{
			Debug.LogError($"{PreferedName} An instance is already exist");
			return _instance;
		}

		GameObject go = new GameObject(PreferedName);
		_instance = go.AddComponent<T>();

		return _instance;
	}

	private static T _instance;

	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
			gameObject.name = (PreferedName);
			if ((_instance as Singleton<T>).IsPersistentSingleton)
				DontDestroyOnLoad(_instance.gameObject);
			OnInitialization();
		}
		else if (_instance != this)
		{
			Destroy(this);
		}
	}

	protected virtual void Reset()
	{
		gameObject.name = (PreferedName);
	}

	protected virtual void OnInitialization()
	{
	}

	public static void SelfDestroy()
	{
		if (!_instance)
		{
			Debug.LogError($"{PreferedName} There is no instance");
			return;
		}
		(_instance as Singleton<T>).SelfDestroyInternal();
		Destroy(_instance.gameObject);
		_instance = null;
	}

	protected virtual void SelfDestroyInternal()
	{
	}

	private void OnApplicationQuit()
	{
		IsApplicationQuitting = true;
	}
}