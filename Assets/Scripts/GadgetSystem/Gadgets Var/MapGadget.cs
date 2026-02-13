using System.Collections;
using GadgetSystem;
using UnityEngine;

public class MapGadget : Gadget
{
    [SerializeField]         Rigidbody rb;
    [SerializeField] private Vector3   readPosition;
    [SerializeField] private float     transitionTime;

    private Coroutine readState;
    
    protected override void OnUse()
    {
        if(readState != null) StopCoroutine(readState);
        readState = StartCoroutine(Read());
    }

    public override void IsTaken()
    {
    }

    public override void Release()
    {
        if(readState != null) StopCoroutine(readState);
        readState = StartCoroutine(Unread());
    }
    
    public override void OnPickup(GadgetController gc)
    {
        gadgetController = gc;
        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(GadgetPickup.gadgetStaticTransform);
        transform.position = GadgetPickup.gadgetStaticTransform.position;
    }

    public override void Drop()
    {
        base.Drop();
        if(readState != null) StopCoroutine(readState);
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }

    IEnumerator Read()
    {
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            transform.localPosition =  Vector3.Lerp(transform.localPosition, readPosition, elapsedTime / transitionTime);
            elapsedTime        += Time.deltaTime;
            yield return null;
        }
    }
    
    
    IEnumerator Unread()
    {
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            transform.localPosition =  Vector3.Lerp(transform.localPosition ,Vector3.zero, elapsedTime / transitionTime);
            elapsedTime        += Time.deltaTime;
            yield return null;
        }
    }
}
