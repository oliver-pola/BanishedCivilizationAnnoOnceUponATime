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
        w.WanderPosChanged();
        _building.WorkerAssignedToBuilding(w);
    }

    public void RemoveWorker(Worker w)
    {
        worker = null;
        w.job = null;
        w.WanderPosChanged();
        _building.WorkerRemovedFromBuilding(w);
    }
}
