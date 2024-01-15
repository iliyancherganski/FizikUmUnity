using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject center;
    public Vector3 camInitialPos;
    [Header("Sensitivity")]
    public float sensitivity;

    [Header("Rotation")]
    public float minXRotation = -60f;
    public float maxXRotation = 60f;

    [Header("Speed")]
    public float speed = 1;

    [Header("Scroll Scale")]
    public float minScrollScale;
    public float maxScrollScale;
    public float scrollSensitivity = 1f;

    [Header("Keybinds")]
    public KeyCode ResetCameraPositionKey = KeyCode.P;
    public KeyCode SpeedUpCameraKey = KeyCode.Space;

    private float currentXRotation = 0f;

    // Update is called once per frame
    void Update()
    {
        CameraRotation();
        CamMovement();
        ZoomCamera();        
    }
    void CameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            float mouseY = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
            float mouseX = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime * -1;

            currentXRotation -= mouseX;
            currentXRotation = Mathf.Clamp(currentXRotation, minXRotation, maxXRotation);

            center.transform.localRotation = Quaternion.Euler(currentXRotation, 0, 0);

            transform.Rotate(0, mouseY, 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        ResetCameraPosition();
    }
    void ResetCameraPosition()
    {
        if (Input.GetKeyDown(ResetCameraPositionKey))
        {
            transform.position = camInitialPos;
        }
    }
    void CamMovement()
    {
        float ws = -Input.GetAxis("Vertical");
        float ad = -Input.GetAxis("Horizontal");
        Vector3 movement = new Vector3(ad, 0, ws);
        float speedMultiplier = 1;
        if (Input.GetKey(SpeedUpCameraKey))
        {
            speedMultiplier = 2;
        }
        transform.Translate(movement * speed * speedMultiplier * Time.deltaTime, Space.Self);
    }
    void ZoomCamera()
    {
        var scaleZ = center.transform.localScale.z + Input.mouseScrollDelta.y * scrollSensitivity * -.1f;
        center.transform.localScale = new Vector3(1, 1, Mathf.Clamp(scaleZ, minScrollScale, maxScrollScale));
    }
}