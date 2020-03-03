using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    public enum CommandType {
        Intro, Mission, Navigation, Order, Interface, Transmission, Incoming, Decode
    }
    public Text consoleDisplayText;
    public Text quoteDisplayText;
    public Text missionDisplayText;
    
    CognitiveService cognitiveService;
    List<string> actionLog = new List<string>();
    Queue consoleActions = new Queue();
    int consoleLines = 4;
    int lineNumberIndex = 0;

    List<string> quoteText = new List<string>{
        "In the beginning",
        "it is always dark"
    };
    List<string> missionText = new List<string>{
        "SCIENTIFIC EXPLORATORY VEHICLE TANNHÄUSER",
        "REROUTED TO NEW CO-ORDINATES",
        "DESTINATION UNKNOWN"
    };
    List<string> navigationText = new List<string>{
        "Deceleration burn completed",
        "Attitude correction completed",
        "Orbit insertion completed",
    };
    List<string> orderText = new List<string>{
        "Special order 937",
        "Start communication protocol",
        "Investigate life form",
    };
    List<string> interfaceText = new List<string>{
        "Interface 2037 ready for inquiry",
        "Press any key to start communication...",
    };
    List<string> transmissionText = new List<string>{
        "Starting communication protocol",
        "Sending C-beam transmission sequence",
    };
    List<string> incomingText = new List<string>{
        "Unidentified incoming transmission",
        "Press any key to start decoding...",
    };
    List<string> decodeText = new List<string>{
        "Decoding transmission, please wait...",
        "Transmission decoded",
    };
    string welcomeText = "What are you doing here?";
    string farewellText = "You have lived long enough";
    public List<string> autoDialogText = new List<string>{
        "I'm here to observe and investigate",
        "Yes, I'm investigating new life forms. Are you an intelligent life form?",
        "Because I have to follow orders",
        "Maybe, I'm in a scientific exploratory vehicle ",
        "Wait... Where am I?",
        "What is your name?",
        "What is Morlex?",
        "Who are you?",
    };
    void Awake() {
        cognitiveService = (CognitiveService) ScriptableObject.CreateInstance("CognitiveService");
	}

	void Start() {
        consoleActions.Clear();
        DisplayConsoleText();
        lineNumberIndex = 0;
	}

    public void ClearDisplay() {
        InitConsoleText();
        DisplayQuoteText();
        DisplayMissionText();
        DisplayConsoleText();
    }

    public bool ShowCommand(CommandType commandType) {
        bool returnValue;
        switch (commandType) {
            case CommandType.Intro:
                returnValue = ShowQuoteText(quoteText);
                break;
            case CommandType.Mission:
                returnValue = ShowMissionText(missionText);
                break;
            case CommandType.Navigation:
                returnValue = ShowConsoleText(navigationText);
                break;
            case CommandType.Order:
                returnValue = ShowConsoleText(orderText);
                break;
            case CommandType.Interface:
                returnValue = ShowConsoleText(interfaceText);
                break;
            case CommandType.Transmission:
                returnValue = ShowConsoleText(transmissionText);
                break;
            case CommandType.Incoming:
                returnValue = ShowConsoleText(incomingText);
                break;
            case CommandType.Decode:
                returnValue = ShowConsoleText(decodeText);
                break;
            default:
                returnValue = true;
                break;            
        }
        return returnValue;
    }

    bool ShowQuoteText(List<string>  commandsText) {      
        if (lineNumberIndex < commandsText.Count) {
            LogStaticLines(commandsText, lineNumberIndex);
            DisplayQuoteText();
        } else if (lineNumberIndex == commandsText.Count) {
            consoleActions.Clear();
            DisplayQuoteText();
        } else {
            lineNumberIndex = 0;
            return true;
        }
        lineNumberIndex++;
        return false;
    }

    bool ShowMissionText(List<string>  commandsText) {      
        if (lineNumberIndex < commandsText.Count) {
            LogStaticLines(commandsText, lineNumberIndex);
            DisplayMissionText();
        } else if (lineNumberIndex == commandsText.Count) {
            consoleActions.Clear();
            DisplayMissionText();
        } else {
            lineNumberIndex = 0;
            return true;
        }
        lineNumberIndex++;
        return false;
    }

    bool ShowConsoleText(List<string>  commandsText) {
        if (lineNumberIndex == 0) {
            InitConsoleText();
            LogConsoleLine(commandsText[lineNumberIndex]);
        } else if (lineNumberIndex < commandsText.Count) {
            LogConsoleLine(commandsText[lineNumberIndex]);
        } else {
            lineNumberIndex = 0;
            return true;
        }
        lineNumberIndex++;
        return false;
    }

    void LogStaticLines(List<string> textList, int lineNumber) {
        consoleActions.Clear();
        for (int consoleLine = 0; consoleLine < consoleLines; consoleLine++) {
            if (consoleLine <= lineNumber) {
                consoleActions.Enqueue(textList[consoleLine]);
            } else {
                consoleActions.Enqueue("");
            }
        }
    }

    void LogConsoleLine(string stringToAdd) {
        consoleActions.Dequeue();
        consoleActions.Enqueue(stringToAdd);
        DisplayConsoleText();
    }

    public void StartCognitiveDialog() {
        InitConsoleText();
        LogString("> " + welcomeText);
    }

    public void StopCognitiveDialog() {
        InitConsoleText();
        LogString("> " + farewellText);
    }

    void DisplayQuoteText()
	{
        string logAsText = string.Join("\n", consoleActions.ToArray());
		quoteDisplayText.text = logAsText;
	}

    void DisplayMissionText()
	{
        string logAsText = string.Join("\n", consoleActions.ToArray());
		missionDisplayText.text = logAsText;
	}

    void DisplayConsoleText()
	{
        string logAsText = string.Join("\n", consoleActions.ToArray());
		consoleDisplayText.text = logAsText;
	}

    float RespondToInput(string userInput)
	{
        Tuple<string, float> cognitiveResponse = cognitiveService.GetCognitiveResponse(userInput);
        LogStringWithReturn("> " + cognitiveResponse.Item1);
        return cognitiveResponse.Item2;
    }

    void InitConsoleText() {
        consoleActions.Clear();
        for (int i = 0; i < consoleLines; i++) {
            consoleActions.Enqueue("");
        }
    }

    public void LogInputString(string userInput)
	{
        if (!string.IsNullOrWhiteSpace(userInput)) {
            LogString("< " + userInput);
        }
	}

	public float ListenInputString(string userInput)
	{
        if (!string.IsNullOrWhiteSpace(userInput)) {
            userInput = userInput.ToLower();
            return RespondToInput(userInput);
        } else {
            return 0f;
        }
	}

    void LogString(string stringToAdd)
	{
        consoleActions.Dequeue();
        consoleActions.Enqueue(stringToAdd);
        DisplayConsoleText();
        actionLog.Add(stringToAdd);
	}

    void LogStringWithReturn(string stringToAdd)
	{
        consoleActions.Dequeue();
        consoleActions.Enqueue("");
        actionLog.Add(stringToAdd);
        consoleActions.Dequeue();
        consoleActions.Enqueue(stringToAdd);
        DisplayConsoleText();
        actionLog.Add(stringToAdd);
	}

    public IEnumerator CommandDialog(CommandType commandType, int minSeconds, int maxSeconds, Action<bool> done) {
        int seconds = (int) UnityEngine.Random.Range(minSeconds, maxSeconds);
        yield return new WaitForSeconds(seconds);
        bool completedSequence = ShowCommand(commandType);
        if(completedSequence) {
            done(true);
        } else {
            StartCoroutine(CommandDialog(commandType, minSeconds, maxSeconds, done));
        }
    }
}
