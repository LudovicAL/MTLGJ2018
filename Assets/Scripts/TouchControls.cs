using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchControls : MonoBehaviour {

	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time
	private CameraController cameraController;
	private WallConstructor wallConstructor;
	private bool movingScreen;

	// Use this for initialization
	void Start () {
		if (!Application.isMobilePlatform) {
			this.enabled = false;
		}
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		gameStatesManager.EndingGameState.AddListener(OnEnding);
		SetState (gameStatesManager.gameState);
		cameraController = GameObject.Find ("Scriptsbucket").GetComponent<CameraController>();
		wallConstructor = GameObject.Find ("Scriptsbucket").GetComponent<WallConstructor>();
		movingScreen = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (gameState == StaticData.AvailableGameStates.Playing) {
			if (Input.touchCount > 0) {	//USER HAS FINGER(S) ON
				/*
				if (!cameraController.isInCameraMode) {
					TouchWallConstructionController ();
				} else {
					if (!EventSystem.current.IsPointerOverGameObject()) {
						TouchScreenMoveController ();
					}
				}
				*/
				if (Camera.main.orthographicSize <= 1.2f && Input.touchCount == 1) {
					TouchWallConstructionController();
				} else if (Input.touchCount == 2) {
					cameraController.TouchCameraZoom();
					if (Camera.main.orthographicSize <= 1.2f) {
						cameraController.UpdateActionStatus(false);
					} else {
						cameraController.UpdateActionStatus(true);
					}
				}
				else //if (!EventSystem.current.IsPointerOverGameObject())
				{
					TouchScreenMoveController();
				}
			}
			if (wallConstructor.getCoordList().Count > 100) {
				wallConstructor.WallEnded (Input.GetTouch(0).position);
			}
		}
	}

	//Decides what to do with 1 finger touch on the screen of a mobile device when in screen-move-mode
	public void TouchScreenMoveController() {
		switch (Input.GetTouch(0).phase) {
			case TouchPhase.Began:
				MovingScreenBegan ();
				break;
			case TouchPhase.Moved:
				MovingScreenMoved ();
				break;
			case TouchPhase.Ended:
				MovingScreenEnded ();
				break;
			case TouchPhase.Canceled:
				MovingScreenCanceled ();
				break;
			default:
				//Do nothing
				break;
		}
	}

	//Decides what to do with 1 finger touch on the screen of a mobile device when in wall-construction-mode
	public void TouchWallConstructionController() {
		switch (Input.GetTouch(0).phase) {
			case TouchPhase.Began:
				wallConstructor.WallBegan (Input.GetTouch(0).position);
				break;
			case TouchPhase.Moved:
				wallConstructor.WallMoved (Input.GetTouch(0).position);
				break;
			case TouchPhase.Ended:
				wallConstructor.WallEnded (Input.GetTouch(0).position);
				break;
			case TouchPhase.Canceled:
				wallConstructor.WallCanceled();
				break;
			default:
				//Do nothing
				break;
		}
	}

	//Player asked to move the screen in the current frame
	private void MovingScreenBegan() {
		cameraController.set_m_PreviousPointerPosition(Input.GetTouch (0).position);
		movingScreen = true;
	}

	//Player kept asking to move the screen in the current frame
	private void MovingScreenMoved() {
		if (movingScreen) {
			cameraController.MoveScreenMagically (Input.GetTouch (0).position);
		}
	}

	//Player stopped asking to move the screen in the current frame
	private void MovingScreenEnded() {
		movingScreen = false;
	}

	//Player canceled his request to move the screen in the current frame
	private void MovingScreenCanceled() {
		movingScreen = false;
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
