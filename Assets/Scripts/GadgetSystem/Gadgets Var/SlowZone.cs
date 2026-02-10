using Player;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    private Collider collider;
    public Vector3 colliderSize;
    
    public float slowFactor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
        colliderSize = collider.bounds.size;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Controller player))
        {
            player.slowFactor = this.slowFactor;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Controller player))
        {
            player.slowFactor = 1;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(this.transform.position, this.colliderSize);
    }
}
