using System.Collections;
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
        if (!animationBusy && (Random.Range(0f, 1f) > 0.995f))
            StartCoroutine(PretendLife());
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

    public void MoveTo(Vector3 position)
    {
        StartCoroutine(MoveToAnim(position));
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
        float angle = Random.Range(-90f, 90f);
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
