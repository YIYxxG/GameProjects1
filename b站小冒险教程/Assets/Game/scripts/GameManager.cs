using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameUiManager gameUImanager;
    public character playerCharacter;
    private bool gameIsOver;
    private void Awake() 
    {
        playerCharacter = GameObject.FindWithTag("Player").GetComponent<character>();    
    }
    private void GameOver()
    {
        gameUImanager.ShowGameOverUI();
    }
    public void GameIsFinished()
    {
        gameUImanager.ShowGameIsFinished();
    }
    void Update()
    {
        if(gameIsOver)
        {
            return;
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            gameUImanager.TogglePauseUI();
        }
        if(playerCharacter.CurrentState == character.CharacterState.Dead)
        {
            gameIsOver = true;
            GameOver();
        }
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainManu");
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ShowGameOverUI()
    {
        
    }
}
