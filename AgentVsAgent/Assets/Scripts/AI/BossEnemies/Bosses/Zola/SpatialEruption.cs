using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SpatialEruption : MonoBehaviour
{
    public bool inRange = false;
    public ZolaRLAgent hitOpponent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.LogError(hitOpponent.gameObject.name);
        //Debug.LogError(collision.gameObject.tag);
        if (hitOpponent != null && collision.gameObject.tag == "PlayerWalkingTag" && collision.transform.parent.gameObject == hitOpponent.gameObject)
        {   
            inRange = true;
            //Debug.LogError("In range of " + hitOpponent.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {   
        
        if (hitOpponent != null && collision.gameObject.tag == "PlayerWalkingTag" && collision.transform.parent.gameObject == hitOpponent.gameObject)
        {   
            inRange = false;
        }
    }

    public void Boom()
    {
        if (inRange && hitOpponent != null)
        {
            hitOpponent.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
        }
    }
}