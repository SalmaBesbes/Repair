using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThorWpSlider : MonoBehaviour
{

    public static ThorWpSlider TWS;
    private Slider gauge;
    // Start is called before the first frame update
    void Start()
    {
        TWS = this;
        gauge = GetComponent<Slider>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.LogWarning(gauge.value);           
        }       
    }


    



}
