using UnityEngine; 
using System.Collections; 

public class Grid : MonoBehaviour { 
	
	public bool displayGrid = true; 

	public int cellCount = 10;
	public int cellSizeX = 100;
	public int cellSizeY = 58;

	/** 一格佔幾單位X */
	public static  float cellUnitX = 1f;
	/** 一格佔幾單位Y */
	public static float cellUnitY = 1f;

	public Color gridLineColor = new Color(0.7f,0.7f,0.7f); 

	/** 原點 */
	private Vector3 _screenOrigin;
		
	/**=============================================
	 * 計算總單位大小
	 *============================================*/
	public Vector3 GetTotalUnitSize()
	{
		float x = cellUnitX * (float)cellCount;
		float y = cellUnitY * (float)cellCount;

		return new Vector3(x, y);
	}
	
	void OnDrawGizmos() 
	{ 
		if( displayGrid ) 
		{ 						
			cellUnitX = (float)cellSizeX / CameraConfig.pixelsPerUnit;
			cellUnitY = (float)cellSizeY / CameraConfig.pixelsPerUnit;

			Gizmos.color = gridLineColor; 

			Camera camera = Camera.main;

			// 記錄畫面原點
			_screenOrigin = camera.WorldToScreenPoint(new Vector3(0,0,0));

			Vector2 totalSize = new Vector2(cellCount * cellSizeX, cellCount * cellSizeY);
			Vector2 distance = new Vector2((float)totalSize.x * 0.5f, (float)totalSize.y * 0.5f);	
			Vector3 oriPos = new Vector3(_screenOrigin.x, _screenOrigin.y);
			oriPos += new Vector3(0, totalSize.y * 0.5f);
			Vector3 oriWorldPos = camera.ScreenToWorldPoint(oriPos);
			oriWorldPos.z = 0;
			Vector3 tarPos = new Vector3((float)totalSize.x * 0.5f, -(float)totalSize.y * 0.5f);
			tarPos.x = oriPos.x + distance.x;
			tarPos.y = oriPos.y - distance.y;
			Vector3 tarWorldPos = camera.ScreenToWorldPoint(tarPos);
			tarWorldPos.z = 0;
			Gizmos.DrawLine(oriWorldPos, tarWorldPos);

			for(int i = 0; i < cellCount; ++i)
			{
				oriPos.x -= (float)cellSizeX * 0.5f;
				oriPos.y -= (float)cellSizeY * 0.5f;
				oriWorldPos = camera.ScreenToWorldPoint(oriPos);
				oriWorldPos.z = 0;
				tarPos.x = oriPos.x + distance.x;
				tarPos.y = oriPos.y - distance.y;
				tarWorldPos = camera.ScreenToWorldPoint(tarPos);
				tarWorldPos.z = 0;
				// 根據camera 像素大小偏移至中心點
				Gizmos.DrawLine(oriWorldPos, tarWorldPos);
			}
			
			oriPos = new Vector3(_screenOrigin.x, _screenOrigin.y);
			oriPos += new Vector3(0, totalSize.y * 0.5f);
			oriWorldPos = camera.ScreenToWorldPoint(oriPos);
			oriWorldPos.z = 0;
			tarPos.x = oriPos.x - distance.x;
			tarPos.y = oriPos.y - distance.y;
			//tarPos += new Vector3(0, totalSize.y * 0.5f);

			tarWorldPos = camera.ScreenToWorldPoint(tarPos);
			tarWorldPos.z = 0;
			
			Gizmos.DrawLine(oriWorldPos, tarWorldPos);
			
			for(int i = 0; i < cellCount; ++i)
			{
				oriPos.x += (float)cellSizeX * 0.5f;
				oriPos.y -= (float)cellSizeY * 0.5f;
				oriWorldPos = camera.ScreenToWorldPoint(oriPos);
				oriWorldPos.z = 0;
				tarPos.x = oriPos.x - distance.x;
				tarPos.y = oriPos.y - distance.y;
				tarWorldPos = camera.ScreenToWorldPoint(tarPos);
				tarWorldPos.z = 0;
				// 根據camera 像素大小偏移至中心點
				Gizmos.DrawLine(oriWorldPos, tarWorldPos);
			}
		} 
	} 
} 