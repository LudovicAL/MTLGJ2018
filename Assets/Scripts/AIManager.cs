using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class AIManager : MonoBehaviour
{
    //public List<DifficultyParameters> m_DifficultyParameters;
    public Transform Civilian;
    //public int m_DifficultyCurrentLevel;
    public int m_CountOfInfected;
    public int m_CountOfDead;
    public int m_CountOfCivilians;
    public bool m_InitialInfectionDone;
    public int m_StartingHumans;

    private GameStatesManager gameStatesManager;
    private StaticData.AvailableGameStates gameState;
    private MusicManager musicM;
    private MapReader g_MapReader;
    private Text m_InfectedCountText;
	private CiviliansSpawner civiliansSpawner;

    //Grid
    private Vector3 m_GridOffset;
    private List<List<GameObject>> GameObjectGridList = new List<List<GameObject>>();
    private float m_GridCellWidthHeight = 0.5f;
    private int m_GridWidth = 18;
    private int m_GridHeight = 14;

    //Zombies
    private GameObject[] m_InfectedTargets;
    private float m_InfectedActiveRed = 0.5f;
    private float m_TimeBeforeSpawn;
    private int m_MaxNumberOfInfectedToUpdateEachFrame = 100;
    private int m_InfectedIndex = 0;
    private int m_InfectedUpdatedThisFrame = 0;
    private int m_InitialActiveZombies;
    private bool[] m_GridsWithZombies;
    private bool m_initializeMap;
    private DifficultyParameters m_DifficultyParameters;

    void Awake() {
        m_DifficultyParameters = GameObject.Find("Scriptsbucket").GetComponent<DifficultyParameters>();
        m_InfectedCountText = GameObject.Find("CurrentInfected").GetComponent<Text>();
        gameStatesManager = GameObject.Find("Scriptsbucket").GetComponent<GameStatesManager>();
        musicM = GameObject.Find("Scriptsbucket").GetComponent<MusicManager>();
		civiliansSpawner = GameObject.Find("Scriptsbucket").GetComponent<CiviliansSpawner>();
		g_MapReader = GameObject.Find("Map").GetComponent<MapReader>();
    }

    void Start() {
        gameStatesManager.MenuGameState.AddListener(OnMenu);
        gameStatesManager.StartingGameState.AddListener(OnStarting);
        gameStatesManager.PlayingGameState.AddListener(OnPlaying);
        gameStatesManager.PausedGameState.AddListener(OnPausing);
        gameStatesManager.EndingGameState.AddListener(OnEnding);
        SetState(gameStatesManager.gameState);
    }

    void Update() {
        if (gameState == StaticData.AvailableGameStates.Playing) {
            if (!m_initializeMap) {
                GameObject.Find("CurrentDifficulty").GetComponent<Text>().text = "Difficulty " + PlayerPrefs.GetInt("Difficulty", 0).ToString() + "." + PlayerPrefs.GetInt("CurrentLevel").ToString();

                m_TimeBeforeSpawn = UnityEngine.Random.Range(2, 5);
                m_GridOffset = -1.0f * new Vector3((m_GridWidth * 0.5f * m_GridCellWidthHeight), (m_GridHeight * 0.5f * m_GridCellWidthHeight), 0.0f);
                m_DifficultyParameters.m_CurrentZombieBoost = m_DifficultyParameters.m_StartingZombieBoost;
                m_CountOfCivilians = m_DifficultyParameters.m_StartingHumans;

                BuildInitialListOfCivilians();
                SetupGameObjectGridList();
                m_initializeMap = true;
            }

            UpdateCivilianGameObjectLists();
            m_InfectedUpdatedThisFrame = 0;
			int amountOfCivilians = civiliansSpawner.arrayOfCivilians.Length;

            if (!m_InitialInfectionDone) { InitialInfection(); }

            for (int i = 0; i < amountOfCivilians; ++i)
            {

				switch (civiliansSpawner.arrayOfCivilians[i].tag)
                {
                    case "Eating":
						ZombieDecay(civiliansSpawner.arrayOfCivilians[i], i, -m_DifficultyParameters.m_BaseEatingSpeed * m_DifficultyParameters.m_CurrentZombieBoost);
						if (!IsHungry(civiliansSpawner.arrayOfCivilians[i]))
                        {
							civiliansSpawner.arrayOfCivilians[i].tag = "Infected";
                        }
                        break;
                    case "Dead":

                        //Do Nothing
                        break;
                    case "Civilian":
						MoveHumanRandomly(civiliansSpawner.arrayOfCivilians[i], i, 0.3f);
                        break;
                    case "Infected":
						ZombieDecay(civiliansSpawner.arrayOfCivilians[i], i, 0.025f);

                        if (i > m_InfectedIndex && m_InfectedUpdatedThisFrame < m_MaxNumberOfInfectedToUpdateEachFrame)
                        {
                            bool diedInWall = false;
							Vector3 infectedPos = civiliansSpawner.arrayOfCivilians[i].transform.position;
                            if (!CanMove(infectedPos))
                            {
								ZombieDied(civiliansSpawner.arrayOfCivilians[i], civiliansSpawner.arrayOfCivilians[i].GetComponent<SpriteRenderer>(), i);
                                diedInWall = true;
                            }

                            ++m_InfectedUpdatedThisFrame;
                            m_InfectedIndex = i;

                            if (diedInWall)
                                break;

							GetClosestCivilian(civiliansSpawner.arrayOfCivilians[i], i);
                        }

						if (IsHungry(civiliansSpawner.arrayOfCivilians[i]))
                        {
                            if (m_InfectedTargets[i] != null)
                            {
								RushCivilian(civiliansSpawner.arrayOfCivilians[i], m_InfectedTargets[i], i);
                            }
                            else
                            {
								MoveHumanRandomly(civiliansSpawner.arrayOfCivilians[i], i, 0.5f);
                            }
                        }

                        break;
                }

            }



            if (m_InfectedUpdatedThisFrame < m_MaxNumberOfInfectedToUpdateEachFrame)
            {
                m_InfectedIndex = 0;
                g_MapReader.PushBloodPixels();
                m_InfectedCountText.text = m_CountOfInfected - m_CountOfDead + " Infected";

            }
        }
    }

    void ZombieDecay(GameObject ActiveInfected, int humanIndex, float DecayRate)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();

        float BlueTarget = .01f;
        float InfectionRate = .04f;
        Color NewColor;
        float Red = ActiveSprite.color.r;
        float Blue = ActiveSprite.color.b;
        float Green = ActiveSprite.color.g;

        if (ActiveSprite.color.g <= 0.0f)
        {
            ZombieDied(ActiveInfected, ActiveSprite, humanIndex);
        }
        else
        {
            if (Red <= m_InfectedActiveRed)
            {
                Green = ActiveSprite.color.g - DecayRate * Time.deltaTime;
                Red = ActiveSprite.color.r - DecayRate * Time.deltaTime;
            }
            else
            {
                Green = ActiveSprite.color.g + InfectionRate * m_DifficultyParameters.m_CurrentZombieBoost * Time.deltaTime;
                Red = ActiveSprite.color.r - InfectionRate * m_DifficultyParameters.m_CurrentZombieBoost * Time.deltaTime;
            }

            if (Blue <= BlueTarget)
            {

                Blue = ActiveSprite.color.b - DecayRate * Time.deltaTime;
            }
            else
            {

                Blue = Math.Max(.001f, ActiveSprite.color.b - InfectionRate * Time.deltaTime);
            }

            NewColor = new Color(Red, Green, Blue);
            ActiveSprite.color = NewColor;
        }
		civiliansSpawner.humanHealthIndex[humanIndex] = ActiveSprite.color.r * 2.0f; //Hooray hacks!
    }

    void ZombieDied(GameObject DyingInfected, SpriteRenderer ZombieRenderer, int i)
    {
        Color ActiveSpriteColor = ZombieRenderer.color;
        ZombieRenderer.color = new Color(0.0f, 0.0f, 0.0f, 0.40f);
        DyingInfected.tag = "Dead";

        m_CountOfDead += 1;
    }

    void GetClosestCivilian(GameObject ActiveInfected, int i)
    {
        GameObject bestTarget = null;
        float ClosestDist = Mathf.Infinity;
        Vector3 currentPosition = ActiveInfected.transform.position;


		foreach (var obj in GameObjectGridList[civiliansSpawner.civilianGridIndex[i]])
        {
            if (obj.tag == "Civilian")
            {
                float TargetDist = Vector3.Distance(currentPosition, obj.transform.position);
                if (TargetDist < ClosestDist)
                {
                    ClosestDist = TargetDist;
                    bestTarget = obj;
                }
            }
        }

        m_InfectedTargets[i] = bestTarget;
    }

    void RushCivilian(GameObject ActiveInfected, GameObject Target, int HumanIndex)
    {
        if (Target == null)
            return;

        Vector3 targetPosition = Target.transform.position;

        if (Vector3.Distance(ActiveInfected.transform.position, Target.transform.position) <= m_DifficultyParameters.m_ZombieConversionRange)
        {
            if (Target.tag != "Infected")
            {
                ActiveInfected.tag = "Eating";
                Target.tag = "Infected";
                m_CountOfInfected += 1;
                m_CountOfCivilians -= 1;

                //Register zombie for panic
                int gridX = GetGridXCoordinateFromWorldPosition(Target.transform.position.x);
                int gridY = GetGridYCoordinateFromWorldPosition(Target.transform.position.y);

                SetGridsWithZombies(gridX, gridY);

                g_MapReader.AddBloodSplat(Target.transform.position, 3, 10);

				civiliansSpawner.humanSpeeds[HumanIndex] = UnityEngine.Random.Range(m_DifficultyParameters.InfectedBaseSpeed - m_DifficultyParameters.InfectedSpeedPlusMinus, m_DifficultyParameters.InfectedBaseSpeed + m_DifficultyParameters.InfectedSpeedPlusMinus);

                if (m_DifficultyParameters.m_CurrentZombieBoost > 1.0f)
                {
                    m_DifficultyParameters.m_CurrentZombieBoost -= m_DifficultyParameters.m_ZombieBoostDegradeAmountPerNewZombie;
                    if (m_DifficultyParameters.m_CurrentZombieBoost < 1.0f)
                    {
                        m_DifficultyParameters.m_CurrentZombieBoost = 1.0f;
                    }
                }

            }
        }
        else
        {
            Vector3 newDirection = Vector3.Normalize(targetPosition - ActiveInfected.transform.position);
            Vector3 newDestination = ActiveInfected.transform.position + (newDirection * m_DifficultyParameters.InfectedBaseSpeed * Time.deltaTime);

            if (CanMove(newDestination))
            {
                ActiveInfected.transform.position = newDestination;
            }
            else
            {
                Vector3 newDestinationX = ActiveInfected.transform.position + (new Vector3(newDirection.x, 0.0f, 0.0f) * m_DifficultyParameters.m_CurrentZombieBoost * Time.deltaTime);
                Vector3 newDestinationY = ActiveInfected.transform.position + (new Vector3(0.0f, newDirection.y, 0.0f) * m_DifficultyParameters.m_CurrentZombieBoost * Time.deltaTime);

                if (newDirection.x > newDirection.y)
                {
                    if (CanMove(newDestinationX))
                    {
                        ActiveInfected.transform.position = newDestinationX;
                    }
                    else if (CanMove(newDestinationY))
                    {
                        ActiveInfected.transform.position = newDestinationY;
                    }
                }
                else
                {
                    if (CanMove(newDestinationY))
                    {
                        ActiveInfected.transform.position = newDestinationY;
                    }
                    else if (CanMove(newDestinationX))
                    {
                        ActiveInfected.transform.position = newDestinationX;
                    }
                }
            }
        }
    }

    void MoveHumanRandomly(GameObject ActiveHuman, int humanIndex, float maxRandomGradualHeadingChange) {
        float terrorModifier = 1.0f;
		if (ActiveHuman.tag != "Infected" && m_GridsWithZombies[civiliansSpawner.civilianGridIndex[humanIndex]]) {
            terrorModifier = 0.02f * (float)m_CountOfInfected;
            if (terrorModifier > 4.0f) {
                terrorModifier = 4.0f;
            }
        }
        Vector3 newDestination;
        if (ActiveHuman.tag == "Infected") {
			newDestination = ActiveHuman.transform.position + (civiliansSpawner.humanSpeeds[humanIndex] * civiliansSpawner.humanHeadings[humanIndex] * civiliansSpawner.humanHealthIndex[humanIndex] * m_DifficultyParameters.m_StartingZombieBoost * Time.deltaTime);
        } else {
			newDestination = ActiveHuman.transform.position + (civiliansSpawner.humanSpeeds[humanIndex] * civiliansSpawner.humanHeadings[humanIndex] * civiliansSpawner.humanHealthIndex[humanIndex] * terrorModifier * Time.deltaTime);
        }
        if (CanMove(newDestination)) {
            ActiveHuman.transform.position = newDestination;
            Vector3 newGradualRandomHeadingchange = new Vector3(UnityEngine.Random.Range(-maxRandomGradualHeadingChange, maxRandomGradualHeadingChange), UnityEngine.Random.Range(-maxRandomGradualHeadingChange, maxRandomGradualHeadingChange), 0.0f);
			civiliansSpawner.humanHeadings[humanIndex] = Vector3.Normalize(civiliansSpawner.humanHeadings[humanIndex] + newGradualRandomHeadingchange);
        } else {
			civiliansSpawner.humanHeadings[humanIndex].x = UnityEngine.Random.Range(-1.0f, 1.0f);
			civiliansSpawner.humanHeadings[humanIndex].y = UnityEngine.Random.Range(-1.0f, 1.0f);
			civiliansSpawner.humanHeadings[humanIndex].z = 0.0f;
			civiliansSpawner.humanHeadings[humanIndex] = Vector3.Normalize(civiliansSpawner.humanHeadings[humanIndex]);
        }
    }

    bool IsHungry(GameObject ActiveInfected) {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        if (ActiveSprite.color.r <= m_InfectedActiveRed) { return true; }
        return false;
    }

    void BuildInitialListOfCivilians() {
		civiliansSpawner.arrayOfCivilians = GameObject.FindGameObjectsWithTag("Civilian");
		int amountOfCivilians = civiliansSpawner.arrayOfCivilians.Length;
		civiliansSpawner.civilianGridIndex = new int[amountOfCivilians];
		civiliansSpawner.humanSpeeds = new float[amountOfCivilians];
		civiliansSpawner.humanHeadings = new Vector3[amountOfCivilians];
        m_InfectedTargets = new GameObject[amountOfCivilians];
		civiliansSpawner.humanHealthIndex = new float[amountOfCivilians];

        for (int i = 0; i < amountOfCivilians; ++i)
        {
			civiliansSpawner.civilianGridIndex[i] = 0;
			civiliansSpawner.humanSpeeds[i] = UnityEngine.Random.Range(m_DifficultyParameters.CivilianBaseSpeed - m_DifficultyParameters.CivilianSpeedPlusMinus, m_DifficultyParameters.CivilianBaseSpeed + m_DifficultyParameters.CivilianSpeedPlusMinus);
			civiliansSpawner.humanHeadings[i].x = UnityEngine.Random.Range(-1.0f, 1.0f);
			civiliansSpawner.humanHeadings[i].y = UnityEngine.Random.Range(-1.0f, 1.0f);
			civiliansSpawner.humanHeadings[i].z = 0.0f;
			civiliansSpawner.humanHeadings[i] = Vector3.Normalize(civiliansSpawner.humanHeadings[i]);
            m_InfectedTargets[i] = null;
			civiliansSpawner.humanHealthIndex[i] = 1.0f;
        }
    }

    void SetupGameObjectGridList() {
        m_GridsWithZombies = new bool[m_GridWidth * m_GridHeight];
        for (int i = 0; i < m_GridWidth * m_GridHeight; ++i)
        {
            GameObjectGridList.Add(new List<GameObject>());
            m_GridsWithZombies[i] = false;
        }
    }

    void DrawDebugGrid()
    {

        for (int i = 0; i <= m_GridWidth; i++)
        {
            Debug.DrawLine(new Vector3((i * m_GridCellWidthHeight), 0.0f, 0.0f) + m_GridOffset, new Vector3((i * m_GridCellWidthHeight), m_GridHeight * m_GridCellWidthHeight, 0.0f) + m_GridOffset, Color.black);
        }

        for (int i = 0; i <= m_GridHeight; i++)
        {
            Debug.DrawLine(new Vector3(0.0f, (i * m_GridCellWidthHeight), 0.0f) + m_GridOffset, new Vector3(m_GridWidth * m_GridCellWidthHeight, (i * m_GridCellWidthHeight), 0.0f) + m_GridOffset, Color.black);
        }
    }

    void UpdateCivilianGameObjectLists()
    {

		int amountOfCivilians = civiliansSpawner.arrayOfCivilians.Length;
        for (int i = 0; i < amountOfCivilians; ++i)
        {

			int gridX = GetGridXCoordinateFromWorldPosition(civiliansSpawner.arrayOfCivilians[i].transform.position.x);
			int gridY = GetGridYCoordinateFromWorldPosition(civiliansSpawner.arrayOfCivilians[i].transform.position.y);
            int newCivilianGridIndex = GetIndexFromGridCoordinates(gridX, gridY);
			if (civiliansSpawner.civilianGridIndex[i] != newCivilianGridIndex)
            {
                //Unregister civilian from old grid group
				GameObjectGridList[civiliansSpawner.civilianGridIndex[i]].Remove(civiliansSpawner.arrayOfCivilians[i]);
                //Register civilian to new grid group
				GameObjectGridList[newCivilianGridIndex].Add(civiliansSpawner.arrayOfCivilians[i]);
				if (civiliansSpawner.arrayOfCivilians[i].tag == "Infected")
                {
                    SetGridsWithZombies(gridX, gridY);
                }

				civiliansSpawner.civilianGridIndex[i] = newCivilianGridIndex;
            }


        }
    }

    void SetGridsWithZombies(int gridX, int gridY)
    {
        m_GridsWithZombies[GetIndexFromGridCoordinates(gridX, gridY)] = true;

        if (gridX < m_GridWidth - 1)
        {
            m_GridsWithZombies[GetIndexFromGridCoordinates(gridX + 1, gridY)] = true;
        }
        if (gridX > 0)
        {
            m_GridsWithZombies[GetIndexFromGridCoordinates(gridX - 1, gridY)] = true;
        }
        if (gridY < m_GridHeight - 1)
        {
            m_GridsWithZombies[GetIndexFromGridCoordinates(gridX, gridY + 1)] = true;
        }
        if (gridY > 0)
        {
            m_GridsWithZombies[GetIndexFromGridCoordinates(gridX, gridY - 1)] = true;
        }
    }

    int GetIndexFromGridCoordinates(int x, int y)
    {
        return (x + (y * m_GridWidth));
    }

    int GetGridXCoordinateFromWorldPosition(float xPos)
    {
        return (int)((xPos - m_GridOffset.x) / m_GridCellWidthHeight);
    }

    int GetGridYCoordinateFromWorldPosition(float yPos)
    {
        return (int)((yPos - m_GridOffset.y) / m_GridCellWidthHeight);
    }

    Vector3 GetGridSquarePositionFromIndex(int index)
    {
        int y = index / m_GridHeight;
        int x = index - (m_GridHeight * y);

        return new Vector3(x * m_GridCellWidthHeight, y * m_GridCellWidthHeight, 0.0f) + m_GridOffset + new Vector3(m_GridCellWidthHeight * 0.5f, m_GridCellWidthHeight * 0.5f);
    }

    bool CanMove(Vector3 destination) {
        return g_MapReader.CanMoveThere(destination[0], destination[1]);
    }


    void InitialInfection()
    {
        m_TimeBeforeSpawn -= Time.deltaTime;
        if (m_TimeBeforeSpawn <= 0)
        {
			int index = civiliansSpawner.arrayOfCivilians.Length;
            int randomCivilian = UnityEngine.Random.Range(0, index);
			civiliansSpawner.arrayOfCivilians[randomCivilian].tag = "Infected";
			musicM.SoundManager(civiliansSpawner.arrayOfCivilians[randomCivilian]);

            m_InitialActiveZombies += 1;
            m_CountOfInfected += 1;
            m_CountOfCivilians -= 1;
            m_TimeBeforeSpawn += UnityEngine.Random.Range(2, 5);

            if (m_InitialActiveZombies == m_DifficultyParameters.m_NumberOfZombies) { m_InitialInfectionDone = true; }
        }
    }

    //Listener functions a defined for every GameState
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

}
