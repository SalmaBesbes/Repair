using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hairdryer : MonoBehaviour
{
    SpriteRenderer sp;
    Color c;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "iceberg")
        {
            if (sp ==null)
            {
                sp = collision.GetComponent<SpriteRenderer>();
            }
            else
            {
                c = sp.color;
                c.a = c.a - 0.02f;
                sp.color = c;
            }
        }
    }
}
