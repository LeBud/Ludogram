using System.Collections;
using UnityEngine;

public class Ball :  Gadget
{
    public  float     speed;
    public  Rigidbody rb;
    private bool      isUsed = false;

    protected override void OnUse()
    {
        isUsed = true;
        target = null;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    public override void OnPickup()
    {
        rb.isKinematic = true;
        isUsed = false;
    }

    public override void Drop()
    {
        base.Drop();
        if (isUsed) return;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }
    
    
}
