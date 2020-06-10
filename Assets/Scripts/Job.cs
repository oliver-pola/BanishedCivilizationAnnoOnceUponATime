﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job
{
    public Worker worker; //The worker occupying this job
    public ProductionBuilding building; //The building offering the job

    //Constructor. Call new Job(this) from the Building script to instanciate a job
    public Job(ProductionBuilding building)
    {
        this.building = building;
    }

    public void AssignWorker(Worker w)
    {
        worker = w;
        building.WorkerAssignedToBuilding(w);
    }

    public void RemoveWorker(Worker w)
    {
        worker = null;
        building.WorkerRemovedFromBuilding(w);
    }
}