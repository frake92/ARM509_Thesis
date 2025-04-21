using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TemporalSurge : MonoBehaviour
{
    public ZolaRLAgent opponent;
   
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject != this.transform.parent.gameObject)
        {
            opponent.TakeDamage(BaseStatsForZolaBoss.temporalSurgeDamage);
        }
    }
}
