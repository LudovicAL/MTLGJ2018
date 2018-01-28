using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndGame : MonoBehaviour {

    private ColorChange colorChange;
    private GameStatesManager gameStatesManager;
    private StaticData.AvailableGameStates gameState;
    public int m_NumberOfCasualties;
    public int m_NumberOfCivilians;
    int m_NumberOfInfected;
    int m_NumberOfDead;
    int m_NumberOfEating;

    void Start()
    {
        colorChange = GameObject.Find("Scriptsbucket").GetComponent<ColorChange>();
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

    protected void OnEnding()
    {
        SetState(StaticData.AvailableGameStates.Ending);
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

        if (colorChange.CountOfInfected == 0 || colorChange.CountOfInfected == colorChange.StartingCivilians)
        {
            EndOfGame();
        }
    }

    void EndOfGame()
    {
        m_NumberOfDead = GameObject.FindGameObjectsWithTag("Dead").Length;
        m_NumberOfCivilians = GameObject.FindGameObjectsWithTag("Civilian").Length;
        m_NumberOfInfected = GameObject.FindGameObjectsWithTag("Infected").Length;
        m_NumberOfCasualties = m_NumberOfInfected + m_NumberOfDead;
        RequestGameStateChange(StaticData.AvailableGameStates.Ending);
    }
}
