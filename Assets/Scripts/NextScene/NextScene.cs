using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    public int sceneId = 1;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        nextScene();
    }
    // Start is called before the first frame update
    public void nextScene()
    {
        SceneManager.LoadScene(sceneId);
    }
}
