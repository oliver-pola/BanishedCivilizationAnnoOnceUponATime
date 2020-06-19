using System.Collections;
using UnityEngine;

public class Worker : MonoBehaviour
{
    #region Manager References
    public JobManager jobManager; // Reference to the JobManager, needs to be set when instanciated
    public WorkerPool workerPool; // Reference to the WorkerPool, is set by WorkerPool on Instanciate
    #endregion

    #region job References
    public Building home; // Reference to housing building, is set by ?
    public Job job; // Reference to production building, is set by ?
    #endregion

    public int age; // The age of this worker
    public float happiness; // The happiness of this worker
    public float oneYearEvery = 15f; // in seconds
    private float oneYearProgress = 0f;
    private bool animationBusy = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        oneYearProgress += Time.deltaTime;
        if (oneYearProgress >= oneYearEvery)
        {
            oneYearProgress = 0f;
            Age();
        }

        if (!animationBusy && (Random.Range(0f, 1f) > 0.995f))
            StartCoroutine(PretendLife());
    }

    // TODO Whacky animation, fixed 10 FPS
    private IEnumerator PretendLife()
    {
        animationBusy = true;
        float angle = Random.Range(-90f, 90f);
        for (int i = 0; i < 10; i++)
        {
            transform.Rotate(0f, 0.1f * angle , 0f);
            yield return new WaitForSeconds(0.1f);
        }
        animationBusy = false;
    }

    private void Age()
    {
        //Implement a life cycle, where a Worker ages by 1 year every 15 real seconds.
        //When becoming of age, the worker enters the job market, and leaves it when retiring.
        //Eventually, the worker dies and leaves an empty space in his home. His Job occupation is also freed up.

        age++;
        if (age == 15)
        {
            BecomeOfAge();
        }
        else if (age == 65)
        {
            Retire();
        }
        else if (age == 100)
        {
            Die();
        }
    }


    public void BecomeOfAge()
    {
        StartCoroutine(GrowToAge());
        jobManager.RegisterWorker(this);
    }

    // TODO Whacky animation, fixed 10 FPS
    private IEnumerator GrowToAge()
    {
        animationBusy = true;
        for (int i = 1; i <= 10; i++)
        {
            transform.localScale = Vector3.one * (0.7f + 0.3f * i / 10);
            yield return new WaitForSeconds(0.1f);
        }
        animationBusy = false;
    }

    public void MoveTo(Vector3 position)
    {
        StartCoroutine(MoveToAnim(position));
    }

    // TODO Whacky animation, fixed 20 FPS
    private IEnumerator MoveToAnim(Vector3 position)
    {
        animationBusy = true;
        Vector3 source = transform.position;
        Vector3 direction = position - source;
        Vector3 lookDirektion = direction;
        lookDirektion.y = 0f;
        transform.rotation = Quaternion.LookRotation(lookDirektion);
        for (int i = 1; i <= 40; i++)
        {
            transform.position = source + direction * i / 40;
            yield return new WaitForSeconds(0.05f);
        }
        animationBusy = false;
    }

    private void Retire()
    {
        jobManager.RemoveWorker(this);
    }

    private void Die()
    {
        // just in case we die before retirement
        jobManager.RemoveWorker(this);

        if (home)
        {
            home.WorkerRemovedFromBuilding(this);
            home = null;
        }
        StartCoroutine(LieDownToDeath());
    }

    // TODO Whacky animation, fixed 10 FPS
    private IEnumerator LieDownToDeath()
    {
        animationBusy = true; 
        for (int i = 1; i <= 10; i++)
        {
            // bury 2 meters deep
            transform.Translate(Vector3.down * 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
        workerPool.Release(this);
        animationBusy = false;
    }

}
