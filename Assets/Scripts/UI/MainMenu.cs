using UnityEngine;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    public HexGame hexGame;
    bool creditsScreen = false;
    KeyCode menuKeyReturn = KeyCode.Return;
    KeyCode menuKeyUp = KeyCode.UpArrow;
    KeyCode menuKeyDown = KeyCode.DownArrow;
    public Text[] textMenuList;
    int activeTextMenu = 0;

    void Awake() {
        activeTextMenu = 0;
        SetTextMenuActive(activeTextMenu);
    }

    public void UpdateCanvasMenu() {
        if (hexGame.GameStarted) {
            UpdateStartedGameMenu();
        } else {
            UpdateNotStartedGameMenu();
        }
    }

    void UpdateNotStartedGameMenu() {
        if(creditsScreen) {
            ShowMainMenu();
        } else {
            ShowCredits();
        }
    }

    void UpdateStartedGameMenu() {
        if(!hexGame.isGameRunning()) {
            ShowMainMenu();
        } else {
            HideMainMenu();
        }
    }

    public void HideMainMenu() {
        gameObject.SetActive(false);
    }

    void QuitGame() {
        hexGame.QuitGame();
    }

    public void ShowMainMenu() {
        gameObject.SetActive(true);
        gameObject.transform.Find("CreditsDisplayText").gameObject.SetActive(false);
        gameObject.transform.Find("Menu").gameObject.SetActive(true);
        creditsScreen = false;
    }

    public void ShowCredits() {
        gameObject.SetActive(true);
        gameObject.transform.Find("CreditsDisplayText").gameObject.SetActive(true);
        gameObject.transform.Find("Menu").gameObject.SetActive(false);
        creditsScreen = true;
    }

    public void ShowEnd() {
        hexGame.EndGame();
        gameObject.SetActive(true);
        gameObject.transform.Find("CreditsDisplayText").gameObject.SetActive(true);
        gameObject.transform.Find("Menu").gameObject.SetActive(false);
        activeTextMenu = 0;
        SetTextMenuActive(activeTextMenu);
        creditsScreen = true;
    }

    public void NewGame() {
        hexGame.GameStarted = false;
        activeTextMenu = 0;
        SetTextMenuActive(activeTextMenu);
        creditsScreen = false;
    }

    void SetTextMenuActive(int activeTextMenuIndex) {
        for(int textMenuIndex = 0; textMenuIndex < textMenuList.Length; textMenuIndex++) {
            textMenuList[textMenuIndex].gameObject.SetActive(false);
        }
        textMenuList[activeTextMenuIndex].gameObject.SetActive(true);
    }

    void Update()
    {
        if (!creditsScreen) {
            if(Input.GetKeyDown(menuKeyUp)) {
                if (activeTextMenu > 0) {
                    activeTextMenu--;
                }
                SetTextMenuActive(activeTextMenu);
            }
            if(Input.GetKeyDown(menuKeyDown)) {
                if (activeTextMenu < textMenuList.Length - 1) {
                    activeTextMenu++;
                }
                SetTextMenuActive(activeTextMenu);            
            }
            if(Input.GetKeyDown(menuKeyReturn)) {
                switch (activeTextMenu) {
                case 0:
                    if (!hexGame.GameStarted) {
                        hexGame.StartGame();
                    } else {
                        hexGame.ResumeGame();
                    }
                    HideMainMenu();
                    break;
                case 1:
                    QuitGame();
                    break;
                case 2:
                    ShowCredits();
                    break;
                default:
                    break;            
                }
            }
        }
    }
}
