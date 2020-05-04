using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public float mouseWheelSensitivity = 1000f;
    public float zoomDistanceMin = 20f;
    public float zoomDistanceMax = 200f;

    private GameManager gm;
    private Vector3 groundpos;
    private float zoomDistance;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindObjectOfType<GameManager>();
        groundpos = new Vector3(0f, 0f, 0f);
        zoomDistance = 100f;

        // Init camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = groundpos;
            cam.transform.Translate(0f, 0f, -zoomDistance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Mouse movement pans the camera in XZ
        if (Input.GetMouseButton(1)) // right button
        {
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // groundpos.y always 0 (or maybe get the height of center tile later)
            groundpos.x = Mathf.Clamp(groundpos.x + mouseX, gm.SceneMinX, gm.SceneMaxX);
            groundpos.z = Mathf.Clamp(groundpos.z + mouseY, gm.SceneMinZ, gm.SceneMaxZ);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Apply the ground position
        cam.transform.position = groundpos;

        // Mouse wheel zoom in/out by changing camera position
        // using translate so that rotation is considered and the ground pos focused
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSensitivity * Time.deltaTime;
        zoomDistance = Mathf.Clamp(zoomDistance - mouseWheel, zoomDistanceMin, zoomDistanceMax);
        cam.transform.Translate(0f, 0f, -zoomDistance);

        // Left click with mouse selects a tile
        if (Input.GetMouseButtonDown(0)) // left button
        {
            // Cast a ray and let the GameManager decide what to do with that
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // To test the ray better change GetMouseButtonDown() to Input.GetMouseButton()
            // Debug.DrawRay(ray.origin, ray.direction * 300, Color.red);

            gm.Select(ray);
        }
    }
}
