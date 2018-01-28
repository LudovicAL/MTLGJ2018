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
	float InfectedSpeedPlusMinus = .5f;
    float CivilianBaseSpeed = .1f;
	float CivilianSpeedPlusMinus = .1f;
    public Transform Civilian;
    private MapReader g_MapReader;
	bool m_HasMapReader = false;

	int m_MaxNumberOfInfectedToUpdateEachFrame = 100;
	int m_InfectedIndex = 0;
	int m_InfectedUpdatedThisFrame = 0;

	//SOURCE TREE SUCKS MY BALLS

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

        for (int i = 0; i < 3000; i++)
        {
            if (g_MapReader != null)
            { // valid map spawn
                float[] randomSpawnPos = g_MapReader.FindRandomWhiteSpace();
                xAxis = randomSpawnPos[0];
                yAxis = randomSpawnPos[1];
            }
            else
            { // test spawn
                xAxis += 2;
                if (xAxis >= 35)
                {
                    xAxis = Civilian.position.x;
                    yAxis += 2;
                }
            }



            Transform newCivilian = Instantiate(Civilian, new Vector3(xAxis, yAxis, 0), Quaternion.identity);
            newCivilian.tag = "Civilian";
        }


        BuildInitialListOfCivilians();
        SetupGameObjectGridList();
        InitialInfect();

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
                case "Dead":

                    break;
					
                case "Civilian":
					MoveRandomly(m_Civilians[i]);
                    break;
			case "Infected":
				ZombieDecay (m_Civilians [i]);

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
							RushCivilian(m_Civilians[i], m_InfectedTargets[i]);
                        }
                        else
                        {
                            MoveRandomly(m_Civilians[i]);
                        }
                    }

                    break;
            }

        }

		if (m_InfectedUpdatedThisFrame < m_MaxNumberOfInfectedToUpdateEachFrame) {
			m_InfectedIndex = 0;
		}

    }

    void ZombieDecay(GameObject ActiveInfected)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        float RegTarget = 0.5f;
        float BlueTarget = .01f;
        float InfectionRate = .04f;
        float DecayRate = .025f;

        Red = ActiveSprite.color.r;
        Blue = ActiveSprite.color.b;

        if (ActiveSprite.color.g <= 0.0f)
        {
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

    }

    void GetClosestCivilian(GameObject ActiveInfected, int i)
    {

        GameObject bestTarget = null;
        float ClosestDist = Mathf.Infinity;
        Vector3 currentPosition = ActiveInfected.transform.position;


        foreach (var obj in GameObjectGridList[m_CivilianGridIndex[i]])
        {
            if (obj.tag != "Infected")
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

    void RushCivilian(GameObject ActiveInfected, GameObject Target)
    {
		if (Target == null)
			return;
		
        Vector3 targetPosition = Target.transform.position;

        if (Vector3.Distance(ActiveInfected.transform.position, Target.transform.position) <= .1f)
        {
            Target.tag = "Infected";
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

    void MoveRandomly(GameObject ActiveInfected)
    {

		Vector3 newDestination =  Vector3.MoveTowards(
			ActiveInfected.transform.position,
			ActiveInfected.transform.position + new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f),
			InfectedBaseSpeed * Time.deltaTime);
		if (CanMove(newDestination))
		{
			ActiveInfected.transform.position = newDestination;
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
	Vector3[] m_HumanHeadings;
	GameObject[] m_InfectedTargets;

    void BuildInitialListOfCivilians()
    {
        m_Civilians = GameObject.FindGameObjectsWithTag("Civilian");

        int amountOfCivilians = m_Civilians.Length;
        m_CivilianGridIndex = new int[amountOfCivilians];
		m_HumanSpeeds = new float[amountOfCivilians];
		m_HumanHeadings = new Vector3[amountOfCivilians];
		m_InfectedTargets = new GameObject[amountOfCivilians];

        for (int i = 0; i < amountOfCivilians; ++i)
        {
            m_CivilianGridIndex[i] = 0;
			m_HumanSpeeds [i] = CivilianBaseSpeed;
			m_HumanHeadings[i].x = 1.0f;
			m_HumanHeadings[i].y = 1.0f;
			m_HumanHeadings[i].z = 0.0f;
			m_InfectedTargets[i] = null;
        }
    }

    void SetupGameObjectGridList()
    {
        for (int i = 0; i < m_GridWidth * m_GridHeight; ++i)
        {
            GameObjectGridList.Add(new List<GameObject>());
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
            int newCivilianGridIndex = GetIndexFromGridCoordinates(GetGridXCoordinateFromWorldPosition(m_Civilians[i].transform.position.x), GetGridYCoordinateFromWorldPosition(m_Civilians[i].transform.position.y));
            if (m_CivilianGridIndex[i] != newCivilianGridIndex)
            {
                //Unregister civilian from old grid group
                GameObjectGridList[m_CivilianGridIndex[i]].Remove(m_Civilians[i]);
                //Register civilian to new grid group
                GameObjectGridList[newCivilianGridIndex].Add(m_Civilians[i]);

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


    void InitialInfect()
    {

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Civilian");
        int index = objects.Length;
        int randomCivilian = UnityEngine.Random.Range(0, index);
        objects[randomCivilian].tag = "Infected";

    }
}
