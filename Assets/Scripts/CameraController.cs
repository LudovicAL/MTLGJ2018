using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public float cameraSpeed = 15.0f;
	private Vector3 m_PreviousPointerPosition;
	public bool isInCameraMode { get; private set;} // vs WorkerMode
	private Transform cameraTransform;
	private Transform mapTransform;
	private Renderer mapRenderer;

	// Use this for initialization
	void Start () {
		cameraTransform = Camera.main.transform;
		isInCameraMode = true;
		mapTransform = GameObject.Find ("Map").transform;
		mapRenderer = GameObject.Find ("Map").GetComponent<Renderer>();
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
}
