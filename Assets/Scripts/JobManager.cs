using System;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    private List<Job> _availableJobs = new List<Job>();
    private List<Worker> _unoccupiedWorkers = new List<Worker>();

    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Don't call it that often, there is only need if job or worker is added
        // HandleUnoccupiedWorkers();
    }
    #endregion


    #region Methods
    private void HandleUnoccupiedWorkers()
    {
        if (_unoccupiedWorkers.Count > 0 && _availableJobs.Count > 0)
        {
            int match = Math.Min(_unoccupiedWorkers.Count, _availableJobs.Count);
            // assign workers to jobs
            for (int i = 0; i < match; i++)
            {
                Job j = _availableJobs[i];
                Worker w = _unoccupiedWorkers[i];
                j.AssignWorker(w);
            }
            // remove assigned items from lists
            _unoccupiedWorkers.RemoveRange(0, match);
            _availableJobs.RemoveRange(0, match);
        }
    }

    public void RegisterJob(Job job)
    {
        _availableJobs.Add(job);
        HandleUnoccupiedWorkers();
    }

    public void RemoveJob(Job job)
    {
        _availableJobs.Remove(job);
        if (job.worker != null)
        {
            _unoccupiedWorkers.Add(job.worker);
            job.RemoveWorker(job.worker);
            HandleUnoccupiedWorkers();
        }
    }

    public void RemoveJob(List<Job> jobs)
    {
        foreach (var job in jobs)
            RemoveJob(job);
    }

    public void RegisterWorker(Worker w)
    {
        _unoccupiedWorkers.Add(w);
        HandleUnoccupiedWorkers();
    }

    public void RemoveWorker(Worker w)
    {
        _unoccupiedWorkers.Remove(w);
        if (w.job != null)
        {
            _availableJobs.Add(w.job);
            w.job.RemoveWorker(w);
            HandleUnoccupiedWorkers();
        }
    }

    public void RemoveWorker(List<Worker> workers)
    {
        foreach (var worker in workers)
            RemoveWorker(worker);
    }

    #endregion
}
