using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsManager : MonoBehaviour {
	public float cameraSpeed = 10.0f;
	public float updateGap = 0.02f;
	private LineRenderer hollowLine;
	private float nextUpdateTime;
	private Transform cameraTransform;
	private List<Vector2> coordList;
	private bool buildingWall;
	private int screenWidth;
	private int screenHeight;
	private float startingZoomDistance;
	private float endingZoomDistance;
	private bool movingScreen;
	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time

	// Use this for initialization
	void Start () {
		nextUpdateTime = 0.0f;
		cameraTransform = Camera.main.transform;
		coordList = new List<Vector2> ();
		buildingWall = false;
		movingScreen = false;
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
					if (Input.touchCount == 1) {	//USER HAS 1 FINGER ON
						switch (Input.GetTouch(0).phase) {
							case TouchPhase.Began:
								wallBegan (Input.GetTouch(0).position);
								break;
							case TouchPhase.Moved:
								wallMoved (Input.GetTouch(0).position);
								break;
							case TouchPhase.Ended:
								wallEnded (Input.GetTouch(0).position);
								break;
							case TouchPhase.Canceled:
								wallCanceled();
								break;
							default:
								//Do nothing
								break;
						}
					} else if (Input.touchCount == 2) {	//USER HAS 2 FINGERS ON
						if (buildingWall) {
							wallCanceled();
						}
						foreach (Touch tch in Input.touches) {
							switch (tch.phase) {
								case TouchPhase.Began:
									movingScreenBegan ();
									break;
								case TouchPhase.Moved:
									movingScreenMoved ();
									break;
								case TouchPhase.Ended:
									movingScreenEnded ();
									break;
								case TouchPhase.Canceled:
									movingScreenCanceled();
									break;
								default:
									//Do nothing
									break;
							}
						}
					} else {	//USER HAS TOO MANY FINGERS ON
						if (buildingWall) {
							wallCanceled();
						}
						if (movingScreen) {
							movingScreenCanceled();
						}
					}
				}
			} else {	//ON PC
				if (Input.GetMouseButton(0)) { //USER IS PRESSING THE MOUSE BUTTON
					if (Input.GetMouseButtonDown(0)) { //USER PRESSED THE MOUSE BUTTON
						wallBegan (Input.mousePosition);
					} else {	//USER JUST KEPT PRESSING THE MOUSE BUTTON
						wallMoved (Input.mousePosition);
					}
				} else if (Input.GetMouseButtonUp(0)) {	//USER RELEASED THE MOUSE BUTTON
					wallEnded (Input.mousePosition);		
				}
				if (Input.GetButton("Horizontal")) {	//USER IS PRESSING A HORIZONTAL ARROW KEY
					moveCameraHorizontally(Mathf.Sign(Input.GetAxisRaw("Horizontal")));
				}
				if (Input.GetButton("Vertical")) {	//USER IS PRESSING A VERTICAL ARROW KEY
					moveCameraVertically(Mathf.Sign(Input.GetAxisRaw("Vertical")));
				}
			}
			if (coordList.Count > 100) {
				wallEnded (Input.mousePosition);
			}
		}
	}

	private void moveCameraHorizontally(float direction) {
		Vector2 vecDir = new Vector2 (direction, 0);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	private void moveCameraVertically(float direction) {
		Vector2 vecDir = new Vector2 (0, direction);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	private void movingScreenBegan() {
		movingScreen = true;
	}

	private void movingScreenMoved() {
		Vector2 movement = Input.GetTouch(0).deltaPosition + Input.GetTouch(1).deltaPosition;
		moveCameraHorizontally (Mathf.Sign (movement.x));
		moveCameraVertically (Mathf.Sign (movement.y));
	}

	private void movingScreenEnded() {
		movingScreen = false;
	}

	private void movingScreenCanceled() {
		movingScreen = false;
	}

	private void wallBegan(Vector2 coord) {
		buildingWall = true;
		coordList.Add(Camera.main.ScreenToWorldPoint(coord));
		nextUpdateTime = Time.time + updateGap;
		hollowLine.positionCount = 1;
		hollowLine.SetPosition (0, Camera.main.ScreenToWorldPoint (coord));
		hollowLine.enabled = true;
	}

	private void wallMoved(Vector2 coord) {
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

	private void wallEnded(Vector2 coord) {
		if (buildingWall) {
			coordList.Add(Camera.main.ScreenToWorldPoint(coord));
			DrawWall ();
			coordList.Clear ();
			nextUpdateTime = 0.0f;
			buildingWall = false;
			hollowLine.enabled = false;
			hollowLine.positionCount = 0;
		}
	}

	private void wallCanceled() {
		coordList.Clear ();
		nextUpdateTime = 0.0f;
		buildingWall = false;
		hollowLine.enabled = false;
		hollowLine.positionCount = 0;
	}
	
	private void DrawWall() {

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
		wallCanceled ();
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}
}