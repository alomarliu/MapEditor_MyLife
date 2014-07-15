#pragma strict

class UnityScriptTest extends MonoBehaviour 
{
	private var _run:int = 4;
	public var gg:int = 10;
	public static var momo:int = 10;
	public function get run():int
	{
		return _run;
	}

	public function set run(value)
	{
		_run = value;
	}
	
	function Start () {

	}
	function Update () {

	}
	
	function OnGUI()
	{
		if(GUI.Button(Rect(10,10,100,30), "uScript"))
		{
			run = 30;
			Debug.Log(run);
		}
	}
}