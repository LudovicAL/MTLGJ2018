using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PCControls : MonoBehaviour {

	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time
	private WallConstructor wallConstructor;
	private CameraController cameraController;

	// Use this for initialization
	void Start () {
		if (Application.isMobilePlatform) {
			this.enabled = false;
		}
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		gameStatesManager.EndingGameState.AddListener(OnEnding);
		wallConstructor = GameObject.Find ("Scriptsbucket").GetComponent<WallConstructor>();
		SetState (gameStatesManager.gameState);
		cameraController = GameObject.Find ("Scriptsbucket").GetComponent<CameraController>();
	}
	
	// Update is called once per frame
	void Update () {
		if (gameState == StaticData.AvailableGameStates.Playing) {
			MouseClickController();
			KeyboardButtonController ();
		}
	}

	//Decides what to do with mouse clicks
	public void MouseClickController() {
		if (!cameraController.isInCameraMode) {
			if (Input.GetMouseButton(0)) { //USER IS PRESSING THE MOUSE BUTTON
				if (Input.GetMouseButtonDown(0)) { //USER PRESSED THE MOUSE BUTTON
					wallConstructor.WallBegan (Input.mousePosition);
				} else {	//USER JUST KEPT PRESSING THE MOUSE BUTTON
					wallConstructor.WallMoved (Input.mousePosition);
				}
			} else if (Input.GetMouseButtonUp(0)) {	//USER RELEASED THE MOUSE BUTTON
				wallConstructor.WallEnded (Input.mousePosition);		
			}
		} else {
			if (Input.GetMouseButton(0)) {
				if (!(EventSystem.current.IsPointerOverGameObject())) {
					if (Input.GetMouseButtonDown(0)) {
						cameraController.set_m_PreviousPointerPosition(Input.mousePosition);
					}
					cameraController.MoveScreenMagically(Input.mousePosition);
				}
			}
		}
	}

	//Decides what to do with keyboard button clicks
	public void KeyboardButtonController() {
		if (Input.GetButton("Horizontal") ) {	//USER IS PRESSING A HORIZONTAL ARROW KEY
			cameraController.MoveCameraHorizontally(-Mathf.Sign(Input.GetAxis("Horizontal")));
		} else if (Input.GetButton("Vertical")) {	//USER IS PRESSING A VERTICAL ARROW KEY
			cameraController.MoveCameraVertically(-Mathf.Sign(Input.GetAxis("Vertical")));
		}
		if (Input.GetButton("Cancel")) {
			SceneManager.LoadScene(0);
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

	protected void OnEnding() {
		SetState (StaticData.AvailableGameStates.Ending);
	}

	private void SetState(StaticData.AvailableGameStates state) {
		gameState = state;
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}
}