using System.Collections;
using UnityEngine;

public class Ball :  Gadget
{
    public  Camera    currentCamera;
    public  float     speed;
    public  Rigidbody rb;
    private bool      isUsed = false;

    protected override void OnUse()
    {
        isUsed = true;
        target = null;
        transform.SetParent(null);
        rb.isKinematic = false;
        Ray ray = new Ray(currentCamera.transform.position, currentCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            rb.AddForce((hit.point - transform.position).normalized * speed, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(ray.direction * speed, ForceMode.Impulse);
        }
    }

    public override void OnPickup(GadgetController gc)
    {
        currentCamera    = gc.player.playerCamera; 
        gadgetController = gc;
        rb.isKinematic   = true;
        isUsed           = false;
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
