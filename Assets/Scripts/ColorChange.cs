using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColorChange : MonoBehaviour
{

    
    Color CurrentColor;
    Color NewColor;
    float Red, Blue, Green;
    public Transform Civilian;
    float InfectedSpeed = 3f;
    float CivilianSPeed = 1.5f;

    // Use this for initialization
    void Start()
    {
        //for (int i = 0; i < 10; i++)
        //{
        //    Transform newCivilian = Instantiate(Civilian, new Vector3(i * 2.0F, 0, 0), Quaternion.identity);
        //    newCivilian.tag = "Civilian";
        //}
    }

    // Update is called once per frame
    void Update()
    {

        string[] ActiveChar = new string[] { "Civilian", "Infected" };
        for (int i = 0; i <= 1; i++)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(ActiveChar[i]);
            foreach (GameObject obj in objects)
            { 
                switch (i)
                {
                    case 0:
                       
                        break;
                    case 1:
                        ZombieDecay(obj);
                        GameObject target = GetClosestCivilian(obj);
                        if (target != null && IsHungry(obj)) { RushCivilian(obj, target); }
                        
                        break;
                }
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

        print("Red : " + Red + "  Green : " + ActiveSprite.color.g + "  Blue : " + Blue);

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

        if (Vector3.Distance(ActiveInfected.transform.position,Target.transform.position) <= 1f)
        {
            
            Target.tag = "Infected";
            print(Target.name);
            print(Target.name);
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

}
