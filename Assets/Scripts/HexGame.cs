using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class HexGame : MonoBehaviour
{
    public MainMenu mainMenu;
    public HexSequenceManager hexSequenceManager;
    public bool GameStarted {get; set;}
    KeyCode menuKeyEscape = KeyCode.Escape;
    KeyCode mouseKey = KeyCode.F1;

    void Awake() {
        GameStarted = false;
        mainMenu.ShowMainMenu();
    }

    public void StartGame() {
        Debug.Log("StartGame");
        GameStarted = true;
        hexSequenceManager.StartSequence();
    }

    public void PauseGame() {
        Debug.Log("PauseGame");
        Time.timeScale = 0;
    }

    public void ResumeGame() {
        Debug.Log("ResumeGame");
        Time.timeScale = 1;
    }

    public void EndGame() {
        Debug.Log("EndGame");
        GameStarted = false;
        Time.timeScale = 0;
    }

    public void QuitGame() {
        Debug.Log("QuitGame");
        Application.Quit();
    }

    public bool isGameRunning() {
        return Time.timeScale != 0;
    }

    void Update() {
        if (Input.GetKey(mouseKey)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
        }
        if (Input.GetKeyDown(menuKeyEscape)) {
            if(GameStarted && isGameRunning()) {
                PauseGame();
            } else if (!GameStarted) {
                StartGame();
            } else {
                ResumeGame();
            }
            mainMenu.UpdateCanvasMenu();
        }
    }
}
