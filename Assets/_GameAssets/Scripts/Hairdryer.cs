using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hairdryer : MonoBehaviour
{
    Image sp;
    Color c;
    Vector3 scaleChange;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "iceberg")
        {
            if (sp ==null)
            {
                sp = collision.GetComponent<Image>();
            }
            else
            {
                scaleChange = new Vector3(-0.01f, -0.01f, -0.01f);
                collision.transform.localScale += scaleChange;
                c = sp.color;
                c.a = c.a - 0.015f;
                sp.color = c;
            }
        }
    }
}
