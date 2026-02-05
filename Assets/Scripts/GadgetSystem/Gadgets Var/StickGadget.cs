using System.Collections;
using UnityEngine;

public class StickGadget : Gadget
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float     setback;
    [SerializeField] float     range;
    
    
    protected override void OnUse()
    {
        Ray baseCast = new Ray(transform.position, transform.forward);
        RaycastHit[] target = Physics.SphereCastAll(baseCast, 0.25f, range);
        foreach (var hit in target)
        {
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForce(-hit.normal * setback, ForceMode.Impulse);
            }
        }
        
    }

   
    
    
    public override void OnPickup()
    {
        rb.isKinematic = true;
    }

    public override void Drop()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce((transform.up + transform.forward)* 5, ForceMode.Impulse);
        
    }

    public override void OnDepleted()
    {
        base.OnDepleted();
    }
}
