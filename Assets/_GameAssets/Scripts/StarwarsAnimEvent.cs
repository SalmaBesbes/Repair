using DoozyUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarwarsAnimEvent : MonoBehaviour
{
    public void triggerPlayableScene()
    {
        UIManager.HideUiElement("swintro", "GGJ");
        UIManager.ShowUiElement("blackfade", "GGJ");
    }
}
