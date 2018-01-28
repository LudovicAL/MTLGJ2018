using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class ColorChange : MonoBehaviour
{

    //I love you Christ

    List<Vector2> GridList = new List<Vector2>();
    Color CurrentColor;
    Color NewColor;
    float Red, Blue, Green;
    float InfectedBaseSpeed = .5f;
	float InfectedSpeedPlusMinus = .2f;
    float CivilianBaseSpeed = .1f;
	float CivilianSpeedPlusMinus = .05f;
    public Transform Civilian;
    private MapReader g_MapReader;
	bool m_HasMapReader = false;
	float m_ZombieConversionRange = 0.05f;
    public int CountOfInfected;
    public int StartingCivilians = 3000;

    public AudioClip Afraid;
    public AudioClip Moans;
    private AudioSource Source;
    int m_NumberOfZombies = 1;

    int m_MaxNumberOfInfectedToUpdateEachFrame = 100;
	int m_InfectedIndex = 0;
	int m_InfectedUpdatedThisFrame = 0;

    // Use this for initialization
    void Start()
    {
        float yAxis = Civilian.position.y;
        float xAxis = Civilian.position.x;
        m_GridOffset = -1.0f * new Vector3((m_GridWidth * 0.5f * m_GridCellWidthHeight), (m_GridHeight * 0.5f * m_GridCellWidthHeight), 0.0f);

        if (GameObject.Find("Map") != null)
        {
            g_MapReader = GameObject.Find("Map").GetComponent<MapReader>();
			m_HasMapReader = true;
        }

        for (int i = 0; i < StartingCivilians; i++)
        {
            if (g_MapReader != null)
            { // valid map spawn
                float[] randomSpawnPos = g_MapReader.FindRandomWhiteSpace();
                xAxis = randomSpawnPos[0];
                yAxis = randomSpawnPos[1];
            }
            //else
            //{ // test spawn
            //    xAxis += 2;
            //    if (xAxis >= 35)
            //    {
            //        xAxis = Civilian.position.x;
            //        yAxis += 2;
            //    }
            //}



            Transform newCivilian = Instantiate(Civilian, new Vector3(xAxis, yAxis, 0), Quaternion.identity);
            newCivilian.tag = "Civilian";
        }


        BuildInitialListOfCivilians();
        SetupGameObjectGridList();
        InitialInfection();

    }

    // Update is called once per frame
    void Update()
    {
		DrawDebugGrid();
        UpdateCivilianGameObjectLists();
		m_InfectedUpdatedThisFrame = 0;

        int amountOfCivilians = m_Civilians.Length;
        for (int i = 0; i < amountOfCivilians; ++i)
        {
            switch (m_Civilians[i].tag)
            {
                case "Eating":
                    ZombieDecay(m_Civilians[i],i, -0.025f);
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
					    ++m_InfectedUpdatedThisFrame;
					    m_InfectedIndex = i;
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
		}

    }

	void ZombieDecay(GameObject ActiveInfected, int humanIndex, float DecayRate)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        float RegTarget = 0.5f;
        float BlueTarget = .01f;
        float InfectionRate = .04f;

        Red = ActiveSprite.color.r;
        Blue = ActiveSprite.color.b;

        if (ActiveSprite.color.g <= 0.0f)
        {
            Color ActiveSpriteColor = ActiveSprite.color;
            ActiveSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.40f);
            ActiveInfected.tag = "Dead";
        }
        else
        {
            if (Red <= RegTarget)
            {
                Green = ActiveSprite.color.g - DecayRate * Time.deltaTime;
                Red = ActiveSprite.color.r - DecayRate * Time.deltaTime;
            }
            else
            {
                Green = ActiveSprite.color.g + InfectionRate * Time.deltaTime;
                Red = ActiveSprite.color.r - InfectionRate * Time.deltaTime;
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

		if (Vector3.Distance(ActiveInfected.transform.position, Target.transform.position) <= m_ZombieConversionRange)
        {
			if (Target.tag != "Infected") {
                ActiveInfected.tag = "Eating";
                Target.tag = "Infected";
                CountOfInfected += 1;
                m_HumanSpeeds [HumanIndex] = UnityEngine.Random.Range (InfectedBaseSpeed - InfectedSpeedPlusMinus, InfectedBaseSpeed + InfectedSpeedPlusMinus); 
				//GameObject.Instantiate (m_BloodSplat).transform.position = Target.transform.position;
				//Do this in the texture. AURELIE! HERE! <3
			}
        }
        else
        {
			Vector3 newDirection = Vector3.Normalize(targetPosition - ActiveInfected.transform.position);
			Vector3 newDestination = ActiveInfected.transform.position + (newDirection * InfectedBaseSpeed * Time.deltaTime);

			if (CanMove (newDestination)) 
			{
				ActiveInfected.transform.position = newDestination;
			} 
			else 
			{
				Vector3 newDestinationX = ActiveInfected.transform.position + (new Vector3(newDirection.x, 0.0f, 0.0f) * InfectedBaseSpeed * Time.deltaTime);
				Vector3 newDestinationY = ActiveInfected.transform.position + (new Vector3(0.0f, newDirection.y, 0.0f) * InfectedBaseSpeed * Time.deltaTime);

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
		if (ActiveHuman.tag != "Infected" && m_GridsWithZombies [m_CivilianGridIndex[humanIndex]]) {
			terrorModifier = 4.0f;
		}
		Vector3 newDestination = ActiveHuman.transform.position + (m_HumanSpeeds[humanIndex] * m_HumanHeadings[humanIndex] * m_HumanHealthIndex [humanIndex]* terrorModifier * Time.deltaTime);
		
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
        if (ActiveSprite.color.r <= .5f) { return true; }
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
			m_HumanSpeeds[i] = UnityEngine.Random.Range(CivilianBaseSpeed - CivilianSpeedPlusMinus, CivilianBaseSpeed + CivilianSpeedPlusMinus);
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
					m_GridsWithZombies [newCivilianGridIndex] = true;
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

                m_CivilianGridIndex[i] = newCivilianGridIndex;
            }
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
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Civilian");
        for (int i = 1; i <= m_NumberOfZombies; ++i)
        {
            int index = objects.Length;
            int randomCivilian = UnityEngine.Random.Range(0, index);
            objects[randomCivilian].tag = "Infected";
            SoundManager(objects[randomCivilian]);
            CountOfInfected += 1;
        }
    }

    void SoundManager(GameObject ActiveObject)
    {
        ActiveObject.AddComponent<AudioSource>();
        Source = ActiveObject.GetComponent<AudioSource>();
        Source.spatialBlend = 1;
        Source.rolloffMode = AudioRolloffMode.Linear;
        Source.minDistance = 9f;
        Source.maxDistance = 11f;
        Source.PlayOneShot(Afraid);
    }
}
