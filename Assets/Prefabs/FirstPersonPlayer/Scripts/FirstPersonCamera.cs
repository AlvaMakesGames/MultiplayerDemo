using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField] private FirstPersonPlayer player;

    private float mouseSens = 200f;
    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(!player.ChatOpen)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -80f, 80f);

            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            player.gameObject.transform.Rotate(Vector3.up * mouseX);
        }

    }
}
