using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsManager : MonoBehaviour {
	public bool wallConstructionMode;
	public float cameraSpeed = 10.0f;
	public float zoomSpeed = 3.0f;
	public float updateGap = 0.02f;
	public float fieldOfViewMin = 2.0f;
	public float fieldOfViewMax = 20.0f;
	private float distanceBetweenFingers;
	private LineRenderer hollowLine;
	private float nextUpdateTime;
	private Transform cameraTransform;
	private List<Vector2> coordList;
	private bool buildingWall;
	private int screenWidth;
	private int screenHeight;
	private float startingZoomDistance;
	private float endingZoomDistance;
	private bool zoomingScreen;
	private bool movingScreen;
	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time

	// Use this for initialization
	void Start () {
		Debug.Log ("Multitouch supported: " + Input.multiTouchEnabled);
		distanceBetweenFingers = 0.0f;
		wallConstructionMode = false;
		nextUpdateTime = 0.0f;
		cameraTransform = Camera.main.transform;
		coordList = new List<Vector2> ();
		buildingWall = false;
		movingScreen = false;
		zoomingScreen = false;
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		hollowLine = GameObject.Find("Line").GetComponent<LineRenderer>();
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
		gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		SetState (gameStatesManager.gameState);
	}
	
	// Update is called once per frame
	void Update () {
		if (gameState == StaticData.AvailableGameStates.Playing) {
			if (Application.isMobilePlatform) {	//ON MOBILE
				if (Input.touchCount > 0) {	//USER HAS FINGER(S) ON
					if (Input.touchCount == 1) {	//USER HAS 1 FINGER ON THE SCREEN
						if (wallConstructionMode) {
							TouchWallConstructionController ();
						} else  {
							TouchScreenMoveController ();
						}
					} else if (Input.touchCount == 2) { //USER HAS 2 FINGERS ON THE SCREEN
						TouchScreenZoomController();
					} else {	//USER HAS TOO MANY FINGERS ON THE SCREEN
						WallCanceled();
						MovingScreenCanceled ();
						ScreenZoomCanceled ();
					}
				}
			} else {	//ON PC
				MouseClickController();
				KeyboardButtonController ();
			}
			if (coordList.Count > 100) {
				WallEnded (Input.mousePosition);
			}
		}
	}

	//Decides what to do with 2 fingers touch on the screen
	public void TouchScreenZoomController() {
		if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[1].phase == TouchPhase.Ended) {
			ScreenZoomEnded ();
		} else if (Input.touches[0].phase == TouchPhase.Canceled || Input.touches[1].phase == TouchPhase.Canceled) {
			ScreenZoomCanceled ();
		} else if (Input.touches[0].phase == TouchPhase.Began) {
			distanceBetweenFingers = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
			ScreenZoomBegan ();
		} else if (Input.touches[0].phase != TouchPhase.Stationary || Input.touches[1].phase != TouchPhase.Stationary) {
			if (zoomingScreen)  {
				float newDistance = distanceBetweenFingers = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
				float direction = newDistance - distanceBetweenFingers;
				ScreenZoomMoved (Mathf.Sign(direction));
				distanceBetweenFingers = newDistance;
			}
		}
	}
		
	private void ScreenZoomBegan() {
		zoomingScreen = true;
	}

	private void ScreenZoomMoved(float direction) {
		if (zoomingScreen) {
			Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + direction, fieldOfViewMin, fieldOfViewMax);
		}
	}

	private void ScreenZoomEnded() {
		zoomingScreen = false;
		distanceBetweenFingers = 0.0f;
	}

	private void ScreenZoomCanceled() {
		zoomingScreen = false;
		distanceBetweenFingers = 0.0f;
	}

	//Decides what to do with mouse clicks
	public void MouseClickController() {
		if (Input.GetMouseButton(0)) { //USER IS PRESSING THE MOUSE BUTTON
			if (Input.GetMouseButtonDown(0)) { //USER PRESSED THE MOUSE BUTTON
				WallBegan (Input.mousePosition);
			} else {	//USER JUST KEPT PRESSING THE MOUSE BUTTON
				WallMoved (Input.mousePosition);
			}
		} else if (Input.GetMouseButtonUp(0)) {	//USER RELEASED THE MOUSE BUTTON
			WallEnded (Input.mousePosition);		
		}
	}

	//Decides what to do with keyboard button clicks
	public void KeyboardButtonController() {
		if (Input.GetButton("Horizontal")) {	//USER IS PRESSING A HORIZONTAL ARROW KEY
			MoveCameraHorizontally(Mathf.Sign(Input.GetAxisRaw("Horizontal")));
		}
		if (Input.GetButton("Vertical")) {	//USER IS PRESSING A VERTICAL ARROW KEY
			MoveCameraVertically(Mathf.Sign(Input.GetAxisRaw("Vertical")));
		}
		if (Input.GetButton("Cancel")) {
			RequestGameStateChange(StaticData.AvailableGameStates.Menu);
		}
		if (Input.GetButton("Fire1")) {
			zoomingScreen = true;
			ScreenZoomMoved (1.0f);
			zoomingScreen = false;
		} else if (Input.GetButton("Fire2")) {
			zoomingScreen = true;
			ScreenZoomMoved (-1.0f);
			zoomingScreen = false;
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
				WallBegan (Input.GetTouch(0).position);
				break;
			case TouchPhase.Moved:
				WallMoved (Input.GetTouch(0).position);
				break;
			case TouchPhase.Ended:
				WallEnded (Input.GetTouch(0).position);
				break;
			case TouchPhase.Canceled:
				WallCanceled();
				break;
			default:
				//Do nothing
				break;
		}
	}

	//Moves the camera horizontally
	private void MoveCameraHorizontally(float direction) {
		Vector2 vecDir = new Vector2 (direction, 0);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	//Moves the camera vertically
	private void MoveCameraVertically(float direction) {
		Vector2 vecDir = new Vector2 (0, direction);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	//Player asked to move the screen in the current frame
	private void MovingScreenBegan() {
		movingScreen = true;
	}

	//Player kept asking to move the screen in the current frame
	private void MovingScreenMoved() {
		if (movingScreen) {
			MoveCameraHorizontally (Mathf.Sign (Input.GetTouch(0).deltaPosition.x));
			MoveCameraVertically (Mathf.Sign (Input.GetTouch(0).deltaPosition.y));
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

	//Player began tracing a wall in the current frame
	private void WallBegan(Vector2 coord) {
		buildingWall = true;
		coordList.Add(Camera.main.ScreenToWorldPoint(coord));
		nextUpdateTime = Time.time + updateGap;
		hollowLine.positionCount = 1;
		hollowLine.SetPosition (0, Camera.main.ScreenToWorldPoint (coord));
		hollowLine.enabled = true;
	}

	//Player kept tracing a wall in the current frame
	private void WallMoved(Vector2 coord) {
		if (buildingWall) {
			if (Time.time > nextUpdateTime) {
				Vector3 convertedCoord = Camera.main.ScreenToWorldPoint (coord);
				convertedCoord.z = 0;
				coordList.Add(convertedCoord);
				nextUpdateTime = Time.time + updateGap;
				hollowLine.positionCount = hollowLine.positionCount + 1;
				hollowLine.SetPosition (hollowLine.positionCount - 1, convertedCoord);
			}
		}
	}

	//Player stopped tracing a wall in the current frame
	private void WallEnded(Vector2 coord) {
		if (buildingWall) {
			coordList.Add(Camera.main.ScreenToWorldPoint(coord));
			DrawWall ();
			WallCanceled ();	//This is not actually cancelling the wall as it has been constructed already. It just resets the variables.
		}
	}

	//Player canceled his request to trace a wall in the current frame
	private void WallCanceled() {
		coordList.Clear ();
		nextUpdateTime = 0.0f;
		buildingWall = false;
		wallConstructionMode = false;
		hollowLine.enabled = false;
		hollowLine.positionCount = 0;
	}

	//A wall has to be drawn
	private void DrawWall() {
		GameObject mapReaderObject = GameObject.Find ("Map");
		if (mapReaderObject == null) {
			return;
		}
		mapReaderObject.GetComponent<MapReader> ().AddWall (coordList);
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
		WallCanceled ();
		MovingScreenCanceled ();
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}
}