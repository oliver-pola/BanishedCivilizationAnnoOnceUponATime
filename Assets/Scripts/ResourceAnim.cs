using UnityEngine;

public class ResourceAnim : MonoBehaviour
{
    public float rotationSpeed = 360;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
