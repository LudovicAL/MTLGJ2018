using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapReader : MonoBehaviour {
	bool[,] m_Bitmap;
	int m_Width;
	int m_Height;

	void Start() {
		string filePath = Application.dataPath + "/Arts/ProtoCity_01.png";
		Texture2D tex = LoadPNG (filePath);

		if (tex == null) {
			Debug.Log ("Could not find texture, will not load map.");
			return;
		}

		m_Width = tex.width;
		m_Height = tex.height;
		m_Bitmap = new bool[m_Width,m_Height];

		for (int i = 0; i < m_Width; ++i) {
			for (int j = 0; j < m_Height; ++j) {
				Color color = tex.GetPixel (i, j);
				m_Bitmap [i,j] = color == Color.white ? true : false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
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
