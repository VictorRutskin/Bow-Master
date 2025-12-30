using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple service locator pattern for dependency injection.
/// Provides centralized access to game services.
/// </summary>
public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    private Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ServiceLocator");
                _instance = go.AddComponent<ServiceLocator>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>
    /// Register a service instance.
    /// </summary>
    public void Register<T>(T service) where T : class
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Overwriting.");
        }
        _services[type] = service;
    }

    /// <summary>
    /// Get a service instance.
    /// </summary>
    public T Get<T>() where T : class
    {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object service))
        {
            return service as T;
        }
        // Don't log error if this is called during initialization (before GameManager.Start)
        // Components will retry in their Start() methods
        return null;
    }

    /// <summary>
    /// Check if a service is registered.
    /// </summary>
    public bool Has<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Unregister a service.
    /// </summary>
    public void Unregister<T>() where T : class
    {
        _services.Remove(typeof(T));
    }

    /// <summary>
    /// Clear all services.
    /// </summary>
    public void Clear()
    {
        _services.Clear();
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}

