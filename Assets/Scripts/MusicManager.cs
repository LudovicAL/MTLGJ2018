using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public AudioClip Afraid;
    public AudioClip Moans;
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

    public void SoundManager(GameObject ActiveObject)
    {
        PlayMusic();
        ActiveObject.AddComponent<AudioSource>();
        audioS = ActiveObject.GetComponent<AudioSource>();
        audioS.spatialBlend = 1;
        audioS.rolloffMode = AudioRolloffMode.Linear;
        audioS.minDistance = 9f;
        audioS.maxDistance = 11f;
        audioS.PlayOneShot(Afraid);
    }
}
