using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallConstructor : MonoBehaviour {

	private List<Vector2> coordList;
	private bool m_WallStarted;
	private bool m_FoundStartingPoint;
	private bool m_WallBuilding;
	private bool m_WallAlmostFinished;
	private bool m_WallFinished;
	private LineRenderer hollowLine;
	private Vector3 m_PreviousCoord;
	private LineRenderer m_BadLine;
	private GameObject m_MapReaderObject;

	// Use this for initialization
	void Start () {
		hollowLine = GameObject.Find("Line").GetComponent<LineRenderer>();
		m_BadLine = GameObject.Find("BadLine").GetComponent<LineRenderer>();
		coordList = new List<Vector2> ();
		m_WallStarted = false;
		m_FoundStartingPoint = false;
		m_WallBuilding = false;
		m_WallAlmostFinished = false;
		m_WallFinished = false;
		m_MapReaderObject = GameObject.Find ("Map");
	}

	public void WallBegan(Vector2 coord) {
		m_WallStarted = true;
		m_FoundStartingPoint = false;
		m_WallBuilding = false;
		m_WallAlmostFinished = false;
		m_WallFinished = false;
		m_BadLine.positionCount = 0;
		m_BadLine.enabled = true;

		Vector3 convertedCoord = Camera.main.ScreenToWorldPoint (coord);
		if (!m_MapReaderObject.GetComponent<MapReader> ().CanMoveThere (convertedCoord [0], convertedCoord [1])) 
		{
			m_PreviousCoord = convertedCoord;
			m_PreviousCoord.z = 0;
			m_FoundStartingPoint = true;

			++m_BadLine.positionCount;
			m_BadLine.SetPosition (m_BadLine.positionCount - 1, convertedCoord);
		}
	}

	public List<Vector2> getCoordList() {
		return coordList;
	}

	//Player kept tracing a wall in the current frame
	public void WallMoved(Vector2 coord) {
		if (m_WallStarted && !m_WallFinished) 
		{
			if (!m_WallBuilding) 
			{
				TryAddWallCoordinate (coord); 
			} 
			else 
			{
				//if (Time.time > nextUpdateTime) 
				//{
				TryAddWallCoordinate (coord); 
				//}
			}
		}
	}

	//Player stopped tracing a wall in the current frame
	public void WallEnded(Vector2 coord) {
		//if (buildingWall) {
		DrawWall ();
		WallCanceled ();	//This is not actually cancelling the wall as it has been constructed already. It just resets the variables.
		//}
	}

	//Player canceled his request to trace a wall in the current frame
	public void WallCanceled() {
		coordList.Clear ();
		hollowLine.enabled = false;
		hollowLine.positionCount = 0;
		m_BadLine.enabled = false;
		m_BadLine.positionCount = 0;
	}

	private void TryAddWallCoordinate(Vector2 coord) {
		if (m_WallFinished)
			return;

		Vector3 convertedCoord = Camera.main.ScreenToWorldPoint (coord);
		convertedCoord.z = 0;

		if (!m_MapReaderObject.GetComponent<MapReader> ().CanMoveThere (convertedCoord [0], convertedCoord [1])) {
			if (!m_FoundStartingPoint) {
				m_FoundStartingPoint = true;
			}
			if (m_WallBuilding) {
				m_WallAlmostFinished = true;
			} else {
				m_PreviousCoord = convertedCoord;
			}
			if (m_WallAlmostFinished) {
				m_WallFinished = true;
			}
		} else {
			if (m_FoundStartingPoint) {
				if (!m_WallBuilding) {
					hollowLine.positionCount = 0;
					hollowLine.enabled = true;
					m_WallBuilding = true;
					coordList.Add(m_PreviousCoord);
					++hollowLine.positionCount;
					hollowLine.SetPosition (hollowLine.positionCount - 1, m_PreviousCoord);
				} 
			}
		}

		if (m_WallBuilding) {
			coordList.Add (convertedCoord);
			//nextUpdateTime = Time.time + updateGap;
			++hollowLine.positionCount;
			hollowLine.SetPosition (hollowLine.positionCount - 1, convertedCoord);
		} else {
			++m_BadLine.positionCount;
			m_BadLine.SetPosition (m_BadLine.positionCount - 1, convertedCoord);
		}
	}

	//A wall has to be drawn
	private void DrawWall() {
		if (m_MapReaderObject == null) {
			return;
		}
		m_MapReaderObject.GetComponent<MapReader> ().AddWall (coordList);
	}
}
