using System;
using UnityEngine;

public class Robber : MonoBehaviour, IKnockable
{
    public void KnockOut(float time)
    {
        Debug.Log($"KnockOut for {time} seconds");
    }

    [ContextMenu("Attack")]
    public void KnockOutPlayer()
    {
        Debug.Log("KnockOut");
        Collider[] col = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider coll in col)
        {
            if (coll.TryGetComponent(out IKnockable knockable))
            {
                knockable.KnockOut(2);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}