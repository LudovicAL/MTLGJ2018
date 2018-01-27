using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour {
	public float movementSpeed = 0.06f;
	MapReader g_MapReader;

	// Use this for initialization
	void Start () {
		g_MapReader = GameObject.Find ("MapReader").GetComponent<MapReader> ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 movement = new Vector3 (0.0f, 0.0f, 0.0f);
		if (Input.GetKey (KeyCode.LeftArrow)) {
			movement += new Vector3 (-1.0f, 0.0f, 0.0f);
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			movement += new Vector3 (1.0f, 0.0f, 0.0f);
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			movement += new Vector3 (0.0f, 1.0f, 0.0f);
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			movement += new Vector3 (0.0f, -1.0f, 0.0f);
		}

		if (movement == Vector3.zero)
			return;

		Vector3 wantedPos = transform.position + movement * movementSpeed;
		if (g_MapReader.CanMoveThere(wantedPos[0], wantedPos[1]))
			transform.position = wantedPos;
	}
}
