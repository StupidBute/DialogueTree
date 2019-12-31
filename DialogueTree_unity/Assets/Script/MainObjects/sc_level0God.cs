using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class sc_level0God : sc_God, i_PlotFlag, i_AreaListener {

	[SerializeField]
	sc_talkNPC Kai, Lee;

	void Start(){
		PlotFlags.Clear ();
		sc_God.RegisterListener (this);
		sc_Area.RegisterListener (this);
		//StartCoroutine (WaitTalk ());
		//ChangeStoryState(State.李哥N1N9);
		//Kai.StartTalkNpcSequence(new string[]{"Wait(0.1)", "Move(X0.5Y0)"});
		//Lee.StartTalkNpcSequence(new string[]{"Wait(5)", "Talk"});
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

	IEnumerator WaitTalk(){
		yield return new WaitForSeconds (1);
		dialGod.StartPlot ("劇情1");
	}

	IEnumerator StorySequence(){
		switch (StoryState) {
		default:
			break;
		}
		yield break;
	}
}