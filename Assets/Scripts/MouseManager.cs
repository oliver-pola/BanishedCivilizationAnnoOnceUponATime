using System;
using UnityEngine;
using UnityEngineInternal;

public class CameraRayEventArgs : EventArgs
{
    public Ray Ray { get; private set; }
    public CameraRayEventArgs(Ray ray)
    {
        Ray = ray;
    }
}

public delegate void MouseClickEventHandler(GameObject source, CameraRayEventArgs e);
public delegate void MouseOverEventHandler(GameObject source, CameraRayEventArgs e);

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
    public float keyPanSpeed = 100f;

    // Public for other code
    public float SceneMaxX { get; set; }
    public float SceneMinX { get; set; }
    public float SceneMaxZ { get; set; }
    public float SceneMinZ { get; set; }

    // Events
    public event MouseClickEventHandler OnMouseClick;
    public event MouseOverEventHandler OnMouseOver;

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
        Vector3 rot = new Vector3(Input.GetKey(KeyCode.Space) ? 90f : cameraAngle, cam.transform.eulerAngles.y, 0f);
        cam.transform.eulerAngles = rot;

        // Mouse movement or cursor keys pan the camera in XZ
        float panX = 0f;
        float panZ = 0f;
        bool panMode = false;
        if (Input.GetMouseButton(1) && !Input.GetMouseButton(0)) // right button
        {
            panMode = true;
            Cursor.lockState = CursorLockMode.Locked;

            panX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            panZ = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            if (invertMouse)
            {
                panX = -panX;
                panZ = -panZ;
            }
        }
        else
        {
            panX = Input.GetAxis("Horizontal") * keyPanSpeed * Time.deltaTime;
            panZ = Input.GetAxis("Vertical") * keyPanSpeed * Time.deltaTime;
            panMode = panX != 0f || panZ != 0f;
        }
        if (panMode)
        {
            // apply relative to camera left/right but absolute forward/back, assume cam is not rolled
            Vector3 right = cam.transform.right;
            Vector3 forward = new Vector3(-right.z, 0f, right.x);
            groundpos += right * panX + forward * panZ;

            // groundpos.y always 0 (or maybe get the height of center tile later), x, z inside map border
            groundpos.x = Mathf.Clamp(groundpos.x, SceneMinX, SceneMaxX);
            groundpos.y = 0f;
            groundpos.z = Mathf.Clamp(groundpos.z, SceneMinZ, SceneMaxZ);
        }
        // Camera rotation around Y only
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) // both buttons
        {
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

            if (invertMouse)
            {
                mouseX = -mouseX;
            }

            rot.y -= mouseX;
            cam.transform.eulerAngles = rot;
        }
        else if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            OnMouseOver?.Invoke(this.gameObject, new CameraRayEventArgs(ray));
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

            OnMouseClick?.Invoke(this.gameObject, new CameraRayEventArgs(ray));
        }
    }

    public void SetCameraGroundPosition(float x, float z)
    {
        groundpos.x = x;
        groundpos.z = z;
    }
}
