using CarScripts;
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

        if (other.TryGetComponent(out CarController car) && other.attachedRigidbody != null)
        {
            other.attachedRigidbody.angularDamping = 10f;
        }

        if (other.TryGetComponent(out EnemyController enemyController))
        {
            Debug.Log("Enemy In");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Controller player))
        {
            player.slowFactor = 1;
        }
        
        if (other.TryGetComponent(out CarController car) && other.attachedRigidbody != null)
        {
            other.attachedRigidbody.angularDamping = 0f;
        }
        
        if (other.TryGetComponent(out EnemyController enemyController))
        {
            Debug.Log("Enemy Out");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(this.transform.position, this.colliderSize);
    }
}
