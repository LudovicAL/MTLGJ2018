using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyParameters : MonoBehaviour {

	[HideInInspector] public bool m_IsInitialized = false;
	[HideInInspector] public int m_StartingHumans = 250;
	[HideInInspector] public int m_NumberOfZombies = 1;
	[HideInInspector] public float m_StartingZombieBoost = 2.0f;
    public float InfectedBaseSpeed = .5f;
    public float InfectedSpeedPlusMinus = .2f;
    public float CivilianBaseSpeed = .1f;
    public float CivilianSpeedPlusMinus = .05f;
    public float m_ZombieConversionRange = 0.05f;

    //Makes the initial spread faster
    public float m_CurrentZombieBoost = 0.0f;
    public float m_ZombieBoostDegradeAmountPerNewZombie = 0.05f;

    public float m_BaseEatingSpeed = .05f;

    public float m_MaxTerrorIndex = 4.0f;
    public float m_TerrorIndexIncreasePerZombie = 0.25f;

	public void AdjustParameters(int sliderValue) {
		m_StartingHumans = Mathf.Min(250 * (int)(Mathf.Pow(sliderValue, 2)) + (250 * PlayerPrefs.GetInt("CurrentLevel", 0)), 3000); ;
		m_NumberOfZombies = Mathf.Min(1 * sliderValue + (int)Mathf.Round(.3f * PlayerPrefs.GetInt("CurrentLevel", 0)), 10);
		m_StartingZombieBoost = .5f * (float)sliderValue;
    }
}