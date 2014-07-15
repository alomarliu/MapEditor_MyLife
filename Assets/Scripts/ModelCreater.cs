using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Xml;
using System;

public class ModelCreater : EditorWindow 
{
	private static int selItem = 0;
	private static GameObject mapManager = null;
	
	private XmlDocument xml;
	private XmlNodeList xmlNL;
	private IEnumerator ienum;
	private Texture2D image;
	private Texture2D[] txtAry;
	Vector2 mPos = Vector2.zero;
	int mTab = 0;

	/** 選擇的紋理 */
	public static Texture2D selectTexture;


	void OnEnable()
	{
		mapManager = GameObject.Find("MapManager");

		if(!mapManager)
		{
			mapManager = new GameObject();
			mapManager.name = "MapManager";
		}

		// 加入Scene事件
		SceneView.onSceneGUIDelegate += MapManager.instance.OnSceneGUI; 

		xml = new XmlDocument();
		xml.LoadXml(Resources.Load("Building").ToString());
		xmlNL = xml.SelectNodes("TextureAtlas/SubTexture");
		image = Resources.Load<Texture2D>("Building") as Texture2D;

		ienum = xmlNL.GetEnumerator();

		txtAry = new Texture2D[xmlNL.Count];

		Color[] color;
		Texture2D txt;
		Rect rect;
		int i = 0;

		while(ienum.MoveNext())
		{
			rect = new Rect(Convert.ToSingle(((XmlElement)ienum.Current).GetAttribute("x")),
			                Convert.ToSingle(((XmlElement)ienum.Current).GetAttribute("y")),
			                Convert.ToSingle(((XmlElement)ienum.Current).GetAttribute("width")),
			                Convert.ToSingle(((XmlElement)ienum.Current).GetAttribute("height")));

			color = image.GetPixels((int)rect.x,
			                        image.height - (int)rect.y - (int)rect.height,
			                        (int)rect.width,
			                        (int)rect.height);

			txt = new Texture2D((int)rect.width, (int)rect.height);
			txt.SetPixels( color );
			txt.Apply();

			txtAry[i] = txt;

			i++;
		}
	}

	void OnDisable()
	{
		Debug.Log("d");
	}

	void OnGUI()
	{
		GUILayout.BeginVertical();

		/*
		Object[] ob = AssetDatabase.LoadAllAssetsAtPath("Assets/MapEditor/Resources/Building.png");

		int i = 0;

		foreach(Object asset in ob)
		{
			if(AssetDatabase.IsSubAsset(asset))
			{
				Debug.Log("fq" + (i++));
			}
		}
		*/
		//GUIStyle myStyle = new GUIStyle(GUILayout.Box);
		//myStyle.fixedWidth = 30;
		//myStyle.fixedHeight = 30;
		
		int newTab = mTab;

		GUILayout.BeginHorizontal();
			if (GUILayout.Toggle(newTab == 0, "1", "ButtonLeft")) newTab = 0;
			if (GUILayout.Toggle(newTab == 1, "2", "ButtonMid")) newTab = 1;
			if (GUILayout.Toggle(newTab == 2, "3", "ButtonRight")) newTab = 2;
		GUILayout.EndHorizontal();
		
		if (mTab != newTab)
		{
			mTab = newTab;
		}
		GUILayout.BeginHorizontal();
		
		int HCellCount = Mathf.FloorToInt( (Screen.width - 12) / 54.0f);

		GUIStyle gs = new GUIStyle(GUI.skin.button);
		gs.fixedWidth = 50;
		gs.fixedHeight = 50;	

		mPos = GUILayout.BeginScrollView(mPos);
		{
			selItem = GUILayout.SelectionGrid(selItem, txtAry, HCellCount, gs );

			// 選擇有變
			//if(oldSel != selItem)
			//{
				selectTexture = txtAry[selItem];
			//}

			/*foreach(Texture2D tt in txtAry)
			{
				Rect rect = new Rect(x, y, 50, 50);
				Rect inner = rect;
				inner.xMin += 2f;
				inner.xMax -= 2f;
				inner.yMin += 2f;
				inner.yMax -= 2f;
				rect.yMax -= 1f; // Button seems to be mis-shaped. It's height is larger than its width by a single pixel.

				DrawTiledTexture(inner, tt);
				
				x += 52;
				
				if (x + 52 > 600)
				{
					y += 52;
					x = 0;
				}

			}
			*/
		}
		GUILayout.EndScrollView();
		/*
		{
			GUI.color = normal;
			DrawTiledTexture(inner, NGUIEditorTools.backdropTexture);
		}
		*/

	//	selItem = GUILayout.SelectionGrid(0, txtAry, txtAry.Length, myStyle);
		GUILayout.EndHorizontal();

		//GUILayout.Label(tt);
		//GUILayout.Label(tt);
		//GameObject obj = PrefabUtility.InstantiatePrefab(YuzuObj) as GameObject; 
		GUILayout.EndVertical();
	}
	
	static public void DrawTiledTexture (Rect rect, Texture2D tex)
	{
		GUI.DrawTexture(new Rect(rect.x, rect.y, 50, 50), tex, ScaleMode.ScaleToFit);
		GUI.BeginGroup(rect);
		{
			//int width  = Mathf.RoundToInt(rect.width);
			//int height = Mathf.RoundToInt(rect.height);
			
			//for (int y = 0; y < height; y += tex.height)
			//{
				//for (int x = 0; x < width; x += tex.width)
				//{
		//	GUI.DrawTexture(new Rect(50, 50, tex.width, tex.height), tex, ScaleMode.StretchToFill);
				//}
			//}
		}
		GUI.EndGroup();
	}
}
	