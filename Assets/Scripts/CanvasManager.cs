using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour {

	public GameObject workerButtonPrefab;
	public int numberOfWorkers = 3;
	//private EndGame eg;
	private GameObject panelGame;
	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time
	private Text textCasualties;
	private Text textSurvivors;
	private Text textRatio;

	// Use this for initialization
	void Start () {
		//eg = GameObject.Find ("Scriptsbucket").GetComponent<EndGame>();
		GameObject.Find ("Button Start").GetComponent<Button> ().onClick.AddListener (StartButtonPress);
		GameObject.Find ("Button Quit").GetComponent<Button> ().onClick.AddListener (QuitButtonPress);
		textCasualties = GameObject.Find ("Text Casualties").GetComponent<Text> ();
		textSurvivors = GameObject.Find ("Text Survivors").GetComponent<Text> ();
		textRatio = GameObject.Find ("Text Ratio").GetComponent<Text> ();
		panelGame = GameObject.Find ("Panel Game");
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		gameStatesManager.PausedGameState.AddListener(OnEnding);
		SetState (gameStatesManager.gameState);
		for (int i = 0; i < numberOfWorkers; i++) {
			int index = i;
			GameObject newButton = GameObject.Instantiate (workerButtonPrefab, panelGame.transform);
			newButton.GetComponent<Button> ().onClick.AddListener (delegate{WorkerButtonPress(index);});
		}
		showPanel ("Panel Menu");
	}
	
	// Update is called once per frame
	void Update () {
		
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
	}

	public void StartButtonPress() {
		RequestGameStateChange(StaticData.AvailableGameStates.Playing);
	}

	public void QuitButtonPress() {
		Application.Quit();
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
		/*
		showPanel ("Panel End");
		textCasualties.text = eg.m_NumberOfCasualties.ToString ();
		textSurvivors.text = m_NumberOfCivilians.ToString ();
		float ratio = m_NumberOfCivilians / (m_NumberOfCasualties + m_NumberOfCivilians);
		textRatio.text = ratio.ToString ("0.0%");
		*/
	}
}
