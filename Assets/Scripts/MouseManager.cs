using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickEventArgs : EventArgs
{
    public Ray Ray { get; private set; }
    public MouseClickEventArgs(Ray ray)
    {
        Ray = ray;
    }
}

public delegate void MouseClickEventHandler(GameObject source, MouseClickEventArgs e);

public class MouseManager : MonoBehaviour
{
    // Exposed to Unity Editor
    public float mouseSensitivity = 200f;
    public float mouseWheelSensitivity = 1000f;
    public bool invertMouse = false;
    public float zoomDistanceMin = 20f;
    public float zoomDistanceMax = 200f;
    public bool invertWheel = false;
    public float cameraAngle = 45f;

    // Public for other code
    public float SceneMaxX { get; set; }
    public float SceneMinX { get; set; }
    public float SceneMaxZ { get; set; }
    public float SceneMinZ { get; set; }

    // Events
    public event MouseClickEventHandler OnMouseClick;

    // Private fields
    private Vector3 groundpos = new Vector3(0f, 0f, 0f);
    private float zoomDistance = 100f;

    // Start is called before the first frame update
    void Start()
    {
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

        // Addon feature: change camera angle to top-down by holding space
        Vector3 rot = new Vector3(Input.GetKey(KeyCode.Space) ? 90f : cameraAngle, 0f, 0f);
        cam.transform.eulerAngles = rot;

        // Mouse movement pans the camera in XZ
        if (Input.GetMouseButton(1)) // right button
        {
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            if (invertMouse)
            {
                mouseX = -mouseX;
                mouseY = -mouseY;
            }

            // groundpos.y always 0 (or maybe get the height of center tile later)
            groundpos.x = Mathf.Clamp(groundpos.x + mouseX, SceneMinX, SceneMaxX);
            groundpos.z = Mathf.Clamp(groundpos.z + mouseY, SceneMinZ, SceneMaxZ);
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
        if (invertWheel)
        {
            mouseWheel = -mouseWheel;
        }
        zoomDistance = Mathf.Clamp(zoomDistance - mouseWheel, zoomDistanceMin, zoomDistanceMax);
        cam.transform.Translate(0f, 0f, -zoomDistance);

        // Left click with mouse selects a tile
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1)) // left button
        {
            // Cast a ray and let the GameManager decide what to do with that
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // To test the ray better change GetMouseButtonDown() to Input.GetMouseButton()
            // Debug.DrawRay(ray.origin, ray.direction * 300, Color.red);

            OnMouseClick?.Invoke(this.gameObject, new MouseClickEventArgs(ray));
        }
    }

    public void SetCameraGroundPosition(float x, float z)
    {
        groundpos.x = x;
        groundpos.z = z;
    }
}
