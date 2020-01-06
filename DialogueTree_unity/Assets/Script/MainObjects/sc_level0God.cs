using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class sc_level0God : sc_God, i_PlotFlag, i_AreaListener {
	[SerializeField]
	sc_talkNPC Kai, Lee;

	sc_DialogGod dialGod;

	public enum State{
		指派任務, 
		執行任務, 
		回報任務, 
		結束
	};
	static public State StoryState = State.指派任務;

	void Start(){
		dialGod = GetComponent<sc_DialogGod>();
		sc_DialogGod.RegisterListener (this);
		sc_Area.RegisterListener (this);
		ChangeStoryState (State.指派任務);
		//StartCoroutine (WaitTalk ("開場"));
	}

	#region listener functions
	public void ChangeArea(string _key){
		switch (_key) {
		case "28f":
			sc_DialogGod.SetPlotFlag ("AreaB", true);
			break;
		default:
			break;
		}
	}

	public void FlagAdd(string _key){
		switch (_key) {
		default:
			break;
		}
	}

	public void FlagRemove(string _key){
		switch (_key) {
		default:
			break;
		}
	}
	#endregion

	protected override void Update () {
		base.Update ();

		if (Input.GetKeyDown (KeyCode.F12))
			dialGod.fastDial = !dialGod.fastDial;

		//偵測切換StoryState的條件
		switch (StoryState) {
		case State.執行任務:
			if(sc_DialogGod.ContainsPF("ChangeState"))
				ChangeStoryState ();
			break;
		default:
			if (dialGod.CheckDialogComplete ())
				ChangeStoryState ();
			break;
		}
	}

	void ChangeStoryState(){
		ChangeStoryState ((State)((int)StoryState + 1));
	}

	void ChangeStoryState(State _state){
		StoryState = _state;
		//改變state後的一次性處理
		switch(StoryState){
		case State.指派任務:
			Lee.StartTalkNpcSequence (new string[] {
				"CamFree(true)", 
				"Wait(0.1)", 
				"PlayerControl(2,false)", 
				"CamAnim(4.8,X-27.6Y3.3,3)",
				"Wait(4)",
				"CamFree(false)",
				"PlayerControl(2,true)", 
				"CheckPlayer(開場,2)"
			});
			break;
		case State.執行任務:
			StartCoroutine (StorySequence ());
			Kai.SetAnim ("idle");
			Kai.SetMove (false, false, true);
			Kai.StartTalkNpcSequence (new string[] {
				"Move(X8.88Y12.5)",
				"Face(Right)", 
				"Wait(0.2)", 
				"Anim(Interact)",
				"CheckPlayer(29f碰面,2)",  
				"Plot(ChangeState)"
			});
			break;
		case State.回報任務:
			dialGod.CheckDialogComplete ();		//reset dialog complete state
			break;
		case State.結束:
			ChangeScene (false, 3, 0);
			break;
		default:
			break;
		}
  	}

	IEnumerator WaitTalk(string key){
		yield return new WaitForSeconds (1);
		dialGod.StartPlot (key);
	}

	IEnumerator StorySequence(){
		switch (StoryState) {
		case State.執行任務:
			while (StoryState == State.執行任務)
				yield return Lee.StartTalkNpcSequence (new string[]{ "WaitPos(Player,X-22.5Y0)", "CheckPlayer(找隊長說話,2)" });
			break;
		default:
			break;
		}
		yield break;
	}
}