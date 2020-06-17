using System.Collections;
using UnityEngine;

public class Worker : MonoBehaviour
{
    #region Manager References
    JobManager _jobManager; //Reference to the JobManager
    GameManager _gameManager;//Reference to the GameManager
    #endregion

    public float age; // The age of this worker
    public float happiness; // The happiness of this worker

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: activate when ready
        // Age();

        if (!pretendingLife)
            StartCoroutine(PretendLife());
    }

    private bool pretendingLife = false;
    private IEnumerator PretendLife()
    {
        pretendingLife = true;
        float angle = Random.Range(-90f, 90f);
        for (int i = 0; i < 10; i++)
        {
            transform.Rotate(0f, 0.1f * angle , 0f);
            yield return new WaitForSeconds(0.1f);
        }
        pretendingLife = false;
    }

    private void Age()
    {
        //TODO: Implement a life cycle, where a Worker ages by 1 year every 15 real seconds.
        //When becoming of age, the worker enters the job market, and leaves it when retiring.
        //Eventually, the worker dies and leaves an empty space in his home. His Job occupation is also freed up.

        if (age > 14)
        {
            BecomeOfAge();
        }

        if (age > 64)
        {
            Retire();
        }

        if (age > 100)
        {
            Die();
        }
    }


    public void BecomeOfAge()
    {
        _jobManager.RegisterWorker(this);
    }

    private void Retire()
    {
        _jobManager.RemoveWorker(this);
    }

    private void Die()
    {
        Destroy(this.gameObject, 1f);
    }
}
