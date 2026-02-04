using System.Collections;
using UnityEngine;

public class Ball :  Gadget
{
    bool             isUsed = false;
    public float     speed;
    public Rigidbody rb;

    protected override void OnUse()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        isUsed = true;
        Debug.Log($"{name} : {isUsed}");
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!isUsed)return;
        
        Debug.Log(collision.gameObject.name);
        Destroy(gameObject);
    }

    public override void OnPickup()
    {
        rb.isKinematic = true;
    }

    public override void Drop()
    {
        if(transform.parent != null) transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(transform.up * 4f, ForceMode.Impulse);
    }


    public override void OnDepleted()
    {
        base.OnDepleted();
    }
}
