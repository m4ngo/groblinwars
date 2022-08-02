using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float clampAngle = 90f;

    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private Vector3 crouchPosition;
    [SerializeField] private Vector3 crawlPosition;

    private float verticalRotation;
    private float horizontalRotation;

    private bool cursorLocked;

    private void OnValidate()
    {
        if (player == null)
            player = GetComponent<Player>();
    }

    private void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
    }

    private void Update()
    {
        SetCursorMode();

        if (Cursor.lockState == CursorLockMode.Locked)
            Look();

        Debug.DrawRay(transform.position, transform.forward * 2f, Color.green);
    }

    private void Look()
    {
        sensitivity = UIManager.Singleton.GetSensitivity();

        float mouseVertical = -Input.GetAxis("Mouse Y");
        float mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += mouseVertical * sensitivity * 100 * Time.deltaTime;
        horizontalRotation += mouseHorizontal * sensitivity * 100 * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    public void Crouch(bool isCrouching, bool isCrawling, bool isMounted)
    {
        if(isMounted)
            transform.localPosition = crawlPosition;
        else if (isCrawling)
            transform.localPosition = crawlPosition;
        else if (isCrouching)
            transform.localPosition = crouchPosition;
        else
            transform.localPosition = defaultPosition;
    }

    private void SetCursorMode()
    {
        if (!UIManager.Singleton.GetPaused())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }


        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
