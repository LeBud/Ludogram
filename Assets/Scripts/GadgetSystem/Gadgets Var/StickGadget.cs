using System.Collections;
using UnityEngine;

public class StickGadget : Gadget
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float     setback;
    [SerializeField] float     range;
    [SerializeField] float     movementTime;
    [SerializeField] float     cooldown;

    private bool canUse = true;
    
    //COOLDOWN
    
    protected override void OnUse()
    {
        if (!canUse) return;
        StartCoroutine(Cooldown());
        StartCoroutine(AnimateGadget());
        Ray baseCast = new Ray(GadgetController.concernedPlayerCamera.transform.position, GadgetController.concernedPlayerCamera.transform.forward);
        RaycastHit[] target = Physics.SphereCastAll(baseCast, 0.25f, range);
        foreach (var hit in target)
        {
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForce(-hit.normal * setback, ForceMode.Impulse);
            }
        }
    }

    IEnumerator Cooldown()
    {
        canUse = false;
        yield return new WaitForSeconds(cooldown);
        canUse = true;
    }

    IEnumerator AnimateGadget()
    {
        transform.localRotation = Quaternion.Euler(-90, 0, 0);
        yield return new WaitForSeconds(0.15f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
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

    public override void OnDepleted()
    {
        base.OnDepleted();
    }
}
