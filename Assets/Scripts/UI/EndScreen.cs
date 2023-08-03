using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    // Start is called before the first frame update
    private void Start()
    {
        GameManager.instance.OnGameLose += ShowLoseScreen;
        GameManager.instance.OnGameWin += ShowWinScreen;

        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }

    private void ShowWinScreen()
    {
        winScreen.SetActive(true);
        GameManager.instance.PauseGame();
        GameManager.instance.PauseGameTime();
    }

    private void ShowLoseScreen()
    {
        loseScreen.SetActive(true);
        GameManager.instance.PauseGame();
        GameManager.instance.PauseGameTime();
    }
}
