using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndGame : MonoBehaviour {
    public int m_NumberOfCasualties;
    public int m_NumberOfCivilians;

    private AIManager aiManager;
    private GameStatesManager gameStatesManager;
    private StaticData.AvailableGameStates gameState;
    private int m_NumberOfInfected;
    private int m_NumberOfDead;
    private int m_PreviousNumberOfInfected;
    private int m_NumberForcedEnd;
    private bool m_forcedEnd;

    void Start()
    {
        aiManager = GameObject.Find("Scriptsbucket").GetComponent<AIManager>();
        gameStatesManager = GameObject.Find("Scriptsbucket").GetComponent<GameStatesManager>();
        gameStatesManager.MenuGameState.AddListener(OnMenu);
        gameStatesManager.StartingGameState.AddListener(OnStarting);
        gameStatesManager.PlayingGameState.AddListener(OnPlaying);
        gameStatesManager.PausedGameState.AddListener(OnPausing);
        gameStatesManager.EndingGameState.AddListener(OnEnding);
        SetState(gameStatesManager.gameState);
    }

    void Update()
    {
        if (gameState == StaticData.AvailableGameStates.Playing)
        {
            if (m_forcedEnd || aiManager.m_InitialInfectionDone && ((aiManager.m_CountOfInfected - aiManager.m_CountOfDead) == 0 || aiManager.m_CountOfCivilians == 0))
            {
                EndOfGame();
            }
            else if(aiManager.m_CountOfInfected == m_PreviousNumberOfInfected)
            {
                m_NumberForcedEnd += 1;
                if (m_NumberForcedEnd == 500) { m_forcedEnd = true; }  
            }
            else
            {
                m_PreviousNumberOfInfected = aiManager.m_CountOfInfected;
                m_NumberForcedEnd = 0;
            }
        }

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

    private void RequestGameStateChange(StaticData.AvailableGameStates state)
    {
        gameStatesManager.ChangeGameState(state);
    }

    void EndOfGame() {
        m_NumberOfCasualties = Math.Min(aiManager.m_StartingHumans, aiManager.m_CountOfInfected);
        m_NumberOfCivilians = Math.Max(0, aiManager.m_StartingHumans - m_NumberOfCasualties);
        RequestGameStateChange(StaticData.AvailableGameStates.Ending);
    }
}
