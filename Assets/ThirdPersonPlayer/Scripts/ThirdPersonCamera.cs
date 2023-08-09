using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform Player { get; set; }    
    private float mouseX, mouseY;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
            Destroy(transform.parent.gameObject);

        mouseX += Input.GetAxis("Mouse X");
        mouseY -= Input.GetAxis("Mouse Y");

        mouseY = Mathf.Clamp(mouseY, -60f, 60f);

        transform.localRotation = Quaternion.Euler(mouseY, mouseX, 0f);        
    }

    private void LateUpdate()
    {
        if (Player != null)
        {
            transform.position = Player.transform.position;
        }
    }
}
