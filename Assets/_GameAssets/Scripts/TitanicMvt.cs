using DG.Tweening;
using DoozyUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitanicMvt : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject clap;
    void Start()
    {
        transform.DOMove(new Vector3(40.9f, -2.7f, 90f), 200f);
        transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 15f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "trigger")
        {
            clap.SetActive(true);
            UIManager.HideUiElement("Inventaire", "GGJ");
            UIManager.HideUiElement("Slot", "GGJ");
            //SceneManager.LoadScene(2);
        }
    }

}
