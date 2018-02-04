using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserprefsManager : MonoBehaviour {

    void Awake()
    {
        PlayerPrefs.SetInt("CurrentDifficulty", PlayerPrefs.GetInt("Difficulty"));
        PlayerPrefs.SetInt("CurrentLevel", PlayerPrefs.GetInt("Level"));
        PlayerPrefs.SetInt("Level", 0);

    }

    public void IncrementDifficulty()
    {
        int level = Mathf.Clamp(PlayerPrefs.GetInt("CurrentLevel") + 1, 0,20);
        PlayerPrefs.SetInt("Level", level);
    }
}
