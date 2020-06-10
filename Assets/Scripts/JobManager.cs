using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{

    private List<Job> _availableJobs = new List<Job>();
    public List<Worker> unoccupiedWorkers = new List<Worker>();

    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleUnoccupiedWorkers();
    }
    #endregion


    #region Methods

    private void HandleUnoccupiedWorkers()
    {
        if (unoccupiedWorkers.Count > 0)
        {

            //TODO: What should be done with unoccupied workers?

        }
    }

    public void RegisterWorker(Worker w)
    {
        unoccupiedWorkers.Add(w);
    }



    public void RemoveWorker(Worker w)
    {
        unoccupiedWorkers.Remove(w);
    }

    #endregion
}
