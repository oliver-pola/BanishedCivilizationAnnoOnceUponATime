using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Analytics;

public class Worker : MonoBehaviour
{
    #region Manager References
    public JobManager jobManager; // Reference to the JobManager, needs to be set when instanciated
    public WorkerPool workerPool; // Reference to the WorkerPool, is set by WorkerPool on Instanciate
    #endregion

    #region job References
    public Building home; // Reference to housing building, is set by HousingBuilding
    public Job job; // Reference to production building, is set by Job on assignment
    #endregion

    public int age; // The age of this worker
    public float happiness; // The happiness of this worker
    public float oneYearEvery = 15f; // in seconds
    private float oneYearProgress = 0f;
    public int ageToBecomeOfAge = 15;
    public int ageToRetire = 65;
    public int ageToDie = 100;
    public Gender gender;
    public string givenName;
    private bool animationBusy = false;
    public Vector2Int currentTilePosition;
    public float speed = 5f;
    public float workProgress = 0f;
    public float workTime = 5f;
    public float relaxProgress = 0f;
    public float relaxTime = 5f;
    private WorkerState state = WorkerState.Relax;
    private Vector3 wanderPosWork;
    private Vector3 wanderPosHome;
    public Animator animator;
    private bool wanderBorderPassed;

    public enum WorkerState { CommuteToWork, Work, CommuteToHome, Relax};

    public enum Gender { Female, Male };

    #region Goods consumption and tax income
    public float economyInterval = 30f; // Seconds for goods consumption and tax payment
    private float economyProgress = 0f; // Seconds spent waiting so far
    // This topic needs a lot of balancing. Consumed goods are produced very rarely and need a lot of workers.
    // We can't assign 1 unit to 1 worker per whatever seconds. We'll have to use fractions.
    // On 100% efficiency we'll get:
    // + 1 fish every 30 seconds, needs 25 workers
    // + 1 cloth every 30 seconds, needs 50 (knitters) + 10 (sheep farm) workers
    // + 1 schnapps every 30 seconds, needs 50 (distillery) + 20 (potato farm) workers
    // = every 30 seconds 155 workers can share 1 of each.
    // If we use 0.0064 of each every 30 seconds, we can afford 156.25 workers, 
    // = 1.25 worker per consumable-group to do some wood work
    // = 12 consumable-groups for 1 wood-group (15 workers).
    // Sounds hard enough to achieve, so that we'll have to deal with less efficiency until we've built that much.
    // In our 32x32 map, there are 22 effcient fishing spots, and I managed to have 40 efficient farms.
    // Finally on average half of the people are workers, the other half are children and retired,
    // so each one is only allowed to cunsume half the amount calculated for a worker.
    // If all people eat 0.0032 of each every 30 seconds, we can afford 156.25 workers.
    // --------------------------------------------------------------------------------------------------------------
    // TODO: Later an extra challenge for the player would be to figure out the best consumables building ratio, 
    // that is not 1 of each. That would relate to different consumption values.
    public float consumeFish = 0.0032f;
    public float consumeClothes = 0.0032f;
    public float consumeSchnapps = 0.0032f;
    // The resoning for tax can't be derived from a fully built infrastructure. 
    // It is more important that we are able to run the early game on it, but it shouldn't bee too easy.
    public float tax = 2f; 
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        GenerateName();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWorkerState();
    }

    private void UpdateWorkerState()
    {
        // check if despawned, then ignore
        if (home == null)
            return;

        // update state machine
        if (state == WorkerState.Relax)
        {
            relaxProgress += Time.deltaTime;

            if (relaxProgress > relaxTime)
            {
                relaxProgress = 0f;
                state = WorkerState.CommuteToWork;
            }
            else
            {
                // busy animation could be dying or growing up, looks awkward while moving
                if (!animationBusy && MoveTo(wanderPosHome))
                    if (UnityEngine.Random.Range(0f, 1f) > 0.995f)
                        // relaxing should be a bit more calm than walking all the time
                        // so don't pick a new position every time
                        wanderPosHome = home.GetWorkerSpawnPosition();
            }
        }
        else if (state == WorkerState.Work)
        {
            workProgress += Time.deltaTime;
            if (workProgress > workTime || job == null) // may have lost job / retired
            {
                workProgress = 0f;
                state = WorkerState.CommuteToHome;
            }
            else
            {
                // currently they can't die or grow up during work, but keep in consistent with relaxing
                if (!animationBusy && MoveTo(wanderPosWork))
                    wanderPosWork = job.GetBuilding().GetWorkerSpawnPosition();
            }
        }

        if (state == WorkerState.CommuteToWork)
        {
            if (job == null) // may have lost job / retired
            {
                state = WorkerState.CommuteToHome;
            }
            // check if goal is reached
            else if (job.GetBuilding().vectorField[currentTilePosition.x, currentTilePosition.y] == currentTilePosition)
            {
                state = WorkerState.Work;
            }
            else
            {
                Vector2Int nextPos = job.GetBuilding().vectorField[currentTilePosition.x, currentTilePosition.y];
                if (!animationBusy && MoveTo(GetCoordiantesForTile(nextPos)))
                {
                    currentTilePosition = nextPos;
                }
            }
        }
        else if (state == WorkerState.CommuteToHome)
        {
            // check if goal is reached
            if (home.vectorField[currentTilePosition.x, currentTilePosition.y] == currentTilePosition)
            {
                state = WorkerState.Relax;
            }
            else
            {
                Vector2Int nextPos = home.vectorField[currentTilePosition.x, currentTilePosition.y];
                if (!animationBusy && MoveTo(GetCoordiantesForTile(nextPos)))
                {
                    currentTilePosition = nextPos;
                }
            }
        }
    }

    public void WanderPosChanged()
    {
        if (home != null) // may be despawned?
            wanderPosHome = home.GetWorkerSpawnPosition();
        if (job != null)
            wanderPosWork = job.GetBuilding().GetWorkerSpawnPosition();
    }

    public Vector3 GetCoordiantesForTile(Vector2Int tile)
    {
        // TODO: better dependency injection here
        GameManager gm = FindObjectOfType<GameManager>();

        Tile t = gm.GetTileFromMapCoords(tile.x, tile.y);
        if (t == null)
            return transform.position; // that's me, stand still
        return t.transform.position;
    }

    private bool MoveTo(Vector3 nextTilePos)
    {
        // look only horizintal, not up or down
        Vector3 lookPos = nextTilePos;
        lookPos.y = transform.position.y; // same height as me
        transform.LookAt(lookPos);

        // jump at tile border, constants very fine tuned to current settings, should better depend on those
        float borderMargin = 0.6f;
        if (transform.position.y > nextTilePos.y)
            borderMargin = -0.1f;
        if (Vector3.Distance(transform.position, lookPos) > 5 + borderMargin)
        {
            transform.position = Vector3.MoveTowards(transform.position, lookPos, speed * Time.deltaTime);
            if (animator != null)
                animator.SetBool("isWalking", true);
            wanderBorderPassed = false;
            return false;
        }
        else if (!wanderBorderPassed)
        {
            Vector3 jumpPos = transform.position;
            jumpPos.y = nextTilePos.y;
            if (Mathf.Abs(transform.position.y - jumpPos.y) > 1f && animator != null)
                animator.SetTrigger("isJumping");
            transform.position = jumpPos;
            wanderBorderPassed = true;
        }

        // check if goal is reached
        if (Vector3.Distance(transform.position, nextTilePos) > 0.01)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextTilePos, speed * Time.deltaTime);
            if (animator != null)
                animator.SetBool("isWalking", true);
            return false;
        }
        else
        {
            if (animator != null)
                animator.SetBool("isWalking", false);
            return true;
        }
    }

    // Simulate economy, is called every second by HousingBuilding.EconomyCycle()
    public void EconomyCycle(Warehouse warehouse)
    {
        oneYearProgress += 1f; // advance one cylce = 1 second
        if (oneYearProgress >= oneYearEvery)
        {
            oneYearProgress = 0f;
            Age();
        }

        economyProgress += 1f; // advance one cylce = 1 second
        if (economyProgress >= economyInterval)
        {
            economyProgress = 0f;
            Consume(warehouse);
            PayTax(warehouse);
        }
    }

    // Needs to be called by Building within the EconomyCycle chain, where we have access to the warehouse
    private void Consume(Warehouse warehouse)
    {
        int happinessCriteria = 0;
        if (warehouse.TryRemoveResource(Warehouse.ResourceTypes.Fish, consumeFish))
            happinessCriteria++;
        if (warehouse.TryRemoveResource(Warehouse.ResourceTypes.Clothes, consumeClothes))
            happinessCriteria++;
        if (warehouse.TryRemoveResource(Warehouse.ResourceTypes.Schnapps, consumeSchnapps))
            happinessCriteria++;
        // children and retired have a higher ground level of happiness, they don't matter in production anyway, only for spawn rate
        if (age < ageToBecomeOfAge || age >= ageToRetire || job != null)
            happinessCriteria++;
        happiness = happinessCriteria / 4f;
    }

    private void PayTax(Warehouse warehouse)
    {
        if (job != null)
            warehouse.AddResource(Warehouse.ResourceTypes.Money, happiness * tax);
    }

    private void Age()
    {
        //Implement a life cycle, where a Worker ages by 1 year every 15 real seconds.
        //When becoming of age, the worker enters the job market, and leaves it when retiring.
        //Eventually, the worker dies and leaves an empty space in his home. His Job occupation is also freed up.

        age++;
        if (age == ageToBecomeOfAge)
        {
            BecomeOfAge();
        }
        else if (age == ageToRetire)
        {
            Retire();
        }
        else if (age == ageToDie)
        {
            Die();
        }
    }

    public void BecomeOfAge()
    {
        StartCoroutine(GrowToAge());
        // coroutine at the end calls: jobManager.RegisterWorker(this);
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
        // coroutine at the end calls: workerPool.Release(this);
    }

    #region Animation mockups
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

    // TODO Whacky animation, fixed 10 FPS
    private IEnumerator PretendLife()
    {
        animationBusy = true;
        float angle = UnityEngine.Random.Range(-90f, 90f);
        for (int i = 0; i < 10; i++)
        {
            transform.Rotate(0f, 0.1f * angle, 0f);
            yield return new WaitForSeconds(0.1f);
        }
        animationBusy = false;
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
        jobManager.RegisterWorker(this);
    }

    #endregion

    #region Name generation
    static readonly System.Random rand = new System.Random(0);
    private static readonly string[] namePrefix = { "Er", "Dor", "Fran", "Jan", "Ol", "Gun", "Tar", "Han", "Gus", "Laus", "Kar", "Ren", "Ran", "Bran", "Thor", "Flor", "Hor", "Wer" };
    private static readonly string[] femaleSuffix = { "a", "ika", "is", "ka", "ie" };
    private static readonly string[] maleSuffix = { "ik", "k", "af", "olf", "ek" };
    private void GenerateName()
    {
        if (gender == Gender.Female)
            givenName = namePrefix[rand.Next(namePrefix.Length)] + femaleSuffix[rand.Next(femaleSuffix.Length)];
        else if (gender == Gender.Male)
            givenName = namePrefix[rand.Next(namePrefix.Length)] + maleSuffix[rand.Next(maleSuffix.Length)];
    }
    #endregion
}
