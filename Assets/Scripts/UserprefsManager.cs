using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserprefsManager : MonoBehaviour {
	private ColorChange cc;

	// Use this for initialization
	void Awake () {
		cc = GameObject.Find ("Scriptsbucket").GetComponent<ColorChange> ();
		cc.m_DifficultyCurrentLevel = PlayerPrefs.GetInt ("Difficulty");
		PlayerPrefs.SetInt ("CurrentGameDifficulty", PlayerPrefs.GetInt("Difficulty"));
		PlayerPrefs.SetInt ("Difficulty", 0);
	}

	public void IncrementDifficulty() {
		int difficulty = Mathf.Clamp (PlayerPrefs.GetInt ("CurrentGameDifficulty") + 1, 0, cc.m_DifficultyParameters.Count - 1);
		PlayerPrefs.SetInt("Difficulty", difficulty);
	}
}
