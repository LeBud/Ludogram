using System.Collections;
using UnityEngine;

public class Ball :  Gadget
{
    public float     speed;
    public Rigidbody rb;

    protected override void OnUse()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    public override void OnPickup()
    {
        rb.isKinematic = true;
    }

    public override void Drop()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }
    
    
}
