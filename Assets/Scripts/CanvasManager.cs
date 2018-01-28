using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour {

	public GameObject workerButtonPrefab;
	public int numberOfWorkers = 3;
	private GameObject panelMenu;
	private GameObject panelGame;
	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time

	// Use this for initialization
	void Start () {
		GameObject.Find ("Button Start").GetComponent<Button> ().onClick.AddListener (StartButtonPress);
		GameObject.Find ("Button Quit").GetComponent<Button> ().onClick.AddListener (QuitButtonPress);
		panelMenu = GameObject.Find ("Panel Menu");
		panelGame = GameObject.Find ("Panel Game");
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		SetState (gameStatesManager.gameState);
		for (int i = 0; i < numberOfWorkers; i++) {
			int index = i;
			GameObject newButton = GameObject.Instantiate (workerButtonPrefab, panelGame.transform);
			newButton.GetComponent<Button> ().onClick.AddListener (delegate{WorkerButtonPress(index);});
		}
		panelGame.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
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
		panelMenu.SetActive (true);
		panelGame.SetActive (false);
	}

	protected void OnStarting() {
		SetState (StaticData.AvailableGameStates.Starting);

	}

	protected void OnPlaying() {
		SetState (StaticData.AvailableGameStates.Playing);
		panelMenu.SetActive (false);
		panelGame.SetActive (true);
	}

	protected void OnPausing() {
		SetState (StaticData.AvailableGameStates.Paused);
	}

	private void SetState(StaticData.AvailableGameStates state) {
		gameState = state;
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}
}
