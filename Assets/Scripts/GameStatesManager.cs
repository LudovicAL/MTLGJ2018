//This script keeps track of the current game-state and warns other scripts when the game-state happens to change.
//Place a unique instance of it somewhere in your game, on a dummy GameObject.

using UnityEngine;
using UnityEngine.Events;

public class GameStatesManager : MonoBehaviour {

	//Declare here any new emitter you may need in addition to the current ones.
	public UnityEvent MenuGameState;
	public UnityEvent StartingGameState;
	public UnityEvent PausedGameState;
	public UnityEvent PlayingGameState;
	//The following variable contains the current GameState.
	public StaticData.AvailableGameStates gameState { get; private set;}

	//Emitters are initialized on Awake()
	void Awake () {
		if (MenuGameState == null) {
			MenuGameState = new UnityEvent();
		}
		if (StartingGameState == null) {
			StartingGameState = new UnityEvent();
		}
		if (PausedGameState == null) {
			PausedGameState = new UnityEvent();
		}
		if (PlayingGameState == null) {
			PlayingGameState = new UnityEvent();
		}
		ChangeGameState(StaticData.AvailableGameStates.Menu);
	}

	//Call this function from anywhere to request a game state change
	public void ChangeGameState(StaticData.AvailableGameStates desiredState) {
		gameState = desiredState;
		switch(desiredState) {
			case StaticData.AvailableGameStates.Menu:
				MenuGameState.Invoke ();
				break;
			case StaticData.AvailableGameStates.Starting:
				StartingGameState.Invoke ();
				break;
			case StaticData.AvailableGameStates.Paused:
				PausedGameState.Invoke ();
				break;
			case StaticData.AvailableGameStates.Playing:
				PlayingGameState.Invoke ();
				break;
		}
	}
}

//This static class makes all the possible game states available to any script.
//You may insert in this enumeration any other game state your game may require.
public static class StaticData {
	public enum AvailableGameStates {
		Menu,	//Consulting the menu
		Starting,	//Game is starting
		Playing,	//Game is playing
		Paused	//Game is paused
	};
}
