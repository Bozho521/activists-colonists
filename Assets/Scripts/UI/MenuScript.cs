using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public GameObject creditsScreen;
    private bool creditsActive = false;
    public Animator creditsAnim;
    public GameObject tutorialScreen;
    private bool tutorialActive = false;
    public Animator tutorialAnim;
    public GameObject vote;

    private void Start()
    {
        creditsScreen.gameObject.SetActive(false);
        tutorialScreen.gameObject.SetActive(false);
    }


    private void Update()
    {

    }

    public void PlayGame()
    {
        vote.gameObject.SetActive(true);
        Invoke("StartGameNow", 1f);
    }

    private void StartGameNow()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        //Time.timeScale = 1;
    }

    public void BackToMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("I QUIT THE GAME");
    }

    public void Credits()
    {
        if (creditsActive)
        {
            Debug.Log("Credits deactivated.");
            creditsActive = false;
            creditsAnim.SetBool("CreditsActive", false);
            Invoke("CreditsFalse", 1f);
        }
        else
        {
            Debug.Log("Credits activated.");
            creditsActive = true;
            creditsScreen.gameObject.SetActive(true);
            creditsAnim.SetBool("CreditsActive", true);
        }
    }

    public void Tutorial()
    {
        if (tutorialActive)
        {
            tutorialActive = false;
            tutorialAnim.SetBool("TutorialActive", false);
            Invoke("TutorialFalse", 1f);
        }
        else
        {
            tutorialActive = true;
            tutorialScreen.gameObject.SetActive(true);
            tutorialAnim.SetBool("TutorialActive", true);
        }
    }

    public void CreditsFalse()
    {
        creditsScreen.gameObject.SetActive(false);
    }

    public void TutorialFalse()
    {
        tutorialScreen.gameObject.SetActive(false);
    }
}
