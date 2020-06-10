using UnityEngine;

public class ResourceAnim : MonoBehaviour
{
    public float rotationSpeed = 360;
    public ParticleSystem particles;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        if (particles)
            particles.Play();
    }

    private void OnDisable()
    {
        if (particles)
            particles.Stop();
    }
}
