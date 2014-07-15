using UnityEngine;
using System.Collections;

public class CameraConfig : MonoBehaviour 
{
	/** 每單位幾 pixels */
	private static float _pixelsPerUnit = 100f;

	public static float pixelsPerUnit 
	{
		get 
		{
			return _pixelsPerUnit;
		}
	}

	void Awake()
	{		
		// 調整 Camera
		//adjustOrthographicSize();
	}

	public static void adjustOrthographicSize()
	{			
		float unitsPerPixel;
		unitsPerPixel = 1f / pixelsPerUnit;

		// 正投影大小
		Camera.main.orthographicSize = (Screen.height / 2f) * unitsPerPixel;
	}
}
