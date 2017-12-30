//Here is an example of how to implement game state management on an object that inherit MonoBehaviour.

using UnityEngine;

public class ExampleOfAScriptUsingGameStateManagement : MonoBehaviour {

	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time

	//The scriptBucket and the listeners are initialized on Start()
	void Start () {
		gameStatesManager = GameObject.Find ("NameOfTheObjectContainingTheGameStateManager").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		SetState (gameStatesManager.gameState);
	}
		
	//Here is an example of how to execute stuff only when a specified GameState is on
	void Update () {
		if (gameState == StaticData.AvailableGameStates.Playing) {
			//Do stuff
		}
	}

	//Listener functions a defined for every GameState
	protected void OnMenu() {
		SetState (StaticData.AvailableGameStates.Menu);
	}

	protected void OnStarting() {
		SetState (StaticData.AvailableGameStates.Starting);

	}

	protected void OnPlaying() {
		SetState (StaticData.AvailableGameStates.Playing);

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
