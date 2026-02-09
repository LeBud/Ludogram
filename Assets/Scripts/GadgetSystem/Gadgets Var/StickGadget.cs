using System.Collections;
using Player;
using UnityEngine;

public class StickGadget : Gadget
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
    private bool       canUse = true;
    private float      baseForce;
    private Coroutine  chargeStick;
    
    
    
    //COOLDOWN
    void Start()
    {
        baseForce = setback;
    }
    
    protected override void OnUse()
    {
        switch (heavyStick)
        {
            case true:
                if (!canUse) return;
                if(chargeStick != null) StopCoroutine(chargeStick);
                chargeStick = StartCoroutine(LoadForce());
                break;
            case false:
                if (!canUse) return;
                StartCoroutine(AnimateGadget());
                Hit();
                break;
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
                knockable.KnockOut(knockTime);
            }
        }
    }

    public override void Release()
    {
        if (canUse)
        {
            if (heavyStick)
            {
                Hit();
                StopCoroutine(chargeStick);
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                setback                 = baseForce;
            }
        
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
        transform.localRotation = Quaternion.Euler(-90, 0, 0);
        yield return new WaitForSeconds(0.15f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    IEnumerator LoadForce()
    {
        float baseForce = setback;
        float elapsed   = 0;
        while (elapsed < maxLoadTime)
        {
            setback                 =  Mathf.Lerp(baseForce, maxLoadForce, elapsed / maxLoadTime);
            transform.localRotation =  Quaternion.Lerp(Quaternion.Euler(0, 0, 0), Quaternion.Euler(-60, 0, 0), elapsed / maxLoadTime);
            elapsed                 += Time.deltaTime;
            yield return null;
        }
    }

    public override void OnPickup()
    {
        rb.isKinematic = true;
    }

    public override void Drop()
    {
        base.Drop();
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }

    public override void OnDepleted()
    {
        Destroy(gameObject, 1);
    }
}
