using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

	private AudioSource audioS;
	private bool firstPass;

	// Use this for initialization
	void Start () {
		audioS = Camera.main.GetComponent<AudioSource> ();
		firstPass = true;
	}

	public void PlayMusic() {
		if (firstPass) {
			StartCoroutine (StartMusic());
		}
	}

	private IEnumerator StartMusic() {
		firstPass = false;
		yield return new WaitForSeconds (6);
		audioS.Play ();
	}
}
