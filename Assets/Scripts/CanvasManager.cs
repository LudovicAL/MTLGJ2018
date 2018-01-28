using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour {

	public GameObject workerButtonPrefab;
	public int numberOfWorkers = 1;
	public Sprite spriteBusyWorker;
	public Sprite spriteFreeWorker;
	private EndGame eg;
	private GameObject panelGame;
	private GameObject scriptsBucketObject;
	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time
	private Text textCasualties;
	private Text textSurvivors;
	private Text textRatio;
	private List<GameObject> workerButtons;
    private float m_SuccessRatioNeeded = 0.3f;

    // Use this for initialization
    void Start () {
		eg = GameObject.Find ("Scriptsbucket").GetComponent<EndGame>();
		GameObject.Find ("Button Start").GetComponent<Button> ().onClick.AddListener (StartButtonPress);
		GameObject.Find ("Button Quit").GetComponent<Button> ().onClick.AddListener (QuitButtonPress);
        GameObject.Find("Button Abandon").GetComponent<Button>().onClick.AddListener(QuitButtonPress);
        GameObject.Find("Button Continue").GetComponent<Button>().onClick.AddListener(ContinueButtonPress);
        //GameObject.Find ("Button Menu").GetComponent<Button> ().onClick.AddListener (MenuButtonPress);
        textCasualties = GameObject.Find ("Text Casualties").GetComponent<Text> ();
		textSurvivors = GameObject.Find ("Text Survivors").GetComponent<Text> ();
		textRatio = GameObject.Find ("Text Ratio").GetComponent<Text> ();
		panelGame = GameObject.Find ("Panel Game");
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		gameStatesManager.EndingGameState.AddListener(OnEnding);
		SetState (gameStatesManager.gameState);
		workerButtons = new List<GameObject> ();
		for (int i = 0; i < numberOfWorkers; i++) {
			int index = i;
			GameObject newButton = GameObject.Instantiate (workerButtonPrefab, panelGame.transform);
			newButton.GetComponent<Button> ().onClick.AddListener (delegate{WorkerButtonPress(index);});
			workerButtons.Add (newButton);
		}
		showPanel ("Panel Menu");
		scriptsBucketObject = GameObject.Find ("Scriptsbucket");
		UpdateWorkerButtons (true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void MenuButtonPress() {
		Application.LoadLevel(Application.loadedLevel);
	}

	private void showPanel(string panelName) {
		for (int i = 0, max = this.transform.childCount; i < max; i++) {
			if (this.transform.GetChild (i).gameObject.name == panelName || this.transform.GetChild (i).gameObject.name == "EventSystem") {
				this.transform.GetChild (i).gameObject.SetActive (true);
			} else {
				this.transform.GetChild (i).gameObject.SetActive (false);
			}
		}
	}

	public void WorkerButtonPress(int buttonNo) {
		Debug.Log ("Pressed: " + buttonNo);
		ControlsManager controlsManager = scriptsBucketObject != null ? scriptsBucketObject.GetComponent<ControlsManager> () : null;
		if (controlsManager != null) {
			controlsManager.ToggleCameraMode ();
			bool newIsInCameraMode = controlsManager.GetIsInCameraMode ();

			// TODO: if we ever have more than one button, change only the one that gets changed
			UpdateWorkerButtons(newIsInCameraMode);
		}
	}

	void UpdateWorkerButtons(bool _isInCameraMode)
	{
		foreach (GameObject go in workerButtons) {
			Transform backgroundImage = go.transform.Find ("BackgroundImage");
			if (backgroundImage != null) {
				Color updatedColor = _isInCameraMode ? Color.green : Color.yellow;
				updatedColor.a = 0.78f; // alpha
				backgroundImage.GetComponent<Image> ().color = updatedColor;
			}
			Transform workerImage = go.transform.Find ("Image Worker");
			if (workerImage != null) {
				workerImage.GetComponent<Image> ().sprite = _isInCameraMode ? spriteFreeWorker : spriteBusyWorker;
			}
		}
	}

	public void StartButtonPress() {
		RequestGameStateChange(StaticData.AvailableGameStates.Playing);
	}

	public void QuitButtonPress() {
		Application.Quit();
	}

    public void ContinueButtonPress()
    {
		//GameObject.Find ("Scriptsbucket").GetComponent<UserprefsManager> ().IncrementDifficulty ();
        Application.LoadLevel(Application.loadedLevel);
    }

    //Listener functions a defined for every GameState
    protected void OnMenu() {
		SetState (StaticData.AvailableGameStates.Menu);
		showPanel ("Panel Menu");
	}

	protected void OnStarting() {
		SetState (StaticData.AvailableGameStates.Starting);

	}

	protected void OnPlaying() {
		SetState (StaticData.AvailableGameStates.Playing);
		showPanel ("Panel Game");
	}

	protected void OnPausing() {
		SetState (StaticData.AvailableGameStates.Paused);
	}

	protected void OnEnding() {
		SetState (StaticData.AvailableGameStates.Ending);
		ShowEndScreen ();
	}

	private void SetState(StaticData.AvailableGameStates state) {
		gameState = state;
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}

	public void ShowEndScreen() {
		
		showPanel ("Panel End");
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(0, 0);
        camera.orthographicSize = 3f;

        textCasualties.text = eg.m_NumberOfCasualties.ToString ();
		textSurvivors.text = eg.m_NumberOfCivilians.ToString ();
		float ratio = (float)eg.m_NumberOfCivilians / ((float)eg.m_NumberOfCasualties + (float)eg.m_NumberOfCivilians);
		textRatio.text =  ratio.ToString("0.0%");

        if (ratio >= m_SuccessRatioNeeded)
        {
            GameObject.Find("WhatsNext").GetComponent<Text>().text = "Continue";
        }
        else
        {
            GameObject.Find("WhatsNext").GetComponent<Text>().text = "Restart";
        }
    }
}
