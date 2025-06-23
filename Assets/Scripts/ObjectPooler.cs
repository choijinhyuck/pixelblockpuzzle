using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Singleton
    public static ObjectPooler Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    // 각 prefab에 대해 할당된 게임 오브젝트 큐를 관리합니다.
    public Dictionary<GameObject, Queue<GameObject>> poolDictionary;
    public Dictionary<int, GameObject> instanceToPoolMap;

    void Start()
    {
        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        instanceToPoolMap = new Dictionary<int, GameObject>();
    }

    // prefab이 없으면 새로 큐를 생성하고,
    // 필요 시 새 오브젝트를 생성하여 반환합니다.
    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> objectPool = poolDictionary[prefab];

        GameObject objectToSpawn;
        if (objectPool.Count == 0)
        {
            objectToSpawn = Instantiate(prefab);
            instanceToPoolMap.Add(objectToSpawn.GetInstanceID(), prefab);
            //Debug.Log($"New instance of {prefab.name} created");
        }
        else
        {
            objectToSpawn = objectPool.Dequeue();
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        var poolableComponents = objectToSpawn.GetComponents<IPoolable>();
        foreach (var component in poolableComponents)
        {
            component.OnObjectSpawn();
        }
        return objectToSpawn;
    }

    // 반환 시 prefab에 해당하는 큐에 다시 넣어 재사용 가능하도록 합니다.
    public void ReturnToPool(GameObject obj)
    {
        // obj가 이미 파괴되었으면 아무 것도 하지 않습니다.
        if (obj == null) return;

        int instanceID = obj.GetInstanceID();

        if (instanceToPoolMap.TryGetValue(instanceID, out GameObject prefab))
        {
            // prefab이 파괴된 경우 예외 처리
            if (prefab == null)
            {
                //Debug.LogWarning("Prefab reference is lost. Object can't be pooled: " + obj.name);
                return;
            }
            
            var poolableComponents = obj.GetComponents<IPoolable>();
            foreach (var component in poolableComponents)
            {
                component.OnObjectReturn();
            }

            obj.SetActive(false);
            poolDictionary[prefab].Enqueue(obj);
        }
        else
        {
            //Debug.LogWarning("Returning an object that wasn't pooled: " + obj.name);
            Destroy(obj);
        }
    }
}

public interface IPoolable
{
    void OnObjectSpawn();
    void OnObjectReturn();
}