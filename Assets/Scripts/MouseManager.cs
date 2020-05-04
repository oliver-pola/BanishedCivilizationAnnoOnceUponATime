using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1)) // right button
        {
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            if (Camera.main != null)
            {
                Camera cam = Camera.main;
                Vector3 pos = cam.transform.position;
                pos.x = Mathf.Clamp(pos.x + mouseX, gm.SceneMinX, gm.SceneMaxX);
                pos.z = Mathf.Clamp(pos.z + mouseY, gm.SceneMinZ, gm.SceneMaxZ);
                cam.transform.position = pos;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
