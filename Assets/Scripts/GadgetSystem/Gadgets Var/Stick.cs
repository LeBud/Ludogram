using System.Collections;
using Player;
using UnityEngine;

public class Stick : Gadget
{
    [SerializeField] Rigidbody rb;
    [SerializeField] LayerMask hitLayerMask;
    [SerializeField] float knockTime = 0.5f;
    [Header("Basic Stick")]
    [SerializeField] float     setback;
    [SerializeField] float     range;
    [SerializeField] float     movementTime;
    [SerializeField] float     cooldown;
    
    [Header("Heavy Stick")]
    [SerializeField] bool heavyStick;
    [SerializeField] float maxLoadTime;
    [SerializeField] float maxLoadForce;
    
    private int        currentLoadTime;
    private Controller player;
    public bool       canUse = true;
    private float      baseForce;
    private Coroutine  chargeStick;
    
    
    
    //COOLDOWN
    void Start()
    {
        baseForce = setback;
    }
    
    protected override void OnUse()
    {
        if (canUse)
        {
            switch (heavyStick)
            {
                case true:
                    if(chargeStick != null) StopCoroutine(chargeStick);
                    chargeStick = StartCoroutine(LoadForce());
                    break;
                case false:
                    StartCoroutine(AnimateGadget());
                    Hit();
                    break;
            }
        }
        
        
    }

    private void Hit()
    {
        Ray baseCast = new Ray(GadgetController.concernedPlayerCamera.transform.position, GadgetController.concernedPlayerCamera.transform.forward);
        RaycastHit[] target = Physics.SphereCastAll(baseCast, 0.25f, range, hitLayerMask);
        foreach (var hit in target)
        {
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForce(-hit.normal * setback, ForceMode.Impulse);
            }
            if (hit.collider.gameObject.TryGetComponent(out IKnockable knockable))
            {
                knockable.KnockOut(knockTime, -hit.normal * setback);
            }
        }
    }

    public override void Release()
    {
        if (canUse)
        {
            
            if (heavyStick)
            {
                Debug.Log("Release");
                Hit();
                if (chargeStick != null) StopCoroutine(chargeStick);
                setback = baseForce;
            }
        
            chargeStick = null;
            StartCoroutine(Cooldown());
        }
        
        
    }
    IEnumerator Cooldown()
    {
        canUse = false;
        base.Release();
        yield return new WaitForSeconds(cooldown);
        canUse = true;
    }

    IEnumerator AnimateGadget()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.x + 90, transform.rotation.y, transform.rotation.z);
        yield return new WaitForSeconds(0.15f);
        transform.rotation = Quaternion.identity;
    }

    public override void IsTaken()
    {
        if (target != null)
        {
            //transform.forward = target.forward;
            Vector3 targetPos                         = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velref, smoothTime);
            if(chargeStick == null)  transform.rotation = Quaternion.LookRotation(target.forward);
        }
    }

    IEnumerator LoadForce()
    {
        float      baseForce   = setback;
        float      elapsed     = 0;
        Quaternion targetAngle = Quaternion.Euler(transform.rotation.x +  60, transform.rotation.z, transform.rotation.z) ;
        while (elapsed < maxLoadTime)
        {
            setback                 =  Mathf.Lerp(baseForce, maxLoadForce, elapsed / maxLoadTime);
            transform.rotation =  Quaternion.Lerp(transform.rotation, targetAngle, elapsed / maxLoadTime);
            elapsed                 += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetAngle;
    }

    public override void OnPickup(GadgetController gc)
    {
        gadgetController = gc;
        rb.isKinematic = true;
    }

    public override void Drop()
    {
        base.Drop();
        rb.isKinematic = false;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }

    public override void OnDepleted()
    {
        Destroy(gameObject, 1);
    }
}
