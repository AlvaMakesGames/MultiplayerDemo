using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public FirstPersonPlayer Owner { get; set; }

    private Rigidbody rb;

    public float bulletLifeTime = 2f;

    private int ricochetCount;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * 1000f);
    }

    void Update()
    {
        bulletLifeTime -= Time.deltaTime; 

        if(bulletLifeTime <=0) 
        {
            Destroy(gameObject);                
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FirstPersonPlayer hitPlayer = collision.gameObject.GetComponent<FirstPersonPlayer>();

            hitPlayer.TakeDamage(Owner);

            Destroy(gameObject);
        }

        ricochetCount++;

        GetComponent<Rigidbody>().AddForce(Vector3.Reflect(transform.forward, collision.GetContact(0).normal));

        if (ricochetCount == 2)
        {
            Destroy(gameObject);
        }
    }
}
