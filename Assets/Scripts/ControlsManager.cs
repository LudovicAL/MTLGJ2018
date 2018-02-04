using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour {
	public float cameraSpeed = 15.0f;
	private Vector3 m_PreviousPointerPosition;
	private Renderer mapRenderer;
	private Transform mapTransform;
	private LineRenderer hollowLine;
	private LineRenderer m_BadLine;
	private Transform cameraTransform;
	private List<Vector2> coordList;
	private bool movingScreen;
	private GameStatesManager gameStatesManager;	//Refers to the GameStateManager
	private StaticData.AvailableGameStates gameState;	//Mimics the GameStateManager's gameState variable at all time
	private bool isInCameraMode; // vs WorkerMode
    private GameObject m_MapReaderObject;
    private CanvasManager canvasManager;
    private Image m_actionStatusBackground;

	// Use this for initialization
	void Start () {
		mapRenderer = GameObject.Find ("Map").GetComponent<Renderer>();
		mapTransform = GameObject.Find ("Map").transform;
		cameraTransform = Camera.main.transform;
		coordList = new List<Vector2> ();
		movingScreen = false;
		isInCameraMode = true;
		hollowLine = GameObject.Find("Line").GetComponent<LineRenderer>();
		m_BadLine = GameObject.Find("BadLine").GetComponent<LineRenderer>();
		gameStatesManager = GameObject.Find ("Scriptsbucket").GetComponent<GameStatesManager>();
        canvasManager = GameObject.Find("Canvas").GetComponent<CanvasManager>();
        gameStatesManager.MenuGameState.AddListener(OnMenu);
		gameStatesManager.StartingGameState.AddListener(OnStarting);
		gameStatesManager.PlayingGameState.AddListener(OnPlaying);
		gameStatesManager.PausedGameState.AddListener(OnPausing);
		gameStatesManager.EndingGameState.AddListener(OnEnding);
		SetState (gameStatesManager.gameState);
		m_MapReaderObject = GameObject.Find ("Map");
        GameObject PW = canvasManager.panelWorker;
        m_actionStatusBackground = PW.transform.Find("ActionStatus").GetComponent<Image>();

    }
	
	// Update is called once per frame
	void Update () {
		if (gameState == StaticData.AvailableGameStates.Playing) {
			//if (Application.isMobilePlatform) {	//ON MOBILE
				if (Input.touchCount > 0) {	//USER HAS FINGER(S) ON

                    //if (!isInCameraMode) {
                    if (Camera.main.orthographicSize <= 1.2f && Input.touchCount == 1)
                    {
                    
                        TouchWallConstructionController ();
					}
                    else if (Input.touchCount == 2)
                    {
                        TouchCameraZoom();
                        if (Camera.main.orthographicSize <= 1.2f)
                        {
                            UpdateActionStatus(false);
                        }
                        else
                        {
                            UpdateActionStatus(true);
                        }
                    }
                    else //if (!EventSystem.current.IsPointerOverGameObject())
                    {
                    TouchScreenMoveController ();
					}
            }
			//} else {	//ON PC
			//	MouseClickController();
			//	KeyboardButtonController ();

			//}
			if (coordList.Count > 100) {
				WallEnded (Input.mousePosition);
			}
		}
	}

	//Decides what to do with mouse clicks
	public void MouseClickController() {
		if (!isInCameraMode) {
			if (Input.GetMouseButton(0)) { //USER IS PRESSING THE MOUSE BUTTON
				if (Input.GetMouseButtonDown(0)) { //USER PRESSED THE MOUSE BUTTON
					WallBegan (Input.mousePosition);
				} else {	//USER JUST KEPT PRESSING THE MOUSE BUTTON
					WallMoved (Input.mousePosition);
				}
			} else if (Input.GetMouseButtonUp(0)) {	//USER RELEASED THE MOUSE BUTTON
				WallEnded (Input.mousePosition);		
			}
		} else {
			if (Input.GetMouseButton(0)) {
				if (!(EventSystem.current.IsPointerOverGameObject())) {
					if (Input.GetMouseButtonDown(0)) {
						m_PreviousPointerPosition = Input.mousePosition;
					}
					MoveScreenMagically(Input.mousePosition);
				}
			}
		}
	}

	//Decides what to do with keyboard button clicks
	public void KeyboardButtonController() {
		if (Input.GetButton("Horizontal") ) {	//USER IS PRESSING A HORIZONTAL ARROW KEY
			MoveCameraHorizontally(-Mathf.Sign(Input.GetAxis("Horizontal")));
		} else if (Input.GetButton("Vertical")) {	//USER IS PRESSING A VERTICAL ARROW KEY
			MoveCameraVertically(-Mathf.Sign(Input.GetAxis("Vertical")));
		}
		if (Input.GetButton("Cancel")) {
            SceneManager.LoadScene(0);
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
		Vector2 vecDir = new Vector2 (-direction, 0);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	//Moves the camera vertically
	private void MoveCameraVertically(float direction) {
		Vector2 vecDir = new Vector2 (0, -direction);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	public void MoveScreenMagically(Vector3 fingerPosition) {
		Vector3 direction = Camera.main.ScreenToWorldPoint (fingerPosition) - Camera.main.ScreenToWorldPoint (m_PreviousPointerPosition);
		direction.z = 0.0f;
		direction = -direction;

		Vector3 newPosition = Camera.main.transform.position + direction;
		newPosition.z = mapTransform.position.z;
		if (mapRenderer.bounds.Contains(newPosition)) {
			Camera.main.transform.position += direction;
		}
		m_PreviousPointerPosition = fingerPosition;

	}

	//Player asked to move the screen in the current frame
	private void MovingScreenBegan() {
		m_PreviousPointerPosition = Input.GetTouch (0).position;
		movingScreen = true;
	}

	//Player kept asking to move the screen in the current frame
	private void MovingScreenMoved() {
		if (movingScreen) {
			MoveScreenMagically (Input.GetTouch (0).position);
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

	private bool m_WallStarted = false;
	private bool m_FoundStartingPoint = false;
	private bool m_WallBuilding = false;
	private bool m_WallAlmostFinished = false;
	private bool m_WallFinished = false;
	private Vector3 m_PreviousCoord;

	//Player began tracing a wall in the current frame
	private void WallBegan(Vector2 coord) {
		m_WallStarted = true;
		m_FoundStartingPoint = false;
		m_WallBuilding = false;
		m_WallAlmostFinished = false;
		m_WallFinished = false;
		m_BadLine.positionCount = 0;
		m_BadLine.enabled = true;

		Vector3 convertedCoord = Camera.main.ScreenToWorldPoint (coord);
		if (!m_MapReaderObject.GetComponent<MapReader> ().CanMoveThere (convertedCoord [0], convertedCoord [1])) 
		{
			m_PreviousCoord = convertedCoord;
			m_PreviousCoord.z = 0;
			m_FoundStartingPoint = true;

			++m_BadLine.positionCount;
			m_BadLine.SetPosition (m_BadLine.positionCount - 1, convertedCoord);
		}
	}


	private void TryAddWallCoordinate(Vector2 coord)
	{
		if (m_WallFinished)
			return;
		
		Vector3 convertedCoord = Camera.main.ScreenToWorldPoint (coord);
		convertedCoord.z = 0;

		if (!m_MapReaderObject.GetComponent<MapReader> ().CanMoveThere (convertedCoord [0], convertedCoord [1])) 
		{
			if (!m_FoundStartingPoint) 
			{
				m_FoundStartingPoint = true;
			}

			if (m_WallBuilding) 
			{
				m_WallAlmostFinished = true;
			} 
			else 
			{
				m_PreviousCoord = convertedCoord;
			}

			if (m_WallAlmostFinished) 
			{
				m_WallFinished = true;
			}
		}
		else
		{
			if (m_FoundStartingPoint) 
			{
				if (!m_WallBuilding) 
				{
					hollowLine.positionCount = 0;
					hollowLine.enabled = true;

					m_WallBuilding = true;
					coordList.Add(m_PreviousCoord);
					++hollowLine.positionCount;
					hollowLine.SetPosition (hollowLine.positionCount - 1, m_PreviousCoord);
				} 
			}
		}
			

		if (m_WallBuilding) {
			coordList.Add (convertedCoord);
			//nextUpdateTime = Time.time + updateGap;
			++hollowLine.positionCount;
			hollowLine.SetPosition (hollowLine.positionCount - 1, convertedCoord);
		} 
		else 
		{
			++m_BadLine.positionCount;
			m_BadLine.SetPosition (m_BadLine.positionCount - 1, convertedCoord);
		}
	}

	//Player kept tracing a wall in the current frame
	private void WallMoved(Vector2 coord) {
		if (m_WallStarted && !m_WallFinished) 
		{
			if (!m_WallBuilding) 
			{
				TryAddWallCoordinate (coord); 
			} 
			else 
			{
				//if (Time.time > nextUpdateTime) 
				//{
					TryAddWallCoordinate (coord); 
				//}
			}
		}
	}

	//Player stopped tracing a wall in the current frame
	private void WallEnded(Vector2 coord) {
		//if (buildingWall) {
			DrawWall ();
			WallCanceled ();	//This is not actually cancelling the wall as it has been constructed already. It just resets the variables.
		//}
	}

	//Player canceled his request to trace a wall in the current frame
	private void WallCanceled() {
		coordList.Clear ();
		hollowLine.enabled = false;
		hollowLine.positionCount = 0;
		m_BadLine.enabled = false;
		m_BadLine.positionCount = 0;
	}

	//A wall has to be drawn
	private void DrawWall() {
		
		if (m_MapReaderObject == null) {
			return;
		}
		m_MapReaderObject.GetComponent<MapReader> ().AddWall (coordList);
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
		WallCanceled ();
		MovingScreenCanceled ();
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}

	public void ToggleCameraMode()
	{
		isInCameraMode = !isInCameraMode;
	}

	public bool GetIsInCameraMode()
	{
		return isInCameraMode;
	}

    private float perspectiveZoomSpeed = 0.005f;        // The rate of change of the field of view in perspective mode.
    private float orthoZoomSpeed = 0.005f;        // The rate of change of the orthographic size in orthographic mode.

    private void TouchCameraZoom()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        // Find the position in the previous frame of each touch.
        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        // If the camera is orthographic...
        if (Camera.main.orthographic)
        {
            // ... change the orthographic size based on the change in distance between the touches.
            Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

            // Make sure the orthographic size never drops below zero.
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, .8f, 2f);
        }
        else
        {
            // Otherwise change the field of view based on the change in distance between the touches.
            Camera.main.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;

            // Clamp the field of view to make sure it's between 0 and 180.
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 0.1f, 179.9f);
        }
    }

    void UpdateActionStatus(bool _isInCameraMode)
    {
        if (m_actionStatusBackground != null)
        {
            m_actionStatusBackground.sprite = _isInCameraMode ? canvasManager.spriteFreeWorker  : canvasManager.spriteBusyWorker;
        }
    }
}