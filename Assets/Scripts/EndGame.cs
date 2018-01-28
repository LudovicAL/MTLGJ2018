using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndGame : MonoBehaviour {

    private GameStatesManager gameStatesManager;
    private StaticData.AvailableGameStates gameState;
    int m_NumberOfCivilians;
    int m_NumberOfInfected;
    int m_NumberOfDead;
    int m_NumberOfEating;

    void Start()
    {
        gameStatesManager = GameObject.Find("Scriptsbucket").GetComponent<GameStatesManager>();
        gameStatesManager.MenuGameState.AddListener(OnMenu);
        gameStatesManager.StartingGameState.AddListener(OnStarting);
        gameStatesManager.PlayingGameState.AddListener(OnPlaying);
        gameStatesManager.PausedGameState.AddListener(OnPausing);
        gameStatesManager.EndingGameState.AddListener(OnEnding);
        SetState(gameStatesManager.gameState);
    }

    protected void OnMenu()
    {
        SetState(StaticData.AvailableGameStates.Menu);
    }

    protected void OnStarting()
    {
        SetState(StaticData.AvailableGameStates.Starting);

    }

    protected void OnPlaying()
    {
        SetState(StaticData.AvailableGameStates.Playing);

    }

    protected void OnPausing()
    {
        SetState(StaticData.AvailableGameStates.Paused);
    }

    private void SetState(StaticData.AvailableGameStates state)
    {
        gameState = state;
    }

    //Use this function to request a game state change from the GameStateManager
    private void RequestGameStateChange(StaticData.AvailableGameStates state)
    {
        gameStatesManager.ChangeGameState(state);
    }
    // Update is called once per frame
    void Update()
    {
        m_NumberOfCivilians = GameObject.FindGameObjectsWithTag("Civilian").Length;
        m_NumberOfInfected = GameObject.FindGameObjectsWithTag("Infected").Length;
        m_NumberOfDead = GameObject.FindGameObjectsWithTag("Dead").Length;
        m_NumberOfEating = GameObject.FindGameObjectsWithTag("Eating").Length;

        if (m_NumberOfCivilians == 0 || m_NumberOfInfected == 0)
        {
            RequestGameStateChange();
        }
    }
}
