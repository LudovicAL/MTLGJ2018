using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyParameters : MonoBehaviour
{

    public bool m_IsInitialized = false;
    public int m_StartingHumans = 250;
    public float InfectedBaseSpeed = .5f;
    public float InfectedSpeedPlusMinus = .2f;
    public float CivilianBaseSpeed = .1f;
    public float CivilianSpeedPlusMinus = .05f;
    public float m_ZombieConversionRange = 0.05f;
    public int m_NumberOfZombies = 1;

    //Makes the initial spread faster
    public float m_StartingZombieBoost = 2.0f;
    public float m_CurrentZombieBoost = 0.0f;
    public float m_ZombieBoostDegradeAmountPerNewZombie = 0.05f;

    public float m_BaseEatingSpeed = .05f;

    public float m_MaxTerrorIndex = 4.0f;
    public float m_TerrorIndexIncreasePerZombie = 0.25f;


    public int m_DifficultyCurrentLevel;

    private Slider SliderDiff;

    void Awake()
    {
        SliderDiff = GameObject.Find("Slider Difficulty").GetComponent<Slider>();
        SliderDiff.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    // Use this for initialization
    void Start()
    {

        m_DifficultyCurrentLevel = PlayerPrefs.GetInt("CurrentDifficulty", 0);
        AdjustParameters();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnValueChanged()
    {
        m_DifficultyCurrentLevel = (int)SliderDiff.value;
        PlayerPrefs.SetInt("Difficulty", m_DifficultyCurrentLevel);
        PlayerPrefs.SetInt("CurrentDifficulty", m_DifficultyCurrentLevel);
        AdjustParameters();

    }

    void AdjustParameters()
    {
        m_StartingHumans = Mathf.Min(250 * (int)(Mathf.Pow(m_DifficultyCurrentLevel, 2)) + (250 * PlayerPrefs.GetInt("CurrentLevel", 0)), 3000); ;
        m_NumberOfZombies = Mathf.Min(1 * m_DifficultyCurrentLevel + (int)Mathf.Round(.3f * PlayerPrefs.GetInt("CurrentLevel", 0)), 10);
        m_StartingZombieBoost = .5f * m_DifficultyCurrentLevel;
    }
}