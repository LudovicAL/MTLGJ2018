using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyParameters : MonoBehaviour {

	public bool m_IsInitialized = false;
	public int m_StartingHumans = 3000;
	public float InfectedBaseSpeed = .5f;
	public float InfectedSpeedPlusMinus = .2f;
	public float CivilianBaseSpeed = .1f;
	public float CivilianSpeedPlusMinus = .05f;
	public float m_ZombieConversionRange = 0.05f;
	public int m_NumberOfZombies = 2;

	//Makes the initial spread faster
	public float m_StartingZombieBoost = 2.0f;
	public float m_CurrentZombieBoost = 0.0f;
	public float m_ZombieBoostDegradeAmountPerNewZombie = 0.05f;

	public float m_BaseEatingSpeed = .05f;

	public float m_MaxTerrorIndex = 4.0f;
	public float m_TerrorIndexIncreasePerZombie = 0.25f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}