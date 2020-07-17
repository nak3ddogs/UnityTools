using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-666)]
public class ServiceManager : MonoBehaviour
{
	public abstract class Service : ScriptableObject
	{
		public virtual void OnAwake() { }

		public virtual void OnStart() { }

		public virtual void OnUpdate(float deltaTime) { }

		public virtual void OnFixedUpdate(float deltaTime) { }

		public abstract void SetAsInstance();

		public abstract void RemoveAsInstance();
	}

	public abstract class Service<T> : Service where T : ScriptableObject
	{
		private static T _instance = null;
		public static T Instance => _instance;

		public override void SetAsInstance()
		{
			_instance = this as T;
		}

		public override void RemoveAsInstance()
		{
			if (_instance == this) _instance = null;
		}
	}

	public static ServiceManager Instance { get; private set; } = null;
	public static bool IsApplicationQuiting { get; private set; } = false;
	[SerializeField] private bool _replaceServicesWithCopies = true;
	[SerializeField] private List<Service> _services = new List<Service>();
	public System.Collections.ObjectModel.ReadOnlyCollection<Service> Services => _services.AsReadOnly();

	public static T Get<T>() where T : Service
	{
		for (int i = 0; i < Instance._services.Count; i++)
		{
			if (Instance._services[i] is T) return (T)Instance._services[i];
		}
		return null;
	}

	public static Service Get(string nameOfSO)
	{
		return Instance._services.FirstOrDefault(x => x.name == nameOfSO);
	}

	private void Awake()
	{
		Instance = this;
		if (_replaceServicesWithCopies) _services = _services.Select(x => Instantiate(x)).ToList();
		for (int i = 0; i < _services.Count; i++) _services[i].SetAsInstance();
		for (int i = 0; i < _services.Count; i++) _services[i].OnAwake();
		this.Log("[ServiceManager] services are awaked");
	}

	private void Start()
	{
		for (int i = 0; i < _services.Count; i++) _services[i].OnStart();
		this.Log("[ServiceManager] services are started");
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < _services.Count; i++) _services[i].OnUpdate(deltaTime);
	}

	private void LateUpdate()
	{
		float deltaTime = Time.fixedDeltaTime;
		for (int i = 0; i < _services.Count; i++) _services[i].OnFixedUpdate(deltaTime);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _services.Count; i++) _services[i].RemoveAsInstance();
	}

	private void OnApplicationQuit()
	{
		IsApplicationQuiting = true;
	}
}