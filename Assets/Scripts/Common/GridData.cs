using UnityEngine;
using System.Collections;

public class GridData 
{
	public string objID 		= "-1";
	/** 幾單位長度 */
	public int unit				=	1;
	/** 用來判定是否要以此格來放置貼圖*/
	public bool build 			= false;
	/** 參考的基準點 */
	public Vector3 baseAryIdx 	= new Vector3();
	/** 物件實體*/
	public GameObject obj		= null;
}
