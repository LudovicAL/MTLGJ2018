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
    float InfectedSpeed = 3f;
    float CivilianSPeed = 1.5f;
    public Transform Civilian;

	private MapReader g_MapReader;

    // Use this for initialization
    void Start()
    {
		g_MapReader = GameObject.Find ("MapReader").GetComponent<MapReader> ();

        float yAxis = Civilian.position.y;
        float xAxis = Civilian.position.x;

        GridSystem();

        for (int i = 0; i < 1000; i++)
        {
			if (g_MapReader != null) { // valid map spawn
				float[] randomSpawnPos = g_MapReader.FindRandomWhiteSpace ();
				xAxis = randomSpawnPos [0];
				yAxis = randomSpawnPos [1];
			} else { // test spawn
				xAxis += 2;
				if (xAxis >= 35) {
					xAxis = Civilian.position.x;
					yAxis += 2;
				}
			}

			Transform newCivilian = Instantiate(Civilian, new Vector3(xAxis, yAxis, 0), Quaternion.identity);
            newCivilian.tag = "Civilian";
            newCivilian.name = "Civilian" + i;
            GridPosition(newCivilian);
        }

        

    }

    // Update is called once per frame
    void Update()
    {
        DrawDebugGrid();
        //string[] ActiveChar = new string[] { "Civilian", "Infected" };
        //for (int i = 0; i <= 1; i++)
        //{
        //    GameObject[] objects = GameObject.FindGameObjectsWithTag(ActiveChar[i]);
        //    foreach (GameObject obj in objects)
        //    { 
        //        switch (i)
        //        {
        //            case 0:

        //                break;
        //            case 1:
        //                ZombieDecay(obj);
        //                GameObject target = GetClosestCivilian(obj);
        //                if (target != null && IsHungry(obj)) { RushCivilian(obj, target); }

        //                break;
        //        }
        //    }
        //}

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

        if (ActiveSprite.color.g <= 0f)
        {
            ActiveSprite.tag = "Dead";
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

    GameObject GetClosestCivilian(GameObject ActiveInfected)
    {
        GameObject bestTarget = null;
        float ClosestDist = Mathf.Infinity;
        Vector3 currentPosition = ActiveInfected.transform.position;

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Civilian");
        foreach (var obj in objects)
        {
            float TargetDist = Vector3.Distance(currentPosition, obj.transform.position);
            if (TargetDist < ClosestDist)
            {
                ClosestDist = TargetDist;
                bestTarget = obj;
            }
        }

        return bestTarget;

    }

    void RushCivilian(GameObject ActiveInfected, GameObject Target)
    {
        Vector3 targetPosition = Target.transform.position;

        if (Vector3.Distance(ActiveInfected.transform.position, Target.transform.position) <= 1f)
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

    bool IsHungry(GameObject ActiveInfected)
    {
        SpriteRenderer ActiveSprite;
        ActiveSprite = ActiveInfected.transform.GetComponent<SpriteRenderer>();
        if (ActiveSprite.color.r <= .5f) { return true; }
        return false;
    }

    int m_GridWidth = 20;
    int m_GridHeight = 20;
    float m_GridCellWidthHeight = 1.0f;
    Vector3 m_Offset;

    void DrawDebugGrid()
    {
        m_Offset = -1.0f * new Vector3((m_GridWidth * 0.0f * m_GridCellWidthHeight), (m_GridHeight * 0.0f * m_GridCellWidthHeight), 0.0f);
        for (int i = 0; i <= m_GridWidth; i++)
        {
            Debug.DrawLine(new Vector3((i * m_GridWidth * m_GridCellWidthHeight), 0.0f, 0.0f) + m_Offset, new Vector3((i * m_GridWidth * m_GridCellWidthHeight), m_GridHeight * m_GridCellWidthHeight, 0.0f) + m_Offset, Color.black);
        }

        for (int i = 0; i <= m_GridHeight; i++)
        {
            Debug.DrawLine(new Vector3(0.0f, (i * m_GridHeight * m_GridCellWidthHeight), 0.0f) + m_Offset, new Vector3(m_GridWidth * m_GridCellWidthHeight * m_GridCellWidthHeight, (i * m_GridHeight), 0.0f) + m_Offset, Color.black);
        }

    }

    void GridSystem()
    {
        int xPos;
        int yPos;
        
        for (int x = 0; x <= 20; x++)
        {
            for (int y = 0; y <= 20; y++)
            {
                xPos = -100 + (x * 20);
                yPos = -100 + (y * 20);
                GridList.Add( new Vector2(xPos, yPos) );
            }
        }

        
    }

    void GridPosition(Transform ActiveCivilian)
    {
        
    }
}
