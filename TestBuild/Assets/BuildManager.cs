using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{

    public static BuildManager Instance { get; private set; }
    public GameObject gameUI;
    public GameObject Player;
    public GameObject Zola1;
    public GameObject Zola2;
    public GameObject Zola3;
    public GameObject Zola4;
    public GameObject pauseMenu;

    public float gameTime = 180f;
    private float currentTime;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    private bool isGameActive = true;

    void Awake()
    {
        if(PlayerPrefs.GetInt("zolaVariant") == 0)
        {
            Zola1.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("zolaVariant") == 1)
        {
            Zola2.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("zolaVariant") == 2)
        {
            Zola3.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("zolaVariant") == 3)
        {
            Zola4.SetActive(true);
        }
    }
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Cursor.visible = true;
        Application.targetFrameRate = 60;
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        currentTime = gameTime;
        Time.timeScale = 1f;


        ZolaRLAgent.Instance.HP = BaseStatsForZolaBoss.zolaMaxHP;
        ZolaRLAgent.Instance.hpBar.value = 1f;
        PlayerHP.Instance.currentHP = PlayerHP.Instance.maxHP;
        PlayerHP.Instance.hpBar.value = 1f;
        PlayerHP.Instance.hpPotions = 2;
        
    }

    void Update()
    {

        if (isGameActive)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else if(currentTime <= 0)
            {
                GameOver();
                gameOverText.text = "Game Over! Nem tudtad időben legyőzni Zolát!";
            }
        }
    }


    public void GameOver()
    {   
        Cursor.visible = true;
        isGameActive = false;
        
        if (Player != null)
            Player.SetActive(false);
            
        DeactivateAllZolas();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    public void DeactivateAllZolas()
    {
        if (Zola1 != null) Zola1.SetActive(false);
        if (Zola2 != null) Zola2.SetActive(false);
        if (Zola3 != null) Zola3.SetActive(false);
        if (Zola4 != null) Zola4.SetActive(false);
    }
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    


/*
    public void StartGame()
    {   

        Cursor.visible = false;
        Time.timeScale = 1f;
        menuUI.SetActive(false);
        gameUI.SetActive(true);
        Player.SetActive(true);
        
        
        isGameActive = true;
    }
*/


    public void PauseGame()
    {
        Cursor.visible = true;
        isGameActive = false;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }
    public void ResumeGame()
    {
        Cursor.visible = false;
        Time.timeScale = 1f;
        isGameActive = true;
        pauseMenu.SetActive(false);
    }
    public void QuitToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }


}
