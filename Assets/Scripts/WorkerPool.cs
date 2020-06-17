using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerPool : MonoBehaviour
{
    public GameObject[] prefabs;
    private readonly List<GameObject> pool = new List<GameObject>();
    private readonly List<GameObject> free = new List<GameObject>();
    // Some deterministic randomness of worker prefabs
    System.Random rand = new System.Random(0);

    public Worker Require(Vector3 position, Quaternion rotation)
    {
        GameObject obj;
        Worker worker;
        if (free.Count > 0)
        {
            // get a free one from the pool
            obj = free[0];
            free.RemoveAt(0);

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(true);

            worker = obj.GetComponent<Worker>();
            return worker;
        }
        else
        {
            // create a new one in the pool
            GameObject prefab = prefabs[rand.Next(prefabs.Length)];
            // parent of instanciated objects is the WorkerPool object
            obj = Instantiate(prefab, position, rotation, transform);
            worker = obj.GetComponent<Worker>();
            if (worker == null)
                throw new Exception("WorkerPool prefabs must have Worker component");
            worker.workerPool = this;
            pool.Add(obj);
            return worker;
        }
    }

    public void Release(GameObject workerObject)
    {
        workerObject.SetActive(false);
        free.Add(workerObject);
    }

    public void Release(Worker worker)
    {
        Release(worker.gameObject);
    }
}
