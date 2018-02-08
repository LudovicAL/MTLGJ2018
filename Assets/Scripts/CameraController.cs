using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

	public float perspectiveZoomSpeed = 0.005f;	// The rate of change of the field of view in perspective mode.
	public float orthoZoomSpeed = 0.005f;	// The rate of change of the orthographic size in orthographic mode.
	public float cameraSpeed = 15.0f;
	private Vector3 m_PreviousPointerPosition;
	public bool isInCameraMode { get; private set;} // vs WorkerMode
	private Transform cameraTransform;
	private Transform mapTransform;
	private Renderer mapRenderer;
	private Image m_actionStatusBackground;
	private CanvasManager canvasManager;

	// Use this for initialization
	void Start () {
		cameraTransform = Camera.main.transform;
		isInCameraMode = true;
		mapTransform = GameObject.Find ("Map").transform;
		mapRenderer = GameObject.Find ("Map").GetComponent<Renderer>();
		canvasManager = GameObject.Find("Canvas").GetComponent<CanvasManager>();
		m_actionStatusBackground = canvasManager.panelWorker.transform.Find("ActionStatus").GetComponent<Image>();
	}

	public void set_m_PreviousPointerPosition(Vector3 v) {
		m_PreviousPointerPosition = v;
	}

	//Moves the camera horizontally
	public void MoveCameraHorizontally(float direction) {
		Vector2 vecDir = new Vector2 (-direction, 0);
		cameraTransform.Translate(vecDir * Time.deltaTime * cameraSpeed);
	}

	//Moves the camera vertically
	public void MoveCameraVertically(float direction) {
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

	public void ToggleCameraMode() {
		isInCameraMode = !isInCameraMode;
	}

	public bool GetIsInCameraMode() {
		return isInCameraMode;
	}

	public void TouchCameraZoom() {
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
		if (Camera.main.orthographic) {
			// ... change the orthographic size based on the change in distance between the touches.
			Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
			// Make sure the orthographic size never drops below zero.
			Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, .8f, 2f);
		} else {
			// Otherwise change the field of view based on the change in distance between the touches.
			Camera.main.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
			// Clamp the field of view to make sure it's between 0 and 180.
			Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 0.1f, 179.9f);
		}
	}

	public void UpdateActionStatus(bool _isInCameraMode) {
		if (m_actionStatusBackground != null) {
			m_actionStatusBackground.sprite = _isInCameraMode ? canvasManager.spriteFreeWorker : canvasManager.spriteBusyWorker;
		}
	}
}
