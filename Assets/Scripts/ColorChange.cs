using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ColorChange : MonoBehaviour
{
	public List<DifficultyParameters> m_DifficultyParameters;
	public int m_DifficultyCurrentLevel = 0;
    //I love you Christ
	private GameStatesManager gameStatesManager;
	private StaticData.AvailableGameStates gameState;
	private MusicManager musicM;
    List<Vector2> GridList = new List<Vector2>();
    Color CurrentColor;
    Color NewColor;
    float Red, Blue, Green;

    public Transform Civilian;
    private MapReader g_MapReader;
	bool m_HasMapReader = false;

    public int CountOfInfected;
	public int DeadInfected;
    public int CountOfCivilians;

    public AudioClip Afraid;
    public AudioClip Moans;
    private AudioSource Source;

    int m_MaxNumberOfInfectedToUpdateEachFrame = 100;
	int m_InfectedIndex = 0;
	int m_InfectedUpdatedThisFrame = 0;

    float m_TimeBeforeSpawn;
    int m_InitialActiveZombies;
    public bool m_InitialInfectionDone;

	private Text m_InfectedCountText;

	void Awake()
	{
		m_InfectedCountText = GameObject.Find ("CurrentInfected").GetComponent<Text> ();
	}

    // Use this for initialization
    void Start()
    {
			musicM = GameObject.Find("Scriptsbucket").GetComponent<MusicManager>();
            m_TimeBeforeSpawn = UnityEngine.Random.Range(2, 5);
            m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost = m_DifficultyParameters[m_DifficultyCurrentLevel].m_StartingZombieBoost;
            gameStatesManager = GameObject.Find("Scriptsbucket").GetComponent<GameStatesManager>();
            gameStatesManager.MenuGameState.AddListener(OnMenu);
            gameStatesManager.StartingGameState.AddListener(OnStarting);
            gameStatesManager.PlayingGameState.AddListener(OnPlaying);
            gameStatesManager.PausedGameState.AddListener(OnPausing);
            gameStatesManager.EndingGameState.AddListener(OnEnding);
            SetState(gameStatesManager.gameState);
            float yAxis = Civilian.position.y;
            float xAxis = Civilian.position.x;
            m_GridOffset = -1.0f * new Vector3((m_GridWidth * 0.5f * m_GridCellWidthHeight), (m_GridHeight * 0.5f * m_GridCellWidthHeight), 0.0f);
            CountOfCivilians = m_DifficultyParameters[m_DifficultyCurrentLevel].m_StartingHumans;
            if (GameObject.Find("Map") != null)
            {
                g_MapReader = GameObject.Find("Map").GetComponent<MapReader>();
                m_HasMapReader = true;
            }

            for (int i = 0; i < m_DifficultyParameters[m_DifficultyCurrentLevel].m_StartingHumans; i++)
            {
                if (g_MapReader != null)
                { // valid map spawn
                    float[] randomSpawnPos = g_MapReader.FindRandomWhiteSpace();
                    xAxis = randomSpawnPos[0];
                    yAxis = randomSpawnPos[1];
                }


                Transform newCivilian = Instantiate(Civilian, new Vector3(xAxis, yAxis, 0), Quaternion.identity);
                newCivilian.tag = "Civilian";
            }


            BuildInitialListOfCivilians();
            SetupGameObjectGridList();

    }

    // Update is called once per frame
    void Update()
    {
		if (gameState == StaticData.AvailableGameStates.Playing) {
            if (!m_InitialInfectionDone) { InitialInfection(); }

			DrawDebugGrid();
	        UpdateCivilianGameObjectLists();
			m_InfectedUpdatedThisFrame = 0;

	        int amountOfCivilians = m_Civilians.Length;
	        for (int i = 0; i < amountOfCivilians; ++i)
	        {
	            switch (m_Civilians[i].tag)
	            {
	                case "Eating":
						ZombieDecay(m_Civilians[i],i, -m_DifficultyParameters[m_DifficultyCurrentLevel].m_BaseEatingSpeed * m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost);
	                    if (!IsHungry(m_Civilians[i]))
	                    {
	                        m_Civilians[i].tag = "Infected";
	                    }
	                    break;
	                case "Dead":

	                    break;
						
	                case "Civilian":
						MoveHumanRandomly(m_Civilians[i], i, 0.3f);
	                    break;
				    case "Infected":
					    ZombieDecay (m_Civilians[i], i, 0.025f);

					    if (i > m_InfectedIndex && m_InfectedUpdatedThisFrame < m_MaxNumberOfInfectedToUpdateEachFrame) 
					    {
							bool diedInWall = false;
							Vector3 infectedPos = m_Civilians [i].transform.position;
							if (!CanMove(infectedPos))
							{
								ZombieDied (m_Civilians [i], m_Civilians [i].GetComponent<SpriteRenderer>());
								diedInWall = true;
							}

						    ++m_InfectedUpdatedThisFrame;
						    m_InfectedIndex = i;

							if (diedInWall)
								break;
						
						    GetClosestCivilian(m_Civilians[i], i);
					    }
						
	                    if (IsHungry(m_Civilians[i]))
	                    {
							if (m_InfectedTargets[i] != null)
	                        {
								RushCivilian(m_Civilians[i], m_InfectedTargets[i], i);
	                        }
	                        else
	                        {
							MoveHumanRandomly(m_Civilians[i], i, 0.5f);
	                        }
	                    }

	                    break;
	            }

	        }

			if (m_InfectedUpdatedThisFrame < m_MaxNumberOfInfectedToUpdateEachFrame) {
				m_InfectedIndex = 0;
				g_MapReader.PushBloodPixels ();
				m_InfectedCountText.text="Living:" + CountOfCivilians + " Infected:"+ CountOfInfected + " Dead:" + DeadInfected;
			}
		}
    }
    float m_RegTarget = 0.5f;

    void ZombieDecay(GameObject ActiveInfected, int humanIndex, float DecayRate)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        
        float BlueTarget = .01f;
        float InfectionRate = .04f;

        Red = ActiveSprite.color.r;
        Blue = ActiveSprite.color.b;

        if (ActiveSprite.color.g <= 0.0f)
        {
			ZombieDied (ActiveInfected, ActiveSprite);
        }
        else
        {
            if (Red <= m_RegTarget)
            {
                Green = ActiveSprite.color.g - DecayRate * Time.deltaTime;
                Red = ActiveSprite.color.r - DecayRate * Time.deltaTime;
            }
            else
            {
				Green = ActiveSprite.color.g + InfectionRate * m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost * Time.deltaTime;
				Red = ActiveSprite.color.r - InfectionRate * m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost * Time.deltaTime;
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
            //print("Red : " + Red + "  Green : " + ActiveSprite.color.g + "  Blue : " + Blue);
            ActiveSprite.color = NewColor;
        }
		m_HumanHealthIndex [humanIndex] = ActiveSprite.color.r * 2.0f; //Hooray hacks!
    }

	void ZombieDied(GameObject DyingInfected, SpriteRenderer ZombieRenderer)
	{
		Color ActiveSpriteColor = ZombieRenderer.color;
		ZombieRenderer.color = new Color(0.0f, 0.0f, 0.0f, 0.40f);
		DyingInfected.tag = "Dead";
		//CountOfInfected -= 1;
		DeadInfected += 1;
	}

    void GetClosestCivilian(GameObject ActiveInfected, int i)
    {

        GameObject bestTarget = null;
        float ClosestDist = Mathf.Infinity;
        Vector3 currentPosition = ActiveInfected.transform.position;


        foreach (var obj in GameObjectGridList[m_CivilianGridIndex[i]])
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

		if (Vector3.Distance(ActiveInfected.transform.position, Target.transform.position) <= m_DifficultyParameters[m_DifficultyCurrentLevel].m_ZombieConversionRange)
        {
			if (Target.tag != "Infected") {
                ActiveInfected.tag = "Eating";
                Target.tag = "Infected";
                CountOfInfected += 1;
                CountOfCivilians -= 1;

				//Register zombie for panic
				int gridX = GetGridXCoordinateFromWorldPosition (Target.transform.position.x);
				int gridY = GetGridYCoordinateFromWorldPosition (Target.transform.position.y);

				SetGridsWithZombies (gridX, gridY);

				//g_MapReader.AddBloodSplat (Target.transform.position, 6, 20);
				g_MapReader.AddBloodSplat (Target.transform.position, 3, 10);

                m_HumanSpeeds [HumanIndex] = UnityEngine.Random.Range (m_DifficultyParameters[m_DifficultyCurrentLevel].InfectedBaseSpeed - m_DifficultyParameters[m_DifficultyCurrentLevel].InfectedSpeedPlusMinus, m_DifficultyParameters[m_DifficultyCurrentLevel].InfectedBaseSpeed + m_DifficultyParameters[m_DifficultyCurrentLevel].InfectedSpeedPlusMinus); 
				//GameObject.Instantiate (m_BloodSplat).transform.position = Target.transform.position;
				//Do this in the texture. AURELIE! HERE! <3

				if (m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost > 1.0f) {
					m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost -= m_DifficultyParameters[m_DifficultyCurrentLevel].m_ZombieBoostDegradeAmountPerNewZombie;
					if (m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost < 1.0f) 
					{
						m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost = 1.0f;
					}
				} 

			}
        }
        else
        {
			Vector3 newDirection = Vector3.Normalize(targetPosition - ActiveInfected.transform.position);
			Vector3 newDestination = ActiveInfected.transform.position + (newDirection * m_DifficultyParameters[m_DifficultyCurrentLevel].InfectedBaseSpeed * Time.deltaTime);

			if (CanMove (newDestination)) 
			{
				ActiveInfected.transform.position = newDestination;
			} 
			else 
			{
				Vector3 newDestinationX = ActiveInfected.transform.position + (new Vector3(newDirection.x, 0.0f, 0.0f) * m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost * Time.deltaTime);
				Vector3 newDestinationY = ActiveInfected.transform.position + (new Vector3(0.0f, newDirection.y, 0.0f) * m_DifficultyParameters[m_DifficultyCurrentLevel].m_CurrentZombieBoost * Time.deltaTime);

				if (newDirection.x > newDirection.y) 
				{
					if (CanMove (newDestinationX)) 
					{
						ActiveInfected.transform.position = newDestinationX;
					} 
					else if (CanMove (newDestinationY)) 
					{
						ActiveInfected.transform.position = newDestinationY;
					}
				} 
				else 
				{
					if (CanMove (newDestinationY)) 
					{
						ActiveInfected.transform.position = newDestinationY;
					} 
					else if (CanMove (newDestinationX)) 
					{
						ActiveInfected.transform.position = newDestinationX;
					}
				}
			}	
        }
    }

	void MoveHumanRandomly(GameObject ActiveHuman, int humanIndex, float maxRandomGradualHeadingChange)
	{
		float terrorModifier = 1.0f;
		if (ActiveHuman.tag != "Infected" && m_GridsWithZombies [m_CivilianGridIndex[humanIndex]]) 
		{
			terrorModifier = 0.02f * (float)CountOfInfected;

			if (terrorModifier > 4.0f) 
			{
				terrorModifier = 4.0f;
			}
		}

		Vector3 newDestination;
		if (ActiveHuman.tag == "Infected") 
		{
			newDestination = ActiveHuman.transform.position + (m_HumanSpeeds [humanIndex] * m_HumanHeadings [humanIndex] * m_HumanHealthIndex [humanIndex] * m_DifficultyParameters[m_DifficultyCurrentLevel].m_StartingZombieBoost * Time.deltaTime);
		} 
		else 
		{
			newDestination = ActiveHuman.transform.position + (m_HumanSpeeds[humanIndex] * m_HumanHeadings[humanIndex] * m_HumanHealthIndex [humanIndex]* terrorModifier * Time.deltaTime);
		}

		if (CanMove (newDestination)) {
			ActiveHuman.transform.position = newDestination;
			Vector3 newGradualRandomHeadingchange = new Vector3 (UnityEngine.Random.Range (-maxRandomGradualHeadingChange, maxRandomGradualHeadingChange), UnityEngine.Random.Range (-maxRandomGradualHeadingChange, maxRandomGradualHeadingChange), 0.0f);
			m_HumanHeadings [humanIndex] = Vector3.Normalize (m_HumanHeadings [humanIndex] + newGradualRandomHeadingchange);
		} 
		else {
			m_HumanHeadings[humanIndex].x = UnityEngine.Random.Range(-1.0f, 1.0f);
			m_HumanHeadings[humanIndex].y = UnityEngine.Random.Range(-1.0f, 1.0f);
			m_HumanHeadings[humanIndex].z = 0.0f;
			m_HumanHeadings [humanIndex] = Vector3.Normalize (m_HumanHeadings [humanIndex]);
		}
	}

    bool IsHungry(GameObject ActiveInfected)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        if (ActiveSprite.color.r <= m_RegTarget) { return true; }
        return false;
    }

    int m_GridWidth = 18;
    int m_GridHeight = 14;
    float m_GridCellWidthHeight = 0.5f;
    Vector3 m_GridOffset;

    List<List<GameObject>> GameObjectGridList = new List<List<GameObject>>();
    GameObject[] m_Civilians;
    int[] m_CivilianGridIndex;
	float[] m_HumanSpeeds;
	float[] m_HumanHealthIndex;
	Vector3[] m_HumanHeadings;
	GameObject[] m_InfectedTargets;
	bool[] m_GridsWithZombies;

    void BuildInitialListOfCivilians()
    {
        m_Civilians = GameObject.FindGameObjectsWithTag("Civilian");

        int amountOfCivilians = m_Civilians.Length;
        m_CivilianGridIndex = new int[amountOfCivilians];
		m_HumanSpeeds = new float[amountOfCivilians];
		m_HumanHeadings = new Vector3[amountOfCivilians];
		m_InfectedTargets = new GameObject[amountOfCivilians];
		m_HumanHealthIndex = new float[amountOfCivilians];

        for (int i = 0; i < amountOfCivilians; ++i)
        {
            m_CivilianGridIndex[i] = 0;
			m_HumanSpeeds[i] = UnityEngine.Random.Range(m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianBaseSpeed - m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianSpeedPlusMinus, m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianBaseSpeed + m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianSpeedPlusMinus);
			m_HumanHeadings[i].x = UnityEngine.Random.Range(-1.0f, 1.0f);
			m_HumanHeadings[i].y = UnityEngine.Random.Range(-1.0f, 1.0f);
			m_HumanHeadings[i].z = 0.0f;
			m_HumanHeadings [i] = Vector3.Normalize (m_HumanHeadings [i]);
			m_InfectedTargets[i] = null;
			m_HumanHealthIndex [i] = 1.0f;
        }
    }

    void SetupGameObjectGridList()
    {
		m_GridsWithZombies = new bool[m_GridWidth * m_GridHeight];
        for (int i = 0; i < m_GridWidth * m_GridHeight; ++i)
        {
            GameObjectGridList.Add(new List<GameObject>());
			m_GridsWithZombies [i] = false;
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
        int amountOfCivilians = m_Civilians.Length;
        for (int i = 0; i < amountOfCivilians; ++i)
        {
			int gridX = GetGridXCoordinateFromWorldPosition (m_Civilians [i].transform.position.x);
			int gridY = GetGridYCoordinateFromWorldPosition (m_Civilians [i].transform.position.y);
			int newCivilianGridIndex = GetIndexFromGridCoordinates(gridX,gridY);
            if (m_CivilianGridIndex[i] != newCivilianGridIndex)
            {
                //Unregister civilian from old grid group
                GameObjectGridList[m_CivilianGridIndex[i]].Remove(m_Civilians[i]);
                //Register civilian to new grid group
                GameObjectGridList[newCivilianGridIndex].Add(m_Civilians[i]);
				if (m_Civilians[i].tag == "Infected")
				{
					SetGridsWithZombies (gridX, gridY);
				}

                m_CivilianGridIndex[i] = newCivilianGridIndex;
            }
        }
    }

	void SetGridsWithZombies(int gridX, int gridY)
	{
		m_GridsWithZombies [GetIndexFromGridCoordinates(gridX,gridY)] = true;

		if (gridX < m_GridWidth - 1) {
			m_GridsWithZombies [GetIndexFromGridCoordinates(gridX + 1,gridY)] = true;
		}
		if (gridX > 0) {
			m_GridsWithZombies [GetIndexFromGridCoordinates(gridX - 1,gridY)] = true;
		}
		if (gridY < m_GridHeight - 1) {
			m_GridsWithZombies [GetIndexFromGridCoordinates(gridX,gridY + 1)] = true;
		}
		if (gridY > 0) {
			m_GridsWithZombies [GetIndexFromGridCoordinates(gridX,gridY - 1)] = true;
		}
	}

    void Test_DrawCivilianGameObjectListDebugLinesStaggered()
    {
        int amountOfCivilians = m_Civilians.Length;
        for (int i = 0; i < amountOfCivilians; ++i)
        {
            Debug.DrawLine(m_Civilians[i].transform.position, GetGridSquarePositionFromIndex(m_CivilianGridIndex[i]), Color.black);
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

	bool CanMove(Vector3 destination)
	{
		if (!m_HasMapReader)
			return false;
		
		return g_MapReader.CanMoveThere(destination[0], destination[1]);
	}


    void InitialInfection()
    {
        m_TimeBeforeSpawn -= Time.deltaTime;
        if (m_TimeBeforeSpawn <= 0 )
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Civilian");

            int index = objects.Length;
            int randomCivilian = UnityEngine.Random.Range(0, index);
            objects[randomCivilian].tag = "Infected";
            SoundManager(objects[randomCivilian]);

            m_InitialActiveZombies += 1;
			CountOfInfected += 1;
			CountOfCivilians -= 1;
            m_TimeBeforeSpawn += UnityEngine.Random.Range(2, 5);

            if (m_InitialActiveZombies == m_DifficultyParameters[m_DifficultyCurrentLevel].m_NumberOfZombies) { m_InitialInfectionDone = true; }
        }
    }

    void SoundManager(GameObject ActiveObject)
    {
		musicM.PlayMusic ();
        ActiveObject.AddComponent<AudioSource>();
        Source = ActiveObject.GetComponent<AudioSource>();
        Source.spatialBlend = 1;
        Source.rolloffMode = AudioRolloffMode.Linear;
        Source.minDistance = 9f;
        Source.maxDistance = 11f;
        Source.PlayOneShot(Afraid);
    }

	//Listener functions a defined for every GameState
	protected void OnMenu() {
		SetState (StaticData.AvailableGameStates.Menu);
	}

	protected void OnStarting() {
		SetState (StaticData.AvailableGameStates.Starting);

	}

	protected void OnPlaying() {
		SetState (StaticData.AvailableGameStates.Playing);

	}

	protected void OnPausing() {
		SetState (StaticData.AvailableGameStates.Paused);
	}

	protected void OnEnding() {
		SetState (StaticData.AvailableGameStates.Ending);
	}

	private void SetState(StaticData.AvailableGameStates state) {
		gameState = state;
	}

	//Use this function to request a game state change from the GameStateManager
	private void RequestGameStateChange(StaticData.AvailableGameStates state) {
		gameStatesManager.ChangeGameState (state);
	}
}
