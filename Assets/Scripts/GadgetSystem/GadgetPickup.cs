using UnityEngine;

public class GadgetPickup : MonoBehaviour
{
    [SerializeField] private GadgetInventory playerInventory;
    [SerializeField] private Transform       gadgetTransform;
    [SerializeField] private LayerMask       gadgetLayerMask;
    [SerializeField] private float           pickupRange = 2f;
    
    private          InputSystem_Actions playerActions;
    private const    int                 MAX_PICKUP_COUNT = 5;
    private          Collider[]          hitColliders;
    

    #region InputSystem

    void OnEnable()
    {
        playerActions = new InputSystem_Actions();
        playerActions.Enable();

        playerActions.Player.PickUpGadget.started += _ =>TryPickupNearbyGadget();
    }

    void OnDisable()
    {
        playerActions.Player.PickUpGadget.started -= _ =>TryPickupNearbyGadget();
        playerActions.Disable();
    }

    #endregion

    void Start()
    {
        hitColliders = new Collider[MAX_PICKUP_COUNT];
    }
    
    [ContextMenu("Pickup")]
    private void TryPickupNearbyGadget()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, pickupRange, hitColliders, gadgetLayerMask);

        Transform closestObj = hitColliders[0].transform;
        for (int i = 0; i < numColliders; i++)
        {
            if (Vector3.Distance(transform.position, hitColliders[i].transform.position) < Vector3.Distance(transform.position, closestObj.position))
            {
                closestObj = hitColliders[i].transform;
            }
        }
        Debug.Log(closestObj.name + "est le plus proche : " + Vector3.Distance(transform.position, closestObj.position));
        Gadget gadget = closestObj.GetComponent<Gadget>();
        
        if (playerInventory.AddGadget(gadget))
        {
            closestObj.position = gadgetTransform.position;
            closestObj.SetParent(gadgetTransform);
            gadget.OnPickup();
            Debug.Log("Ramassé:" + gadget.Name);
        }
        else
        {
            Debug.Log("Inventaire plein !");
        }
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gadgetTransform.position, 0.5f);
    }
}
