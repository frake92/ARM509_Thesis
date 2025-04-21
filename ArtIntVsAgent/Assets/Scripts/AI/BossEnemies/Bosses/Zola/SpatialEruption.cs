using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SpatialEruption : MonoBehaviour
{
    bool inRange = false;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerWalkingTag"))
        {
            inRange = true;
        }
        {
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerWalkingTag"))
        {
            inRange = false;
        }
        {
            inRange = false;
        }
    }

    public void Boom()
    {
        if (inRange)
        {
            ZolaImitation.Instance.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
        }
    }
}