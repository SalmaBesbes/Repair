using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanicMvt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.DOMove(new Vector3(40.9f, -2.7f, 90f), 35f);
        transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 8f);
    }

}
