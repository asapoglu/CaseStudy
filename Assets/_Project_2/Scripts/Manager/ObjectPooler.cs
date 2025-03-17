using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abdurrahman.Project_2.Core
{
    [System.Serializable]
    public class ObjectPoolItem
    {
        public GameObject objectToPool;
        public int amountToPool;
        public bool shouldExpand = true;
        public ObjectPoolItem(GameObject obj, int amt, bool exp = true)
        {
            objectToPool = obj;
            amountToPool = Mathf.Max(amt, 2);
            shouldExpand = exp;
        }
    }
}
namespace Abdurrahman.Project_2.Core
{
    [DefaultExecutionOrder(-100)]
    public class ObjectPooler : MonoBehaviour
    {
        public List<ObjectPoolItem> itemsToPool;
        public Transform spawnTransform;
        public List<List<GameObject>> pooledObjectsList;
        public List<GameObject> pooledObjects;
        private List<int> positions;
        void Awake()
        {
            pooledObjectsList = new List<List<GameObject>>();
            pooledObjects = new List<GameObject>();
            positions = new List<int>();
            for (int i = 0; i < itemsToPool.Count; i++)
            {
                ObjectPoolItemToPooledObject(i);
            }
        }
        private void Start()
        {

        }
        public GameObject GetPooledObject(int index)
        {
            int curSize = pooledObjectsList[index].Count;
            for (int i = positions[index] + 1; i < positions[index] + pooledObjectsList[index].Count; i++)
            {
                if (!pooledObjectsList[index][i % curSize].activeInHierarchy)
                {
                    positions[index] = i % curSize;
                    return pooledObjectsList[index][i % curSize];
                }
            }
            if (itemsToPool[index].shouldExpand)
            {
                GameObject obj = (GameObject)Instantiate(itemsToPool[index].objectToPool);
                obj.SetActive(false);
                obj.transform.parent = this.transform;
                obj.transform.position = spawnTransform.position;
                obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                pooledObjectsList[index].Add(obj);
                return obj;
            }
            return null;
        }
        public List<GameObject> GetAllPooledObjects(int index)
        {
            return pooledObjectsList[index];
        }
        public int AddObject(GameObject GO, int amt = 3, bool exp = true)
        {
            ObjectPoolItem item = new ObjectPoolItem(GO, amt, exp);
            int currLen = itemsToPool.Count;
            itemsToPool.Add(item);
            ObjectPoolItemToPooledObject(currLen);
            return currLen;
        }
        void ObjectPoolItemToPooledObject(int index)
        {
            ObjectPoolItem item = itemsToPool[index];
            pooledObjects = new List<GameObject>();
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = Instantiate(item.objectToPool);
                obj.SetActive(false);
                obj.transform.parent = this.transform;
                obj.transform.position = spawnTransform.position;
                obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                pooledObjects.Add(obj);
            }
            pooledObjectsList.Add(pooledObjects);
            positions.Add(0);
        }

    }
}