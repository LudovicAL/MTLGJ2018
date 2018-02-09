using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CiviliansSpawner : MonoBehaviour {

	public List<DifficultyParameters> m_DifficultyParameters;
	public Transform Civilian;
	[HideInInspector] public GameObject[] arrayOfCivilians;
	[HideInInspector] public GameObject[] arrayOfInfectedTargets;
	[HideInInspector] public int[] civilianGridIndex;
	[HideInInspector] public float[] humanSpeeds;
	[HideInInspector] public float[] humanHealthIndex;
	[HideInInspector] public Vector3[] humanHeadings;
	private MapReader g_MapReader;
	private int m_DifficultyCurrentLevel = 0;

	void Start() {
		g_MapReader = GameObject.Find("Map").GetComponent<MapReader>();
	}

	public void SpawnCivilians() {
		RemoveOldCivilians ();
		PlaceNewCivilians (m_DifficultyParameters[m_DifficultyCurrentLevel].m_StartingHumans);
		BuildArrayOfCivilians ();
	}

	private void RemoveOldCivilians() {
		if (arrayOfCivilians != null) {
			for (int i = 0, max = arrayOfCivilians.Length; i < max; i++) {
				Destroy (arrayOfCivilians [i]);
			}
			arrayOfCivilians = null;
		}
	}

	private void PlaceNewCivilians(int numberOfCiviliansToSpawn) {
		float yAxis = Civilian.position.y;
		float xAxis = Civilian.position.x;
		for (int i = 0; i < numberOfCiviliansToSpawn; i++) {
			float[] randomSpawnPos = g_MapReader.FindRandomWhiteSpace();
			xAxis = randomSpawnPos[0];
			yAxis = randomSpawnPos[1];
			Instantiate(Civilian, new Vector3(xAxis, yAxis, 0), Quaternion.identity);
		}
	}

	private void BuildArrayOfCivilians() {
		arrayOfCivilians = GameObject.FindGameObjectsWithTag("Civilian");
		int amountOfCivilians = arrayOfCivilians.Length;
		civilianGridIndex = new int[amountOfCivilians];
		humanSpeeds = new float[amountOfCivilians];
		humanHeadings = new Vector3[amountOfCivilians];
		arrayOfInfectedTargets = new GameObject[amountOfCivilians];
		humanHealthIndex = new float[amountOfCivilians];

		for (int i = 0; i < amountOfCivilians; ++i) {
			civilianGridIndex[i] = 0;
			humanSpeeds[i] = UnityEngine.Random.Range(m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianBaseSpeed - m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianSpeedPlusMinus, m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianBaseSpeed + m_DifficultyParameters[m_DifficultyCurrentLevel].CivilianSpeedPlusMinus);
			humanHeadings[i].x = UnityEngine.Random.Range(-1.0f, 1.0f);
			humanHeadings[i].y = UnityEngine.Random.Range(-1.0f, 1.0f);
			humanHeadings[i].z = 0.0f;
			humanHeadings [i] = Vector3.Normalize (humanHeadings [i]);
			arrayOfInfectedTargets[i] = null;
			humanHealthIndex [i] = 1.0f;
		}
	}
}
