using System.Collections;
using UnityEngine;

public class MapGadget : Gadget
{
    [SerializeField] Rigidbody rb;
    [SerializeField] private Vector3 readPosition;
    [SerializeField] private float transitionTime;

    private Coroutine readState;
    
    protected override void OnUse()
    {
        if(readState != null) StopCoroutine(readState);
        readState = StartCoroutine(Read());
    }
    
    
    public override void Release()
    {
        if(readState != null) StopCoroutine(readState);
        readState = StartCoroutine(Unread());
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

    IEnumerator Read()
    {
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, readPosition, elapsedTime / transitionTime);
            elapsedTime             += Time.deltaTime;
            yield return null;
        }
    }
    
    
    IEnumerator Unread()
    {
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, elapsedTime / transitionTime);
            elapsedTime             += Time.deltaTime;
            yield return null;
        }
    }
}
