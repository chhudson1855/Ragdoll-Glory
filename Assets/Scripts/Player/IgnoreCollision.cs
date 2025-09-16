using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            for(int k = i + 1; k < colliders.Length; k++)
            {
                // Check if either collider has the "Jumpable" tag.  If so, don't ignore collisions.
                if (colliders[i].gameObject.CompareTag("Jumpable") || colliders[k].gameObject.CompareTag("Jumpable"))
                {
                    continue; // Skip to the next iteration of the inner loop.
                }

                Physics2D.IgnoreCollision(colliders[i], colliders[k]);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            Physics2D.IgnoreCollision(this.gameObject.GetComponent<Collider2D>(), coll.gameObject.GetComponent<Collider2D>());
        }
    }
}
