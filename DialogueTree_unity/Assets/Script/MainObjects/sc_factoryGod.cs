using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class sc_factoryGod : sc_God {
	[SerializeField]
	GameObject EndScene;
	[SerializeField]
	GameObject[] EHints;

	void Start () {
		ChangeStoryState (State.葉宜樺N1N7);
		//ChangeStoryState (State.等電梯);
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

	void SetEHint(bool _activate){
		foreach (GameObject EH in EHints)
			EH.SetActive (_activate);
	}

	void ChangeStoryState(){
		ChangeStoryState ((State)((int)StoryState + 1));
	}

	void ChangeStoryState(State _state){
		StoryState = _state;

		//改變state後的一次性處理
		switch(StoryState){
		case State.葉宜樺N1N7:
			dialGod.scPlayer.canInteract = false;
			SetEHint (false);
			break;
		case State.蔣瑜涵All:
			dialGod.scPlayer.canInteract = true;
			SetEHint (true);
			break;
		case State.何欣潔N3N4:
			StartCoroutine (CheckPlayerInArea ("D", State.蔣瑜涵All));
			break;
		case State.結束:
			dialGod.scPlayer.ActiveControl (2, false);
			EndScene.SetActive (true);
			StartCoroutine (WaitChangeScene (9f));
			break;
		}
	}

	IEnumerator WaitChangeScene(float _time){
		yield return new WaitForSeconds (_time);
		ChangeScene (false, 4, 1);
	}

	IEnumerator CheckPlayerInArea(string areaCode, State endState){
		WaitForSeconds _waitTime = new WaitForSeconds (0.1f);
		string lastArea = areaCode;
		float originSize = MainCam.scCam.targetSize;
		Vector2 originPos = MainCam.scCam.targetPos;
		while ((int)StoryState < (int)endState) {
			if (lastArea != sc_Area.NowArea) {
				//player change area
				lastArea = sc_Area.NowArea;
				if (lastArea != areaCode) {
					MainCam.scCam.StopCamAnim ();
					originSize = MainCam.scCam.targetSize;
					originPos = MainCam.scCam.targetPos;
					MainCam.scCam.FreeCamera (false);
					MainCam.scCam.SetFollowTarget (true);
				} else {
					MainCam.scCam.FreeCamera (true);
					MainCam.scCam.CamAnim (originSize, originPos, 2.5f);
				}
			}
			yield return _waitTime;
		}
		yield break;
  	}
}

#region 快速對話開關
/*Toggle SpeedDialog;
	SpeedDialog = GetComponentInChildren<Toggle> ();
	if (SpeedDialog.isOn)
		sc_NpcDialog.letterTime = 0.01f;
	else
		sc_NpcDialog.letterTime = 0.04f;*/
#endregion
