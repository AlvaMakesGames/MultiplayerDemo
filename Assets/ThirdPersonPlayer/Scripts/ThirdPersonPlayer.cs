using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ThirdPersonPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;

    private CharacterController controller;
    private GameObject cameraRigInst;
    private Transform cam;
    private float speed = 5f;
    private Vector3 vertVel;
    private float gravity = 15f;
    private float jumpForce = 5f;
    private float rotationSmooth = 0.1f;
    private float turnSmoothVelocity;

    public override void OnStartAuthority()
    {
        enabled = true; 
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        InitialiseCamera();
    }

    void Update()
    {
        Movement();
        ToggleCursor();
        Shoot();
    }

    void ToggleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Cursor.visible = !Cursor.visible;

            if (Cursor.visible)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void Movement()
    {
        Sprint();

        //Calculate vertical movement first
        if (IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.Space))
                vertVel.y = jumpForce;
        }
        else
        {
            vertVel.y -= gravity * Time.deltaTime;
        }

        controller.Move(vertVel * Time.deltaTime);

        //Calculate 2D movement next
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(hor, 0f, ver);

        if (direction.magnitude >= 0.1f)
        {
            //Determine rotation angle
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            //Smooth between current and target angle
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmooth);

            //Apply rotation to the transform
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            //If direction magnitude greater than 1 (through strafing/lateral movement), then normalise it
            if (direction.sqrMagnitude > 1)
                direction.Normalize();

            //Construct a movement vector using angle and normalised direction
            Vector3 moveDirection = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward) * direction.sqrMagnitude;

            //Apply movement to the character controller
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
    }

    void Shoot()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CmdShoot();
        }
    }

    [Command]
    void CmdShoot()
    {
        GameObject go = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        go.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward * 2000f);
        NetworkServer.Spawn(go);
    }

    bool IsGrounded()
    {
        Ray ray = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y), controller.bounds.center.z), Vector3.down);
        return (Physics.Raycast(ray, 0.3f));
    }

    void Sprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = 10f;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = 5f;
        }
    }

    void InitialiseCamera()
    {
        cameraRigInst = Instantiate(cameraRig);
        cameraRigInst.name = cameraRig.name;
        cameraRigInst.transform.rotation = transform.rotation;
        ThirdPersonCamera camScript = cameraRigInst.GetComponentInChildren<ThirdPersonCamera>();
        camScript.Player = transform;        
        cam = camScript.GetComponentInChildren<Camera>().transform;
    }
}
