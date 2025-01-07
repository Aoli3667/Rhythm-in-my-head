using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System;

/// <summary>
/// a general object pool for spawning prefabs
/// </summary>
public class AObjectPool : MonoBehaviour, IObjectPool<GameObject>
{
    private ObjectPool<GameObject> objectPool;
    [SerializeField] bool collectionCheck = true;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxCapacity = 100;
    [SerializeField] GameObject poolObjPrefab;
    private List<GameObject> inPoolObj = new();
    private readonly Func<GameObject> _objectConst;
    private Action<GameObject> _enterHandle;
    private Action<GameObject> _leaveHandle;
    private int _count = 0;
    private Transform _parent;
    //public readonly Dictionary<Object, int> Counts = new();

    public AObjectPool(GameObject itemPrefab, Transform parent = null, Func<GameObject> objConstructor = null, int initPoolSize = 0, int maxPoolSize = 20, Action<GameObject> enterHandle = null,
            Action<GameObject> leaveHandle = null)
    {
        poolObjPrefab = itemPrefab;
        _parent = parent;
        _objectConst = objConstructor;
        _enterHandle = enterHandle;
        _leaveHandle = leaveHandle;
        defaultCapacity = initPoolSize;
        maxCapacity = maxPoolSize;
        _count = 0;

        InitPool();
    }

    public void InitPool()
    {
        objectPool = new ObjectPool<GameObject>(
            CreatePoolObj,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPool,
            collectionCheck,
            defaultCapacity,
            maxCapacity
        );
    }

    public void DebugPool()
    {
        Debug.Log(objectPool.CountInactive);
        Debug.Log(objectPool.CountAll);
        Debug.Log(objectPool.CountActive);

    }

    public GameObject CreatePoolObj()
    {
        var newObject = _objectConst != null ? _objectConst() : Instantiate(poolObjPrefab);
        _count++;
        if (newObject is GameObject obj)
        {
            if (_parent)
            {
                obj.transform.SetParent(_parent);
                obj.transform.localPosition = Vector3.zero;
            }
            obj.SetActive(false);
        }
        return newObject;
    }

    public void OnTakeFromPool(GameObject poolObj)
    {
        if (poolObj is GameObject obj)
        {
            obj.SetActive(true); // Activate object
            if (_parent)
            {
                obj.transform.SetParent(_parent);
                obj.transform.localPosition = Vector3.zero;
            }
        }
        if (inPoolObj.Contains(poolObj))
        {
            inPoolObj.Remove(poolObj); // Remove from pool list
        }
        else
        {
            Debug.LogWarning($"Object {poolObj.name} was not found in the pool!");
        }
        _leaveHandle?.Invoke(poolObj);
    }

    public void OnReturnedToPool(GameObject poolObj)
    {
        if (poolObj is GameObject obj)
        {
            obj.SetActive(false); // Deactivate object
        }
        if (!inPoolObj.Contains(poolObj))
        {
            inPoolObj.Add(poolObj); // Add to pool list
        }
        else
        {
            Debug.LogWarning($"Object {poolObj.name} is already in the pool!");
        }
        _enterHandle?.Invoke(poolObj);
    }


    public void OnDestroyPool(GameObject poolObj)
    {
        if (poolObj is GameObject obj) Destroy(obj.gameObject);
    }

    public GameObject Get()
    {
        return objectPool.Get();
    }


    public void ReturnToPool(GameObject poolObj)
    {
        if (objectPool == null)
        {
            Debug.LogError("Object pool is not initialized!");
            return;
        }

        objectPool.Release(poolObj);
    }

}
