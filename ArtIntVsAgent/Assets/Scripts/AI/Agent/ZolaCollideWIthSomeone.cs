using UnityEngine;

public class ZolaCollideWIthSomeone : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ZolaWalking"))
        {
            Debug.Log("ÜTKÖZTÜNK! Collided with ZolaWalking");
            transform.parent.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            other.transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ZolaWalking"))
        {   
            Debug.Log("MÁR NEM ÜTKÖZÜNK! Not collided with ZolaWalking");
            transform.parent.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            other.transform.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
