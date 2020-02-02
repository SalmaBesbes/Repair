using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    public static Timer TimerClass;

    public float MaxTimer;
    public SpriteRenderer PopcornSprite;

    public float myTimer;

    public Sprite[] PopCornsTimer;
    // Start is called before the first frame update
    void Start()
    {
        TimerClass = this;
        MaxTimer = 8f;
        myTimer = MaxTimer;
    }

    // Update is called once per frame
    void Update()
    {
        myTimer -= Time.deltaTime;

        PopcornState(Mathf.RoundToInt(myTimer));
    }

    public void ResetTimer()
    {
        myTimer = MaxTimer;
    }

    public void loseTime(float Value)
    {
        myTimer -= Value;
    }

    public void PopcornState(int value)
    {
        switch (value)
        {
            case 8:
                PopcornSprite.sprite = PopCornsTimer[0];
                break;
            case 7:
                PopcornSprite.sprite = PopCornsTimer[1];
                break;
            case 6:
                PopcornSprite.sprite = PopCornsTimer[2];
                break;
            case 5:
                PopcornSprite.sprite = PopCornsTimer[3];
                break;
            case 4:
                PopcornSprite.sprite = PopCornsTimer[4];
                break;
            case 3:
                PopcornSprite.sprite = PopCornsTimer[5];
                break;
            case 2:
                PopcornSprite.sprite = PopCornsTimer[6];
                break;
            case 1:
                PopcornSprite.sprite = PopCornsTimer[7];
                break;

        }
    }
}
