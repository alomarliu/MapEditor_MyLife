using UnityEngine;
using System.Collections;

public class UnityScriptTest : MonoBehaviour {

	private int _run = 4;

	// Use this for initialization
	private void Start () {
	
	}
	
	// Update is called once per frame
	private void Update () {
	
	}

	public int run
	{
		set { _run = value; }
		get { return _run; }
	}

	private void OnGUI()
	{
		if(GUI.Button(new Rect(200,10,100,30), "C#"))
		{
			run = 20;
			Debug.Log(run);
		}
	}
}
