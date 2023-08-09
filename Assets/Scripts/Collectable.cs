using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Collectable : NetworkBehaviour
{
    [SerializeField] private int amtToAdd;
    public int AmtToAdd => amtToAdd;

    private MeshRenderer meshRenderer;
    private Collider boxCollider;
    private bool isPickedUp;

    [SyncVar]
    public float respawnTime;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<Collider>();
    }
    void Update()
    {
        if (!isPickedUp)
        {
            transform.Rotate(Vector3.up * 45f * Time.deltaTime);
        }
        else
        {
            RpcUnHideItem();
        }
    }

    [ClientRpc]
    public void RpcHideItem()
    {
        meshRenderer.enabled = false;
        boxCollider.enabled = false;
        isPickedUp = true;
    }

    [ClientRpc]
    public void RpcUnHideItem()
    {
        respawnTime += Time.deltaTime;

        if (respawnTime >= 30f)
        {
            isPickedUp = false;
            meshRenderer.enabled = true;
            boxCollider.enabled = true;

            respawnTime = 0;
        }
    }
}
