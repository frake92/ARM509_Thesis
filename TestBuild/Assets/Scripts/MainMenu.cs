using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{


    public GameObject mainMenuUI;
    public GameObject creditsMenuUI;

    public void ZolaVsPlayer()
    {
        PlayerPrefs.SetInt("zolaVariant",0);
        SceneManager.LoadScene("Test-Train");
    }
    public void ZolaVsZola()
    {
        PlayerPrefs.SetInt("zolaVariant", 1);
        SceneManager.LoadScene("Test-Train");
    }
    public void ZolaVsMI()
    {
        PlayerPrefs.SetInt("zolaVariant", 2);
        SceneManager.LoadScene("Test-Train");
    }
    public void ZolaVsZolaWithPlayer()
    {
        PlayerPrefs.SetInt("zolaVariant", 3);
        SceneManager.LoadScene("Test-Train");
    }

    public void OpenCredits()
    {
        mainMenuUI.SetActive(false);
        creditsMenuUI.SetActive(true);
    }
    public void CloseCredits()
    {
        mainMenuUI.SetActive(true);
        creditsMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
