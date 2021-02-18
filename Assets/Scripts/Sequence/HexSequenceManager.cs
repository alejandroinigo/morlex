using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class HexSequenceManager : MonoBehaviour
{   
    // ### Set Demo to test terrain generation pressing 'C'
    enum Environment {
        Demo, Development, Production
    }
    enum Sequence {
        Intro, Mission, Orbit, Interaction, Cognitive, Hole, Cube, End
    }
    enum SequenceIntro {
        Intro,
    }
    enum SequenceMission {
        SpaceNavigation, SpaceFade, Dialog, DialogFade,
    }
    enum SequenceOrbit {
        Insertion, InsertionFade, OrderDialog,
    }
    enum SequenceInteraction {
        Dialog, StartCommunication, Generation, TransmissionDialog, Decode,
    }
    enum SequenceCognitive {
        IncomingDialog, Dialog, ClearDialog, StopDialog,
    }
    enum SequenceHole {
        Creation, Descent,
    }
    enum SequenceCube {
        Fade, Load, Zoom, Stop, Destroy,
    }
    enum SequenceEnd {
        End,
    }
    public InputField inputField;
    public DialogController dialogController;
    public CanvasLoader canvasLoader;
    public HexaoidGenerator hexaoidGenerator;
    public HexCubeUnit hexCubeUnitPrefab;
    public AudioSource AmbientAudioSource;
    public MainMenu mainMenu;
    Sequence nextSequence;
    SequenceIntro nextSequenceIntro;
    SequenceMission nextSequenceMission;
    SequenceOrbit nextSequenceOrbit;
    SequenceInteraction nextSequenceInteraction;
    SequenceCognitive nextSequenceCognitive;
    SequenceHole nextSequenceHole;
    SequenceCube nextSequenceCube;
    SequenceEnd nextSequenceEnd;
    HexCubeUnit hexCubeUnitInstance;
    int hexMapSize;
    int acceptedInputs;
    bool waitPressAnyKey;
    // ### Time and wait iterations Control Vars
    int introSeconds = 3;
    int missionSeconds = 2;
    int minCommandSeconds = 2;
    int maxCommandSeconds = 4;
    int minIncomingDialogSeconds = 5;
    int maxIncomingDialogSeconds = 6;
    int minDialogListenSeconds = 2;
    int maxDialogListenSeconds = 4;
    int delayNone = 0;
    int delayDefault = 1;
    int delayInteractionGeneration = 10;
    int delayInteractionDecode = 3;
    int delayMissionDialog = 2;
    int delayCognitiveIncomingDialog = 3;
    int delayHoleCreation = 10;
    int delayCubeFade = 3;
    int delayCubeDestroy = 3;
    int maxAcceptedInputs;
    // ### Autodialog: Set true to activate automatic answers 
    int delayAutoDialog = 4;
    bool autoDialog = false;
    int autoDialogTextIndex;
    int autoDialogtypeIndex = 1;
    static float minCharactersperMinute = 350f;
    static float maxCharactersperMinute = 2000f;
    float minTypeDialogSeconds = 60f / minCharactersperMinute;
    float maxTypeDialogSeconds = 60f /maxCharactersperMinute;

    Environment environment = Environment.Production;

    void LoadConfiguration() {
        if(environment == Environment.Demo) {
            hexMapSize = 40;
        } else if(environment == Environment.Development) {
            hexMapSize = 5;
            introSeconds = 1;
            missionSeconds = 1;
            minCommandSeconds = 1;
            maxCommandSeconds = 1;
            minIncomingDialogSeconds = 1;
            maxIncomingDialogSeconds = 1;
            minDialogListenSeconds = 1;
            maxDialogListenSeconds = 1;
            delayInteractionGeneration = 1;
            delayInteractionDecode = 1;
            delayMissionDialog = 1;
            delayCognitiveIncomingDialog = 1;
            delayHoleCreation = 1;
            delayCubeFade = 1;
            delayCubeDestroy = 1;
            maxAcceptedInputs = 2;
            // hexaoidGenerator.HexaoidClimateEvolution = false;
        } else {
            hexMapSize = 60;
            maxAcceptedInputs = autoDialog ? dialogController.autoDialogText.Count - 1 : 10;
        }
    }
    
    void Awake() {
        inputField.onEndEdit.AddListener(AcceptStringInput);
    }
    
    void Start() {
        LoadConfiguration();
        mainMenu.ShowMainMenu();
        mainMenu.NewGame();
        CreateHexMap(hexMapSize);
    }

    void InitSubSequences() {
        nextSequenceIntro = 0;
        nextSequenceMission = 0;
        nextSequenceOrbit = 0;
        nextSequenceInteraction = 0;
        nextSequenceCognitive = 0;
        nextSequenceHole = 0;
        nextSequenceCube = 0;
        nextSequenceEnd = 0;
    }

    public void StartSequence() {
        InitGame();
        if (environment == Environment.Demo) {
            Cursor.lockState = CursorLockMode.None;
            waitPressAnyKey = false;
            ShowHexMap(true);
            SetSpaceshipZoomBottom();
            HexGameCamera.SetRotation(120f);
            mainMenu.HideMainMenu();
            canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Mission);
            StartCoroutine(canvasLoader.Fade(CanvasLoader.FadeDirection.Out, 0.1f, fadeDone => {}));
            hexaoidGenerator.HexaoidClimateEvolution = false;
        } else if(environment == Environment.Development) {
            ShowHexMap(true);
            SetSpaceshipZoomBottom();
            StartSpaceshipRotation();
            nextSequence = Sequence.Cube;
            Cursor.lockState = CursorLockMode.None;
            canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Fps);
            // hexaoidGenerator.HexaoidClimateEvolution = false;
        } else {
            nextSequence = Sequence.Intro;
        }
        if (environment != Environment.Demo) {
            StartCoroutine(DelayNextSequence(0));
        }
    }

    void InitGame() {
        InitSubSequences();
        EventSystem.current.SetSelectedGameObject(null, null);
        dialogController.ClearDisplay();
        AmbientAudioSource.Stop();
        AmbientAudioSource.loop = true;
        waitPressAnyKey = false;
    }

    void ExecuteNextSequence() {
        Debug.Log("Execute Sequence " + nextSequence);
        switch (nextSequence) {
            case Sequence.Intro:
                StartCoroutine(ExecuteSequenceIntro(delaySeconds => {
                    nextSequenceIntro++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.Mission:
                StartCoroutine(ExecuteSequenceMission(delaySeconds => {
                    nextSequenceMission++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.Orbit:
                StartCoroutine(ExecuteSequenceOrbit(delaySeconds => {
                    nextSequenceOrbit++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.Interaction:
                StartCoroutine(ExecuteSequenceInteraction(delaySeconds => {
                    nextSequenceInteraction++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.Cognitive:
                StartCoroutine(ExecuteSequenceCognitive(delaySeconds => {
                    nextSequenceCognitive++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.Hole:
                StartCoroutine(ExecuteSequenceHole(delaySeconds => {
                    nextSequenceHole++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.Cube:
                StartCoroutine(ExecuteSequenceCube(delaySeconds => {
                    nextSequenceCube++;
                    StartCoroutine(DelayNextSequence(delaySeconds));
                }));
                break;
            case Sequence.End:
                StartCoroutine(ExecuteSequenceEnd(delaySeconds => {
                    nextSequenceEnd++;
                }));
                break;
            default:
                Debug.Log("Default nextSequence " + nextSequence);
                InitGame();
                nextSequence = Sequence.Intro;
                break;
        }
    }

    IEnumerator ExecuteSequenceIntro(Action<int> done) {
        Debug.Log("Execute nextSequenceIntro " + nextSequenceIntro);
        switch (nextSequenceIntro) {
            case SequenceIntro.Intro:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Intro);
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Intro, introSeconds, introSeconds, commandDialogDone => {
                    done(delayDefault);
                }));
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;            
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceMission(Action<int> done) {
        Debug.Log("Execute nextSequenceMission " + nextSequenceMission);
        switch (nextSequenceMission) {
            case SequenceMission.SpaceNavigation:
                SetSpaceshipZoomBottom();
                StartSpaceshipRotation();
                done(delayDefault);
                break;
            case SequenceMission.SpaceFade:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Mission);
                AmbientAudioSource.Play();
                StartCoroutine(canvasLoader.Fade(CanvasLoader.FadeDirection.Out, 5f, fadeDone => {
                    done(delayNone);
                }));
                break;
            case SequenceMission.Dialog:
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Mission, missionSeconds, missionSeconds, commandDialogDone => {
                    done(delayMissionDialog);
                }));
                break;
            case SequenceMission.DialogFade:
                StartCoroutine(canvasLoader.Fade(CanvasLoader.FadeDirection.In, 0.5f, fadeDone => {
                    done(delayNone);
                }));
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceOrbit(Action<int> done) {
        Debug.Log("Execute nextSequenceOrbit " + nextSequenceOrbit);
        switch (nextSequenceOrbit) {
            case SequenceOrbit.Insertion:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Command);
                ShowHexMap(true);
                StartOrbitInsertion();
                done(delayNone);
                break;
            case SequenceOrbit.InsertionFade:
                StartCoroutine(canvasLoader.Fade(CanvasLoader.FadeDirection.Out, 5f, fadeDone => {
                    done(delayDefault);
                }));
                break;
            case SequenceOrbit.OrderDialog:
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Order, minCommandSeconds, maxCommandSeconds, commandDialogDone => {
                    done(delayDefault);
                }));
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;           
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceInteraction(Action<int> done) {
        Debug.Log("Execute nextSequenceInteraction " + nextSequenceInteraction);
        switch (nextSequenceInteraction) {
            case SequenceInteraction.Dialog:
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Interface, minCommandSeconds, maxCommandSeconds, commandDialogDone => {
                    StartWaitPressAnyKey();
                    done(delayNone);
                }));
                break;
            case SequenceInteraction.StartCommunication:
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Transmission, minCommandSeconds, maxCommandSeconds, commandDialogDone => {
                    done(delayNone);
                }));
                break;
            case SequenceInteraction.Generation:
                hexaoidGenerator.GenerateHexaoid(10, 2);
                done(delayInteractionGeneration);
                break;
            case SequenceInteraction.TransmissionDialog:
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Incoming, minCommandSeconds, maxCommandSeconds, commandDialogDone => {
                    StartWaitPressAnyKey();
                    done(delayNone);
                }));
                break;
            case SequenceInteraction.Decode:
                dialogController.ClearDisplay();
                hexaoidGenerator.SinkHexaoid();
                done(delayInteractionDecode);
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceCognitive(Action<int> done) {
        Debug.Log("Execute nextSequenceCognitive " + nextSequenceCognitive);
        switch (nextSequenceCognitive) {
            case SequenceCognitive.IncomingDialog:
                StartCoroutine(dialogController.CommandDialog(DialogController.CommandType.Decode, minIncomingDialogSeconds, maxIncomingDialogSeconds, commandDialogDone => {
                    done(delayCognitiveIncomingDialog);
                }));
                break;
            case SequenceCognitive.Dialog:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Dialog);
                EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                inputField.OnPointerClick(new PointerEventData(EventSystem.current));
                StartCoroutine(CognitiveDialog(cognitiveDialogDone => {
                    done(minDialogListenSeconds);
                }));
                break;
            case SequenceCognitive.ClearDialog:
                dialogController.ClearDisplay();
                done(minDialogListenSeconds);
                break;
            case SequenceCognitive.StopDialog:
                dialogController.StopCognitiveDialog();
                done(maxDialogListenSeconds);
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceHole(Action<int> done) {
        Debug.Log("Execute nextSequenceHole " + nextSequenceHole);
        switch (nextSequenceHole) {
            case SequenceHole.Creation:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.None);
                hexaoidGenerator.CreateHole();
                done(delayHoleCreation);
                break;
            case SequenceHole.Descent:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.HoleDescent);
                StartCoroutine(HoleDescent(holeDescentDone => {
                    done(delayNone);
                }));
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceCube(Action<int> done) {
        Debug.Log("Execute nextSequenceCube " + nextSequenceCube);
        switch (nextSequenceCube) {
            case SequenceCube.Fade:
                canvasLoader.LoadSceneCanvas(CanvasLoader.Scene.Cube);
                ShowHexMap(false);
                StartCoroutine(canvasLoader.Fade(CanvasLoader.FadeDirection.Out, 0.1f, fadeDone => {
                    done(delayCubeFade);
                }));
                break;
            case SequenceCube.Load:
                StartCoroutine(LoadCube(loadCubeDone => {
                    done(delayNone);
                }));
                break;
            case SequenceCube.Zoom:
                StartCoroutine(ZoomCube(zoomCubeDone => {
                    done(delayNone);
                }));
                break;
            case SequenceCube.Stop:
                StartCoroutine(StopCube(stopCubeDone => {
                    done(delayNone);
                }));
                break;
            case SequenceCube.Destroy:
                DestroyCube();
                AmbientAudioSource.loop = false;
                done(delayCubeDestroy);
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;
        }
        yield return null;
    }

    IEnumerator ExecuteSequenceEnd(Action<int> done) {
        Debug.Log("Execute nextSequenceEnd " + nextSequenceEnd);
        switch (nextSequenceEnd) {
            case SequenceEnd.End:
                mainMenu.ShowEnd();
                done(delayNone);
                break;
            default:
                nextSequence++;
                done(delayNone);
                break;
        }
        yield return null;
    }    

    IEnumerator DelayNextSequence (int delaySeconds) {
        yield return new WaitWhile(() => waitPressAnyKey);        
        yield return new WaitForSeconds(delaySeconds);
        ExecuteNextSequence();
    }

    void ShowHexMap(bool show) {
        hexaoidGenerator.ShowMap(show);
    }

    void CreateHexMap(int size) {
        hexaoidGenerator.GenerateMap(size, size);
        hexaoidGenerator.CreateClimate();
        hexaoidGenerator.SetTerrainType();
        hexaoidGenerator.ShowMap(false);
    }

    void SetSpaceshipZoomMiddle() {
        HexGameCamera.ResetZoomMiddle();
    }

    void SetSpaceshipZoomBottom() {
        HexGameCamera.ResetZoomIn();
    }

    void StartSpaceshipRotation() {
        HexGameCamera.StartRotation();
    }

    void MoveSpaceshipCloser() { 
        HexGameCamera.ZoomIn();        
    }

    void MoveSpaceshipFarther() { 
        HexGameCamera.ZoomOut();        
    }

    void StartOrbitInsertion() {
        SetSpaceshipZoomMiddle();
        StartSpaceshipRotation();
        MoveSpaceshipCloser();
        HexGameCamera.StartOscillation();
    }

    IEnumerator HoleDescent(Action<bool> done) {
        HexGameCamera.StopOscillation();
        MoveSpaceshipFarther();
        StartCoroutine(HexGameCamera.FallDown(fallDownDone => {
            StartCoroutine(canvasLoader.Fade(CanvasLoader.FadeDirection.In, 1f, fadeDone => {
                done(true);
            }));
        }));
        yield return null;
    }

    IEnumerator LoadCube(Action<bool> done) {
        HexGameCamera.StopRotation();
        HexGameCamera.ResetZoomOut();
        hexCubeUnitInstance = Instantiate(hexCubeUnitPrefab);
        StartCoroutine(hexCubeUnitInstance.ShowChilds());
        StartCoroutine(hexCubeUnitInstance.StartRotation(startRotationDone => {
            done(true);
        }));
        yield return null;
    }

    IEnumerator ZoomCube(Action<bool> done) {
        StartCoroutine(HexGameCamera.TranslateOrigin(translateOriginDone => {
            done(true);
        }));
        yield return null;
    }

    IEnumerator StopCube(Action<bool> done) {
        StartCoroutine(hexCubeUnitInstance.StopRotation(stopRotationDone => {
            done(true);
        }));
        yield return null;
    }

    void DestroyCube() {
        Destroy(hexCubeUnitInstance.gameObject);
    }

    IEnumerator CognitiveDialog(Action<bool> done) {
        acceptedInputs = 0;
        autoDialogTextIndex = 0;
        dialogController.StartCognitiveDialog();
        if(autoDialog) {
            StartCoroutine(EditInputField());
        }        
        yield return new WaitWhile(() => !IsDialogEnded());
        done(true);
    }

    void AcceptStringInput(string userInput)
    {
        if (IsDialogEnded()) {
            dialogController.LogInputString (userInput);
        } else {
            dialogController.LogInputString (userInput);
            StartCoroutine(ListenInputString (userInput, minDialogListenSeconds, maxDialogListenSeconds));
        }
        inputField.text = null;
    }

    IEnumerator ListenInputString (string userInput, int minSeconds, int maxSeconds) {
        yield return new WaitForSeconds(UnityEngine.Random.Range(minSeconds, maxSeconds));
        float score = dialogController.ListenInputString (userInput);
        Debug.Log("score " + score);
        int raiseLandIterations = 1;
        int sinkLandIterations = 0;

        if (score >= 2f && score < 3f) {
            raiseLandIterations = 2;
            sinkLandIterations = 1;
        } else if (score >= 3f && score < 4f) {
            raiseLandIterations = 4;
            sinkLandIterations = 2;
        } else if (score >= 4f) {
            raiseLandIterations = 5;
            sinkLandIterations = 2;
        }

        if (score > 0f) {
            hexaoidGenerator.GenerateHexaoid(raiseLandIterations, sinkLandIterations);
            acceptedInputs++;
        }
        
        if(autoDialog && !IsDialogEnded()) {
            StartCoroutine(EditInputField());
        } else {
            inputField.ActivateInputField ();
        }
    }

    bool IsDialogEnded() {
        return acceptedInputs >= maxAcceptedInputs;
    }

    IEnumerator EditInputField() {
        yield return new WaitForSeconds(delayAutoDialog);
        StartCoroutine(TypeInputField(dialogController.autoDialogText[autoDialogTextIndex], minTypeDialogSeconds, maxTypeDialogSeconds, done => {
            StartCoroutine(InputFieldEndEditEvent());
        }));
    }

    public IEnumerator TypeInputField(String inputText, float minSeconds, float maxSeconds, Action<bool> done)
    {
        while (autoDialogtypeIndex <= inputText.Length)
        {
            float seconds = UnityEngine.Random.Range(minSeconds, maxSeconds);
            yield return new WaitForSeconds(seconds);
            inputField.text = inputText.Substring(0, autoDialogtypeIndex);
            inputField.caretPosition = inputField.text.Length;
            autoDialogtypeIndex++;
        }
        autoDialogtypeIndex = 1;
        done(true);
    }

    IEnumerator InputFieldEndEditEvent() {
        yield return new WaitForSeconds(delayAutoDialog);
        inputField.text = "";
        AcceptStringInput(dialogController.autoDialogText[autoDialogTextIndex]);
        autoDialogTextIndex++;
        autoDialogTextIndex = autoDialogTextIndex < dialogController.autoDialogText.Count ? autoDialogTextIndex : 0;
    }
    
    IEnumerator PressAnyKeyEvent() {
        yield return new WaitForSeconds(delayAutoDialog);
        StopWaitPressAnyKey();
    }

    void StartWaitPressAnyKey() {
        waitPressAnyKey = true;
        if(autoDialog) {
            StartCoroutine(PressAnyKeyEvent());
        }
    }

    void StopWaitPressAnyKey() {
        dialogController.ClearDisplay();
        waitPressAnyKey = false;
    }

    void Update()
    {
        if(waitPressAnyKey && Input.anyKeyDown) {
            StopWaitPressAnyKey();
        }

        if(environment == Environment.Demo && Input.GetKeyDown(KeyCode.C)) {
            hexaoidGenerator.GenerateHexaoid(10, 2);
        }
    }
}
