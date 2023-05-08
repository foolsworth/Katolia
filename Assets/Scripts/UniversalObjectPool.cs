using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

public class UniversalObjectPool :  SerializedMonoBehaviour
{
    [SerializeField, ReadOnly] private Dictionary<Type, List<Object>> _PooledObjects = new Dictionary<Type, List<Object>>();
    public static UniversalObjectPool instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void ReturnToPool(Object pooledObject)
    {
        if (_PooledObjects.ContainsKey(pooledObject.GetType()))
        {
            _PooledObjects[pooledObject.GetType()].Add(pooledObject);
        }
    }

    public T GetObject<T>(T pooledObject, Vector3 position, Quaternion rotation) where T : MonoBehaviour
    {
        if (_PooledObjects.ContainsKey(typeof(T)))
        {
            if (_PooledObjects[typeof(T)].Count > 0)
            {
                var objectInPool =  (T) Convert.ChangeType(_PooledObjects[pooledObject.GetType()][0], typeof(T));
                var objectTransform = objectInPool.transform;
                objectTransform.position = position;
                objectTransform.rotation = rotation;
                _PooledObjects[typeof(T)].RemoveAt(0);
                return objectInPool;
            }

            var newObject = Instantiate(pooledObject, position, rotation);
            return (T) Convert.ChangeType(newObject, typeof(T));
        }

        _PooledObjects.Add(pooledObject.GetType(), new List<Object>());
        var newObject2 = Instantiate(pooledObject, position, rotation);
        return (T) Convert.ChangeType(newObject2, typeof(T));
    }
}