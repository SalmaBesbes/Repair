using DoozyUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarwarsAnimEvent : MonoBehaviour
{
    public void triggerPlayableScene()
    {
        UIManager.HideUiElement("swintro", "GGJ");
        UIManager.ShowUiElement("blackfade", "GGJ");
    }

    public void triggerTitanic()
    {
        SceneManager.LoadScene("Titanic");
    }

    public void triggerstarwars()
    {
        SceneManager.LoadScene(2);
    }

    public void triggerThanos()
    {
        SceneManager.LoadScene(3);
    }

    public void triggerNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public GameObject clap;

    public void clapactive()
    {
        clap.SetActive(true);
    }

    public AudioSource _audio;
    public void appear()
    {
        _audio.Play();
    }
}
