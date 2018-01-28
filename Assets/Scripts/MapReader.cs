using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapReader : MonoBehaviour {
	bool[,] m_Bitmap;
	List<int[]> m_WhiteSpots;
	int m_Width;
	int m_Height;
	public Texture2D MapTexture; 

	void Awake() {
		//string filePath = Application.dataPath + "/Arts/ProtoCity_01.png";
		//Texture2D tex = LoadPNG (filePath);
		//Texture2D tex = Resources.Load ("Maps/ProtoCity_01.png", typeof(Texture2D));

		if (MapTexture == null) {
			Debug.Log ("Could not find texture, will not load map.");
			return;
		}

		m_Width = MapTexture.width;
		m_Height = MapTexture.height;
		m_Bitmap = new bool[m_Width,m_Height];
		m_WhiteSpots = new List<int[]>();

		for (int i = 0; i < m_Width; ++i) {
			for (int j = 0; j < m_Height; ++j) {
				Color color = MapTexture.GetPixel (i, j);
				bool isFreeSpot = color == Color.white;

				m_Bitmap [i,j] = isFreeSpot ? true : false;

				if (isFreeSpot) {
					int[] spotCoord = { i, j };
					m_WhiteSpots.Add (spotCoord); 
				}
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
		int pixelPosX = (int)((m_Width / 2.0f) + _xPos*100.0f);
		int pixelPosY = (int)((m_Height / 2.0f) + _yPos*100.0f); 

		//Debug.Log ("Got " + _xPos + ", " + _yPos + " converted to pixels " + pixelPosX + ", " + pixelPosY);

		if (pixelPosX < 0 || pixelPosX >= m_Width)
			return false;

		if (pixelPosY < 0 || pixelPosY >= m_Height)
			return false;

		return m_Bitmap[pixelPosX, pixelPosY];
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
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}

}
