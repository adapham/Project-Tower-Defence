using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    //Play again
    public void playAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    //Start play at sence with name is Level2
    public void nextScene()
    {
        SceneManager.LoadScene("Level2");
    }
    //Start play at sence with name is Level1
    public void prevScene()
    {
        SceneManager.LoadScene("Level1");
    }

    public void levelScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("ChooseLevel");
    }
}
