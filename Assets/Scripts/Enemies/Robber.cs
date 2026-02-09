using System;
using System.Collections;
using UnityEngine;

public class Robber : MonoBehaviour, IKnockable
{
    public bool isStunned = false;
    public void KnockOut(float time)
    {
        if (!isStunned)
        {
            Debug.Log($"KnockOut for {time} seconds");
            StartCoroutine(Stun(time));
            isStunned = true;   
        }
    }

    IEnumerator Stun(float time)
    {
        yield return new WaitForSeconds(time);
        isStunned = false;
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