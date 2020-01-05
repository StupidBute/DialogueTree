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
		開始, 
		指派任務, 
		執行任務, 
		回報任務, 
		結束
	};
	static public State StoryState = State.開始;

	void Start(){
		dialGod = GetComponent<sc_DialogGod>();
		sc_DialogGod.RegisterListener (this);
		sc_Area.RegisterListener (this);
		StartCoroutine (WaitTalk ("開場"));
	}

	#region listener functions
	public void ChangeArea(string _key){
		switch (_key) {
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

		//偵測切換StoryState的條件
		switch (StoryState) {
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
		default:
			break;
		}
		yield break;
	}
}