using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public AudioSource audioSourceUI;
    public AudioClip audioClipUIHover;
    public AudioClip audioClipUIClose;

    void Start()
    {
        MenuSwitched();
    }

    public void StartGame(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void Exit()
    {
        Application.Quit();
    }
    public void MenuSwitched()
    {
        audioSourceUI.PlayOneShot(audioClipUIClose, 0.75f);
    }

    public void MouseEnteredButton()
    {
        audioSourceUI.PlayOneShot(audioClipUIHover, 0.25f);
    }
}
