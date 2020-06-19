public class Job
{
    public Worker worker; //The worker occupying this job
    private Building _building; //The building offering the job

    //Constructor. Call new Job(this) from the Building script to instanciate a job
    public Job(Building building)
    {
        this._building = building;
    }

    public Building GetBuilding()
    {
        return _building;
    }

    public void AssignWorker(Worker w)
    {
        worker = w;
        w.job = this;
        _building.WorkerAssignedToBuilding(w);
        w.MoveTo(_building.GetWorkerSpawnPosition());
    }

    public void RemoveWorker(Worker w)
    {
        worker = null;
        w.job = null;
        _building.WorkerRemovedFromBuilding(w);
        w.MoveTo(w.home.GetWorkerSpawnPosition());
    }
}
