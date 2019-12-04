using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class sc_level0God : sc_God, i_StoryPoint, i_AreaListener {
	#region 全域變數

	[SerializeField]
	sc_talkNPC Ah_Kai, Ning, Ning_Run, Lee;

	#endregion

	void Start(){
		StoryPoint.Clear ();
		sc_God.RegisterListener (this);
		sc_Area.RegisterListener (this);
		ChangeStoryState(State.李哥N1N9);

	}

	public void ChangeArea(string _nowArea){
	}

	public void PointAdd(string _key){
		switch (_key) {
		default:
			break;
		}
	}

	public void PointRemove(string _key){
		switch (_key) {
		case "NoHu":
			SetStoryPoint ("FindHu", true);
			break;
		case "LockShot":
			MainCam.scCam.SetFollowTarget (true);
			break;
		default:
			break;
		}
	}

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

	IEnumerator StorySequence(){
		switch (StoryState) {
		default:
			break;
		}
		yield break;
	}
}