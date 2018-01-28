using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapReader : MonoBehaviour {
	bool[,] m_Bitmap;
	List<int[]> m_WhiteSpots;
	int m_Width;
	int m_Height;
	private Texture2D m_CityTexture;
	private Texture2D m_RuntimeColoredWallTexture;


	void Awake() {

		m_CityTexture = GameObject.Find ("Map").GetComponent<SpriteRenderer> ().sprite.texture;

		m_Width = m_CityTexture.width;
		m_Height = m_CityTexture.height;
		m_Bitmap = new bool[m_Width,m_Height];
		m_WhiteSpots = new List<int[]>();

		for (int i = 0; i < m_Width; ++i) {
			for (int j = 0; j < m_Height; ++j) {
				Color color = m_CityTexture.GetPixel (i, j);
				bool isFreeSpot = color == Color.white;

				m_Bitmap [i,j] = isFreeSpot ? true : false;

				if (isFreeSpot) {
					int[] spotCoord = { i, j };
					m_WhiteSpots.Add (spotCoord); 
				}
			}
		}
	}

	void Start() {
		GameObject blankMapForWalls = GameObject.Find ("BlankMapForWalls");
		if (blankMapForWalls != null) {
			SpriteRenderer renderer = blankMapForWalls.GetComponent<SpriteRenderer> ();
			if (renderer != null) {
				m_RuntimeColoredWallTexture = (Texture2D)GameObject.Instantiate (renderer.sprite.texture);
				renderer.sprite = Sprite.Create (m_RuntimeColoredWallTexture, renderer.sprite.rect, new Vector2 (0.5f, 0.5f));
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
	}

	public float[] FindRandomWhiteSpace()
	{
		int[] whiteSpot = m_WhiteSpots[Random.Range(0, m_WhiteSpots.Count)];
		return ConvertPixelCoordToWorldCoord (whiteSpot [0], whiteSpot [1]);
	}

	public bool CanMoveThere(float _xPos, float _yPos)
	{
		int[] pixelCoords = ConvertWorldCoordToPixelCoord (_xPos, _yPos);

		if (pixelCoords[0] < 0 || pixelCoords[0] >= m_Width)
			return false;

		if (pixelCoords[1] < 0 || pixelCoords[1] >= m_Height)
			return false;

		return m_Bitmap[pixelCoords[0], pixelCoords[1]];
	}

	int[] ConvertWorldCoordToPixelCoord(float _xPos, float _yPos)
	{
		int[] result = new int[2];
		result[0] = (int)((m_Width / 2.0f) + _xPos*100.0f);
		result[1] = (int)((m_Height / 2.0f) + _yPos*100.0f); 
		return result;
	}

	float[] ConvertPixelCoordToWorldCoord(int _pixelX, int _pixelY)
	{
		float[] result = new float[2];
		result[0] = (_pixelX - (m_Width/2.0f))/100.0f;
		result[1] = (_pixelY - (m_Height/2.0f))/100.0f;
		return result;
	}

	public Texture2D LoadPNG(string _filePath)
	{
		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists(_filePath))     {
			fileData = File.ReadAllBytes(_filePath);
			tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}

	public void AddWall(List<Vector2> _wallCoords)
	{			
		for (int i = 1; i < _wallCoords.Count; ++i) {
			Vector2 wallCoordA = _wallCoords [i-1];
			Vector2 wallCoordB = _wallCoords [i];

			Vector2 directionVector = wallCoordB - wallCoordA;
			Vector2 leftVector = new Vector2(-directionVector.y, directionVector.x);
			leftVector.Normalize ();
			Vector2 rightVector = new Vector2(directionVector.y, -directionVector.x);
			rightVector.Normalize ();

			QueueWallPixels (wallCoordA, wallCoordB);

			for (int extraWallIndex = 0; extraWallIndex < 25; ++extraWallIndex)
			{
				float extraWallOffset = 0.0025f * (float)extraWallIndex;
				QueueWallPixels (wallCoordA + (leftVector * extraWallOffset), wallCoordB + (leftVector * extraWallOffset));
				QueueWallPixels (wallCoordA + (rightVector * extraWallOffset), wallCoordB + (rightVector * extraWallOffset));
			}

			PushWallPixels ();
		}
	}

	public void QueueWallPixels(Vector2 wallCoordA, Vector2 wallCoordB)
	{
		int[] pixelCoordA = ConvertWorldCoordToPixelCoord (wallCoordA [0], wallCoordA [1]);
		int[] pixelCoordB = ConvertWorldCoordToPixelCoord (wallCoordB [0], wallCoordB [1]);
		QueueWallPixelsBetweenPoints_Bresenham (pixelCoordA, pixelCoordB);

		//bool wasAWall = m_Bitmap [pixelCoord [0], pixelCoord [1]] == false;
		//if (wasAWall) break into smaller walls
		//m_Bitmap [pixelCoord [0], pixelCoord [1]] = false; // update unwalkable

		//Debug.DrawLine(_wallCoords[i-1], _wallCoords[i]);
		//Debug.Log ("wall coords: " + _wallCoords[i-1].ToString("F4") + ", " + _wallCoords[i].ToString("F4"));

		//Debug.Log ("pixel coords: (" + pixelCoordA[0] + ", " + pixelCoordA[1] + "), (" + pixelCoordB[0] + ", " + pixelCoordB[1] + ")");

	}

	void QueueWallPixelsBetweenPoints_Bresenham(int[] _startPixelCoord, int[] _endPixelCoord)
	{
		// clamp outside points
		_startPixelCoord [0] = Mathf.Clamp (_startPixelCoord [0], 0, m_Width-1);
		_startPixelCoord [1] = Mathf.Clamp (_startPixelCoord [1], 0, m_Height-1);
		_endPixelCoord [0] = Mathf.Clamp (_endPixelCoord [0], 0, m_Width-1);
		_endPixelCoord [1] = Mathf.Clamp (_endPixelCoord [1], 0, m_Height-1);

		float dx = Mathf.Abs (_endPixelCoord [0] - _startPixelCoord [0]);
		float dy = Mathf.Abs (_endPixelCoord [1] - _startPixelCoord [1]);

		int x0 = _startPixelCoord [0];
		int y0 = _startPixelCoord [1];
		int x1 = _endPixelCoord [0];
		int y1 = _endPixelCoord [1];

		int x = x0;
		int y = y0;

		int sx = x0 > x1 ? -1 : 1;
		int sy = y0 > y1 ? -1 : 1;

		if (dx > dy) { 
			float err = dx / 2.0f;
			while (x != x1) {
				m_Bitmap [x, y] = false;
				m_RuntimeColoredWallTexture.SetPixel ((int)x, (int)y, Color.yellow);
						
				err -= dy;
				if (err < 0) {
					y += sy;
					err += dx;
				} 
				x += sx;
			}
		}
		else 
		{
			float err = dy / 2.0f;
			while (y != y1) {
				m_Bitmap [x, y] = false;
				m_RuntimeColoredWallTexture.SetPixel ((int)x, (int)y, Color.yellow);
				err -= dx;
				if (err < 0) {
					x += sx;
					err += dy;
				}
				y += sy;
			}
		}

		m_Bitmap [x1, y1] = false;
		m_RuntimeColoredWallTexture.SetPixel ((int)x1, (int)y1, Color.yellow);
	}

	void PushWallPixels()
	{
		m_RuntimeColoredWallTexture.Apply();
	}
}


