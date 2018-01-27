using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class ColorChange : MonoBehaviour
{



    List<Vector2> GridList = new List<Vector2>();
    Color CurrentColor;
    Color NewColor;
    float Red, Blue, Green;
    float InfectedSpeed = .5f;
    float CivilianSPeed = .1f;
    public Transform Civilian;
    private MapReader g_MapReader;

    // Use this for initialization
    void Start()
    {
        float yAxis = Civilian.position.y;
        float xAxis = Civilian.position.x;
        m_GridOffset = -1.0f * new Vector3((m_GridWidth * 0.5f * m_GridCellWidthHeight), (m_GridHeight * 0.5f * m_GridCellWidthHeight), 0.0f);

        if (GameObject.Find("Map") != null)
        {
            g_MapReader = GameObject.Find("Map").GetComponent<MapReader>();
        }

        for (int i = 0; i < 100; i++)
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
        UpdateCivilianGameObjectLists();

        int amountOfCivilians = m_Civilians.Length;
        for (int i = 0; i < amountOfCivilians; ++i)
        {


            switch (m_Civilians[i].tag)
            {
                case "Dead":

                    break;
                case "Civilian":

                    break;
                case "Infected":
                    ZombieDecay(m_Civilians[i]);
                    GameObject target = GetClosestCivilian(m_Civilians[i], i);
                    if (IsHungry(m_Civilians[i]))
                    {
                        if (target != null)
                        {
                            RushCivilian(m_Civilians[i], target);
                        }
                        else
                        {
                            MoveRandomly(m_Civilians[i]);
                        }
                    }

                    break;
            }

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

    GameObject GetClosestCivilian(GameObject ActiveInfected, int i)
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

        return bestTarget;

    }

    void RushCivilian(GameObject ActiveInfected, GameObject Target)
    {
        Vector3 targetPosition = Target.transform.position;

        if (Vector3.Distance(ActiveInfected.transform.position, Target.transform.position) <= .1f)
        {
            Target.tag = "Infected";
        }
        else
        {
            ActiveInfected.transform.position =
                Vector3.MoveTowards(
                ActiveInfected.transform.position,
                targetPosition,
                InfectedSpeed * Time.deltaTime);
        }
    }

    void MoveRandomly(GameObject ActiveInfected)
    {
        ActiveInfected.transform.position =
            Vector3.MoveTowards(
            ActiveInfected.transform.position,
            ActiveInfected.transform.position + new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f),
                InfectedSpeed * Time.deltaTime * 3.0f);
    }

    bool IsHungry(GameObject ActiveInfected)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        if (ActiveSprite.color.r <= .5f) { return true; }
        return false;
    }

    int m_GridWidth = 50;
    int m_GridHeight = 50;
    float m_GridCellWidthHeight = 8.0f;
    Vector3 m_GridOffset;

    List<List<GameObject>> GameObjectGridList = new List<List<GameObject>>();
    GameObject[] m_Civilians;
    int[] m_CivilianGridIndex;

    void BuildInitialListOfCivilians()
    {
        m_Civilians = GameObject.FindGameObjectsWithTag("Civilian");

        int amountOfCivilians = m_Civilians.Length;
        m_CivilianGridIndex = new int[amountOfCivilians];

        for (int i = 0; i < amountOfCivilians; ++i)
        {
            m_CivilianGridIndex[i] = 0;
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


    void InitialInfect()
    {

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Civilian");
        int index = objects.Length;
        int randomCivilian = UnityEngine.Random.Range(0, index);
        objects[randomCivilian].tag = "Infected";

    }
}
