using System;
using Manager;
using UnityEngine;

public class MoneyBag : Gadget
{
    private                  Camera    currentCamera;
    [SerializeField] public Rigidbody rb;
    public  int       moneyValue;
    [SerializeField] private LayerMask moneyZoneLayerMask;
    [SerializeField] private float     launchSpeed;
    private                  bool      isUsed = false;
    public bool isPickedUp {get; private set;}

    private void Start()
    {
        GameManager.instance.moneyManager.RegisterAllBags(this);
    }

    protected override void OnUse()
    {
        // isUsed = true;
        // target = null;
        // transform.SetParent(null);
        // rb.isKinematic = false;
        // Ray ray = new Ray(currentCamera.transform.position, currentCamera.transform.forward);
        // if (Physics.Raycast(ray, out RaycastHit hit))
        // {
        //     rb.AddForce((hit.point - transform.position).normalized * launchSpeed, ForceMode.Impulse);
        // }
        // else
        // {
        //     rb.AddForce(ray.direction * launchSpeed, ForceMode.Impulse);
        // }
    }

    public override void OnPickup(GadgetController gc)
    {
        currentCamera    = gc.player.playerCamera;
        gadgetController = gc;
        rb.isKinematic   = true;
        isUsed           = false;
        col.enabled = false;
        isPickedUp = true;
    }

    public override void Drop()
    {
        base.Drop();
        if (isUsed) return;
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;
        isPickedUp = false;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }

    public void EnableCollider() {
        col.enabled = true;
    }

    public void DisableCollider() {
        col.enabled = false;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == moneyZoneLayerMask)
        {
            Destroy(gameObject);
        }
    }
}
