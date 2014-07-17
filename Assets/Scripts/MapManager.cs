using UnityEngine;
using System.Collections;
using System.Xml;
using UnityEditor;
using System.Collections.Generic;

public class MapManager 
{
	public GameObject[] layers = new GameObject[MAX_LAYERCOUNT];

	/** singleton */
	private static MapManager _instance  	= null;
	/** 格線類別 */
	public Grid grid;
	/** 選擇的項目 */
	public Item selectItem;
	/** 最大物件層級 */
	public const int MAX_LAYERCOUNT 		= 3;
	/** 目前層級 */
	public int nowLayer						= 0;

	private int _lastCellCount				= 10;

	private Vector3 putPosition 			= new Vector3(0,0,0);
	/** 感應區 */
	public GameObject detectPlane;

	private Dictionary<int, GridData[,]> _gridDic = new Dictionary<int, GridData[,]>();
	//private GridData[,] _gridData;

	/** manager 主 object */
	public GameObject mainObj 				= null;

	/** getter */
	public static MapManager instance
	{
		get
		{
			if(_instance == null)
				_instance = new MapManager();

			return _instance;
		}
	}
	
	/**=============================================
	 * 清除階層物件
	 * ===========================================*/
	public void ClearLayer()
	{
		int childCount = mainObj.transform.childCount;
		Transform child;
		
		// 刪除物件
		while(childCount > 0)
		{
			child = mainObj.transform.GetChild(0) as Transform;
			
			if(child)
				GameObject.DestroyImmediate(child.gameObject);

			childCount = mainObj.transform.childCount;
		}

		_gridDic.Clear();
	}
	
	/**=============================================
	 * 建立階層物件
	 * ===========================================*/
	public void CreateLayer()
	{
		string layerName = "Layer";
		GameObject layer;
		GridData[,] gridData;
		
		// 清除階層物件
		ClearLayer();

		for(int j = 0; j < MAX_LAYERCOUNT; ++j)
		{
			layer = GameObject.Find(layerName + (j+1));
			
			if(!layer)
			{
				layer = new GameObject();
				layer.transform.parent = mainObj.transform;
				layer.name = layerName + (j+1);
			}
			
			layers[j] = layer;
			
			// 改變資料結構大小
			gridData = new GridData[grid.cellCount,grid.cellCount];
			_gridDic[j] = gridData;
		}
	}


	/**=============================================
	 * 
	 * ===========================================*/
	public void OnSceneGUI( SceneView sceneView ) 
	{ 
		Event e = Event.current;

		if(selectItem == null)
			return;

		// cell 格數有改變
		if(_lastCellCount != grid.cellCount)
		{
			// 建立階層
			CreateLayer();

			_lastCellCount = grid.cellCount;
		}

		if(e.type == EventType.MouseDown && e.button == 0 ) 
		{ 
			e.Use();
			putObject();
		}
		else if(e.type == EventType.MouseMove)
		{
			e.Use();
			
			//Texture2D txt = selectItem.tex as Texture2D;
			Vector3 mousePos = e.mousePosition;
			Camera mainCam = Camera.current;
			mousePos.y = mainCam.pixelHeight - mousePos.y;
			Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
			worldPos.z = 0;

			// 計算貼圖共幾單位長
			int unit = (e.alt)? 1 : selectItem.unit;
			// 是否為偶數單位長
			bool even = unit % 2 == 0;	
			float halfUnitX = Grid.cellUnitX * 0.5f;
			float halfUnitY = Grid.cellUnitY * 0.5f;
			float unitX1 = (even)? Mathf.FloorToInt(worldPos.x - halfUnitX) : Mathf.FloorToInt(worldPos.x);
			float unitX2 = unitX1 + 1f;
			float unitY1 = Mathf.FloorToInt(worldPos.y / Grid.cellUnitY) * Grid.cellUnitY;
			float unitY2 = unitY1 + Grid.cellUnitY;
			Vector3 start1 = new Vector3(unitX1, unitY1);
			Vector3 end1 = new Vector3(unitX2, unitY2);
			Vector3 start2 = new Vector3(unitX1, unitY2);
			Vector3 end2 = new Vector3(unitX2, unitY1);

			// 偶數
			if(even)
			{
				start1.x += halfUnitX;
				end1.x += halfUnitX;
				start2.x += halfUnitX;
				end2.x += halfUnitX;
			}

			// 奇數
			//else
			{				
				Vector3 n1 = Vector3.Normalize(end1 - start1);
				Vector3 n2 = Vector3.Normalize(end2 - start2);
				Vector3 targetn1 = Vector3.Normalize(worldPos - start1);
				Vector3 targetn2 = Vector3.Normalize(worldPos - start2);

				// down
				if(targetn1.x > n1.x && targetn2.x < n2.x)
				{
					worldPos.x = start1.x + halfUnitX;
					worldPos.y = start1.y;
				}
				// right
				else if(targetn1.x > n1.x && targetn2.x > n2.x)
				{
					worldPos.x = end1.x;
					worldPos.y = start1.y + halfUnitY;
				}
				// left
				else if(targetn1.x < n1.x && targetn2.x < n2.x)
				{
					worldPos.x = start1.x;
					worldPos.y = start1.y + halfUnitY;
				}			
				// up
				else if(targetn1.x < n1.x && targetn2.x > n2.x)
				{
					worldPos.x = start1.x + halfUnitX;
					worldPos.y = start2.y;
				}
			}

			detectPlane.transform.position = worldPos; 
			detectPlane.transform.localScale = new Vector3(unit, unit);

			putPosition = worldPos;
			
			SpriteRenderer sr = detectPlane.GetComponent<SpriteRenderer>();

			// 轉換為陣列索引
			Vector3 aryIdx = transferPosToArrayIndex(putPosition, unit);

			if(checkInBound(aryIdx, unit))
				sr.color = new Color(0,1f,0,0.6f);
			else
				sr.color = new Color(1f,0,0,0.6f);

			// 有按下ctrl
			if(e.control)
			{
				putObject();
			}
			// 按下 alt
			else if(e.alt)
			{
				sr.color = new Color(1f,1f,1f,0.6f);
				deleteObject();
			}
		}
				
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.fontSize = 20;

		Handles.BeginGUI();	
		GUILayout.Label("按住Ctrl可連續建立物件, 按住Alt可刪除物件", style); 

		Handles.EndGUI();
	}
	
	/**=============================================
	 * 放物件
	 * ===========================================*/
	void putObject()
	{		
		GameObject layer = layers[nowLayer];
		
		Texture2D txt = selectItem.tex as Texture2D;
		
		if(!txt)
			return;
		
		// 讓紋理依滑鼠位置水平置中
		int unit = selectItem.unit;
		// 轉換為陣列索引
		Vector3 aryIdx = transferPosToArrayIndex(putPosition, unit);
		
		// 不在允許範圍內
		if(!checkInBound(aryIdx, unit))
			return;
		
		Sprite ss = Sprite.Create(txt, new Rect(0,0,txt.width, txt.height), new Vector2(0,0));
		
		GameObject prefab = Resources.Load("sprite") as GameObject;
		GameObject tt = GameObject.Instantiate(prefab) as GameObject;
		SpriteRenderer sr = tt.GetComponent<SpriteRenderer>();
		sr.sprite = ss;

		putPosition.x -= (float)txt.width * 0.5f / CameraConfig.pixelsPerUnit;
		putPosition.y -= (float)unit * 0.5f * Grid.cellUnitY;
		
		tt.transform.parent = layer.transform;
		tt.transform.position = putPosition; 
		//tt.transform.hideFlags = HideFlags.NotEditable;
		tt.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;
		sr.sortingLayerID = nowLayer+1;
		sr.sortingOrder = grid.cellCount * (int)aryIdx.x  + (int)aryIdx.y;// + nowLayer * grid.cellCount * grid.cellCount;
		
		// 設定 grid 資料
		setGridData(aryIdx, unit, selectItem.guid, tt);
	}
	
	/**=============================================
	 * 座標轉為成陣列索引
	 * ===========================================*/
	Vector3 transferPosToArrayIndex(Vector3 pos, int unit)
	{
		float x = 0;
		float y = 0;

		Vector3 unitSize = grid.GetTotalUnitSize();

		// 乘上pixelsPerUnit 只是為了方便計算, 比較不會有浮點數的困擾
		x = Mathf.Floor(pos.x * CameraConfig.pixelsPerUnit + x);
		x = x /(float)grid.cellSizeX;
		x = Mathf.Floor(x * 2f);

		// 乘上pixelsPerUnit 只是為了方便計算, 比較不會有浮點數的困擾
		y = unitSize.y * CameraConfig.pixelsPerUnit * 0.5f;
		y = Mathf.Floor(pos.y * CameraConfig.pixelsPerUnit - y);
		y = y / (float)grid.cellSizeY;
		y = Mathf.Abs (y);
		y = Mathf.Floor(y * 2f);
		y = Mathf.Max(0, y - 1f);

		float finalX = (y-x)/2;
		float finalY = y-finalX;

		// 選擇的貼圖單位長度是否為偶數
		bool even = unit % 2 == 0;
		int halfUnit = Mathf.FloorToInt(unit / 2f);

		if(even)
		{
			finalX = Mathf.Ceil(finalX) - halfUnit;
			finalY = Mathf.Ceil(finalY) - halfUnit;
		}
		else
		{
			finalX -= halfUnit;
			finalY -= halfUnit;
		}

		return new Vector3(finalX, finalY);
	}
	
	/**=============================================
	 * 檢查是否在範圍內
	 * @param aryIdx 陣列索引 {aryIdx.x = row, aryIdx.y = col}
	 * @param unit 貼圖所佔單位長
	 * ===========================================*/
	bool checkInBound(Vector3 aryIdx, int unit)
	{
		if(aryIdx.x < 0 || aryIdx.y < 0)
			return false;

		// 單位長有誤
		if(unit <= 0)
			return false;

		int maxUnit = grid.cellCount;

		// 超出邊界
		if(aryIdx.x + unit > maxUnit || aryIdx.y + unit > maxUnit)
			return false;
		
		int size = unit * unit;
		int row = 0;
		int col = 0;
		GridData[,] gridAry = _gridDic[nowLayer];
		GridData data;
		
		for(int i = 0; i < size; ++i)
		{			
			row = Mathf.FloorToInt(i / unit) + (int)aryIdx.x;
			col = i % unit + (int)aryIdx.y;
			data = gridAry[row,col];

			// 該格有物件
			if(data != null)
				return false;
		}

		return true;
	}
	
	/**=============================================
	 * 清除物件
	 * ===========================================*/
	void deleteObject()
	{
		// 轉換為陣列索引
		Vector3 aryIdx = transferPosToArrayIndex(putPosition, 1);

		if (aryIdx.x < 0 || aryIdx.y < 0)
			return;

		if (aryIdx.x >= grid.cellCount || aryIdx.y >= grid.cellCount)
			return;

		int row = 0;
		int col = 0;
		
		GridData[,] gridAry = _gridDic[nowLayer];
		GridData data = gridAry[(int)aryIdx.x, (int)aryIdx.y];

		if(data == null)
			return;

		data = gridAry[(int)data.baseAryIdx.x, (int)data.baseAryIdx.y];

		if(data == null)
			return;

		int unit = data.unit;
		int size = unit * unit;
		GridData temp;

		for(int i = 0; i < size; ++i)
		{			
			row = Mathf.FloorToInt(i / unit) + (int)data.baseAryIdx.x;
			col = i % unit + (int)data.baseAryIdx.y;

			temp = gridAry[row,col];

			// 該格有物件
			if(temp != null)
			{
				gridAry[row,col] = null;

				if(temp.obj != null)
				{
					GameObject.DestroyImmediate(temp.obj);
					temp.obj = null;
				}
			}
		}
	}

	/**=============================================
	 * 設定地圖資料
	 * ===========================================*/
	bool setGridData(Vector3 aryIdx, int unit, string objID, GameObject obj)
	{
		if(obj == null)
			return false;

		// 不在允許範圍內
		if(!checkInBound(aryIdx, unit))
			return false;

		GridData data;
		int size = unit * unit;
		int row = 0;
		int col = 0;
		Vector3 baseIdx = new Vector3();		
		GridData[,] gridAry = _gridDic[nowLayer];

		for(int i = 0; i < size; ++i)
		{
			data = new GridData();

			data.objID = objID;
			// 第一格才要開旗標
			data.build = i == 0;
			data.unit = unit;

			row = Mathf.FloorToInt(i / unit) + (int)aryIdx.x;
			col = i % unit + (int)aryIdx.y;

			gridAry[row,col] = data;

			if(i == 0) baseIdx = new Vector3((float)row, (float)col);
			data.baseAryIdx = baseIdx;
			data.obj = obj;
		}

		return true;
	}

	public void clearGridData()
	{
		_gridDic.Clear();
	}
}