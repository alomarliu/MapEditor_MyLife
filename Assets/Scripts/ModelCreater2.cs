/** ========================================
 *  此為XML載 prefab 版本
 * 
 * 
 * 
 * =========================================*/

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class ModelCreater2 : EditorWindow 
{
	private const int MAX_TAB				= 3;
	private static int selItem 				= 0;
	private static GameObject mapManager 	= null;
	
	private Texture2D image;
	
	/** Enable 時載入的 prefab */
	private UnityEngine.Object[] _prefabTemp;
	
	private Vector2 mPos			 		= Vector2.zero;
	private int _tab 						= 0;
	private BetterList<Item> _itemList 		= new BetterList<Item>();
	private GUIContent[] _content;

	// 比例
	private Vector3 scale;
	/** 選擇項目的style */
	private GUIStyle _itemStyle;
	/** 選擇項目的style */
	private GUIStyle _toolTipStype;
	
	/**=============================================
	 * 啟動時
	 * ===========================================*/
	void OnEnable()
	{		
		Debug.Log("OnEnable");
		_itemStyle = new GUIStyle("Button");
		_itemStyle.fixedHeight = 50;
		_itemStyle.fixedWidth = 50;

		_toolTipStype = new GUIStyle();
		_toolTipStype.normal.background = UnityEditor.EditorGUIUtility.whiteTexture;
		_toolTipStype.alignment = TextAnchor.MiddleCenter;

		mapManager = GameObject.Find("MapManager");
		
		if(!mapManager)
		{
			mapManager = new GameObject();
			mapManager.name = "MapManager";
		}

		MapManager.instance.mainObj = mapManager;

		// 初始化組件
		initComponent();

		MapManager.instance.CreateLayer();

		// 載入prefab
		LoadAllPrefab();
		prepareTexture ();

		// 加入Scene事件
		SceneView.onSceneGUIDelegate += MapManager.instance.OnSceneGUI; 
	}
	
	/**=============================================
	 * 初始化組件
	 * ===========================================*/
	void initComponent()
	{
		CameraConfig.adjustOrthographicSize();

		if( mapManager.GetComponent<Grid>() == null ) 
		{ 
			MapManager.instance.grid = mapManager.AddComponent<Grid>(); 
		} 

		// 攝影機設定
		if( mapManager.GetComponent<CameraConfig>() == null ) 
		{ 
			mapManager.AddComponent<CameraConfig>(); 
		} 
				
		GameObject detectPlane = GameObject.Find("DetectPlane");
		
		if(!detectPlane)
		{
			GameObject preGrid = Resources.Load("DetectPlane") as GameObject;
			detectPlane = GameObject.Instantiate(preGrid) as GameObject;
			detectPlane.name = "DetectPlane";
			//detectPlane.transform.parent = mapManager.transform;
			detectPlane.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			//SpriteRenderer sr = detectPlane.GetComponent<SpriteRenderer>();
			//sr.sortingOrder = 9999;
		}

		MapManager.instance.detectPlane = detectPlane;
	}

	/**=============================================
	 * 載入Resources/Prefabs/的所有prefab  
	 * ===========================================*/
	void LoadAllPrefab ()
	{
		string path = "Prefabs";
		_prefabTemp = Resources.LoadAll(path, typeof(GameObject));
	}
	
	/**=============================================
	 * EditorWindow 結束時
	 * ===========================================*/
	void OnDisable()
	{
		Debug.Log("OnDisable");
		// 釋放
		foreach (Item item in _itemList) DestroyTexture(item);
		_itemList.Clear();

		GameObject.DestroyImmediate(MapManager.instance.detectPlane);
		MapManager.instance.detectPlane = null;
		// 移除Scene事件
		SceneView.onSceneGUIDelegate -= MapManager.instance.OnSceneGUI; 

		MapManager.instance.clearGridData();

		GameObject.DestroyImmediate(mapManager);
		mapManager = null;
	}
	
	/**=============================================
	 * 釋放紋理
	 * ===========================================*/
	void DestroyTexture (Item item)
	{
		if (item != null && item.tex != null)
		{
			GameObject.DestroyImmediate(item.tex);
			item.tex = null;
		}
	}

	/**=============================================
	 * 準備貼圖
	 * ===========================================*/
	void prepareTexture ()
	{
		if(_tab >= _prefabTemp.Length)
			return;

		// 取出 atlas prefab
		GameObject go = _prefabTemp[_tab] as GameObject;
		UIAtlas atlas = go.GetComponent<UIAtlas>();
		BetterList<string> spriteList = atlas.GetListOfSprites();
		UISpriteData sData;
		String sName = "";
		Color[] color;
		Texture2D oriTxt;
		Texture2D tarTxt;
		Item item;

		_itemList.Clear();

		int size = spriteList.size;
		_content = new GUIContent[size];

		for(int i = 0; i < spriteList.size; ++i)
		{
			sName = spriteList[i];
			
			if(String.IsNullOrEmpty(sName))
				continue;
			
			sData = atlas.GetSprite(sName);			

			oriTxt = atlas.texture as Texture2D;
			color = oriTxt.GetPixels(sData.x, oriTxt.height - sData.height - sData.y, sData.width, sData.height);
			tarTxt = new Texture2D(sData.width, sData.height);
			
			tarTxt.SetPixels(color);
			tarTxt.Apply();

			item = new Item();
			item.tex = tarTxt;
			item.guid = sName;
			item.unit = Mathf.CeilToInt((float)tarTxt.width / CameraConfig.pixelsPerUnit);
			_itemList.Add(item);

			_content[i] = new GUIContent(tarTxt, sName);
		}
	}
	
	/**=============================================
	 * 準備貼圖
	 * ===========================================*/
	void OnGUI()
	{		
		Event currentEvent = Event.current;

		if(_prefabTemp == null || _prefabTemp.Length <= 0)
			return;

		GUILayout.BeginVertical();
		{
			// 選擇要編輯的layer
			string[] str = initLayerName();
			MapManager.instance.nowLayer =  EditorGUILayout.Popup(MapManager.instance.nowLayer, str);

			// 選取prefab的頁籤
			GUILayout.BeginHorizontal();
			{
				int newTab = _tab;
				int curLen = (_prefabTemp != null)? _prefabTemp.Length : 0;
				int maxLen = Mathf.Max(MAX_TAB, curLen);
				String btnType = "ButtonLeft";
				
				for(int i = 0; i < maxLen; ++i)
				{					
					btnType = (i == 0)? "ButtonLeft" : ((i == maxLen-1)? "ButtonRight" : "ButtonMid");
					
					if (GUILayout.Toggle(newTab == i, (i+1).ToString(), btnType)) 
						newTab = i;
				}
				
				// 頁籤有切換
				if (_tab != newTab)
				{
					_tab = newTab;
					// 準備貼圖
					prepareTexture();
				}
			}			
			GUILayout.EndHorizontal();

			int HCellCount = Mathf.FloorToInt( (Screen.width - 12) / 54.0f);
			//int oldSel = 0;

			GUILayout.BeginHorizontal();
			{
				mPos = GUILayout.BeginScrollView(mPos);
				{
					//oldSel = selItem;
					selItem = GUILayout.SelectionGrid(selItem, _content, HCellCount, _itemStyle);

					MapManager.instance.selectItem = _itemList[selItem];
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		if(GUI.tooltip != "")
		{
			Vector2 size = _toolTipStype.CalcSize(new GUIContent(GUI.tooltip));
			Vector2 pos = currentEvent.mousePosition;
			pos.x -= size.x * 0.5f;
			pos.y += 10f;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Label(new Rect(pos.x, pos.y, size.x + 10, size.y + 10), GUI.tooltip, _toolTipStype);
		}
	}

	string[] initLayerName()
	{
		string[] str = new string[MapManager.MAX_LAYERCOUNT];

		for(int i = 0; i < MapManager.MAX_LAYERCOUNT; ++i)
		{
			str[i] = "Layer" + (i+1);
		}

		return str;
	}
}