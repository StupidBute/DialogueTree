using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_talkNPC : sc_character {
	[System.NonSerialized]
	public sc_NpcDialog scDialog;
	protected sc_player scPlayer;
	sc_AICenter AI;
	Transform playerTR;

	override protected void Start () {
		base.Start ();
		scDialog = GetComponent<sc_NpcDialog> ();
		AI = sc_AICenter.AI;
		playerTR = AI.GetPlayerTR ();
		scPlayer = AI.GetPlayerSC ();
  	}


	#region 其他等待函式
	//等玩家靠近
	protected IEnumerator CheckPlayer(string dialKey, float checkDistance){
		if (checkDistance < 0f)
			checkDistance = 2.2f;
		WaitForSeconds nextCheckTime = new WaitForSeconds (0.1f);
		while (Vector2.Distance (playerTR.position, transform.position) > checkDistance || Mathf.Abs(playerTR.position.z - transform.position.z) > 1.5f) {
			yield return nextCheckTime;
		}
		if (dialKey == "")
			scDialog.RunSheet ();
		else
			scDialog.StartSheetAt (dialKey);
	}
	//面對目標
	public IEnumerator FaceTarget(Transform target){
		int faceNum = transform.position.x < target.position.x ? 1 : -1;
		bool faceRight = faceNum == 1 ? true : false;
		SetMove (faceRight, !faceRight);
		while (face != faceNum)
			yield return null;
		SetMove (false, false);
	}
	public IEnumerator FaceTarget(bool _right){
		int faceNum = _right ? 1 : -1;
		SetMove (_right, !_right);
		while (face != faceNum)
			yield return null;
		SetMove (false, false);
	}
	//等待目標到達位置
	public IEnumerator WaitPos(Transform targetTR, Vector2 _pos){
		WaitForSeconds _waitCheckTime = new WaitForSeconds (0.1f);
		float _dy = Mathf.Abs (_pos.y - targetTR.position.y);
		float _dx = _pos.x - targetTR.position.x;
		float originDirect = Mathf.Sign(_dx);
		while (!(_dy < 0.5f && Mathf.Abs (_dx) < 0.04f)) {
			if (_dy <= 0.5f && _dx * originDirect < 0f)
				break;
			yield return _waitCheckTime;
			_dy = Mathf.Abs (_pos.y - targetTR.position.y);
			_dx = _pos.x - targetTR.position.x;
		}
		yield break;
	}
	#endregion

	#region NPC_Sequence
	public void StartTalkNpcSequence(string[] array_function){
		StartCoroutine (IE_TalkNpcSequence (array_function));
	}

	IEnumerator IE_TalkNpcSequence(string[] array_function){
		char[] funcSpliter = new char[]{ '(', ',', ')' };


		foreach(string functionUnit in array_function){
			string[] funcVars = functionUnit.Split (funcSpliter, System.StringSplitOptions.RemoveEmptyEntries);
			switch (funcVars [0]) {
			case "Move":
				yield return StartCoroutine (MoveToPos (SplitVectorStr(funcVars[1])));
				break;
			case "GoStairs":
				yield return StartCoroutine (GoUpStairs (int.Parse (funcVars [1]), bool.Parse (funcVars [2]), float.Parse (funcVars [3])));
				break;
			case "Face":
				if (funcVars [1] == "right" || funcVars[1] == "Right")
					yield return StartCoroutine (FaceTarget (true));
				else if (funcVars [1] == "left" || funcVars[1] == "Left")
					yield return StartCoroutine (FaceTarget (false));
				else {
					Transform faceTarget;
					if (funcVars [1] == "Player")
						faceTarget = playerTR;
					else
						faceTarget = scDialog.scGod.GetNpcDialog (funcVars [1]).transform;
					yield return StartCoroutine (FaceTarget (faceTarget));
				}
				break;
			case "BoxSide":
				if (funcVars [1] == "Right")
					scDialog.isRightBox = true;
				else
					scDialog.isRightBox = false;
				break;
			case "CheckPlayer":
				if(funcVars.Length == 3)
					yield return StartCoroutine (CheckPlayer (funcVars [1], float.Parse (funcVars [2])));
				else
					yield return StartCoroutine (CheckPlayer ("", float.Parse (funcVars [1])));
				break;
			case "Wait":
				yield return new WaitForSeconds (float.Parse (funcVars [1]));
				break;
			case "Fade":
				yield return StartCoroutine (FadeSprite (float.Parse (funcVars [1]), float.Parse (funcVars [2])));
				break;
			case "WaitPos":
				Transform waitTarget;
				if (funcVars [1] == "Player")
					waitTarget = playerTR;
				else if (funcVars [1] == "Self")
					waitTarget = transform;
				else
					waitTarget = scDialog.scGod.GetNpcDialog (funcVars [1]).transform;
					
				yield return StartCoroutine (WaitPos (waitTarget, SplitVectorStr (funcVars [2])));
				break;
			case "Teleport":
				if (funcVars.Length == 3 && funcVars [1] == "Player") {
					Transform targetTR;
					sc_character targetSC;
					if (funcVars [1] == "Player") {
						targetTR = playerTR;
						targetSC = scPlayer;
					}else{
						sc_NpcDialog tmpDial = scDialog.scGod.GetNpcDialog (funcVars [1]);
						targetTR = tmpDial.transform;
						targetSC = tmpDial.scTalk;
					}
					Vector2 _tmpVec = SplitVectorStr (funcVars [2]);
					targetTR.position = new Vector3 (_tmpVec.x, _tmpVec.y, targetTR.position.z);
					targetSC.SnapFloor (1f);
				} else {
					Vector2 _tmpVec = SplitVectorStr (funcVars [1]);
					transform.position = new Vector3 (_tmpVec.x, _tmpVec.y, transform.position.z);
					SnapFloor (1f);
				}
					
				break;
			case "Talk":
				if (funcVars.Length == 1)
					scDialog.RunSheet ();
				else
					scDialog.StartSheetAt (funcVars[1]);
				break;
			case "Anim":
				SetAnim (funcVars [1]);
				break;
			case "PlayerAnim":
				scPlayer.SetAnim (funcVars [1]);
				break;
			case "CamAnim":
				sc_God.MainCam.scCam.CamAnim (float.Parse (funcVars [1]), SplitVectorStr (funcVars [2]), float.Parse (funcVars [3]));
				break;
			case "CamFocus":
				sc_God.MainCam.scCam.SetFocusBlack (float.Parse (funcVars [1]), float.Parse (funcVars [2]));
				break;
			case "CamFree":
				sc_God.MainCam.scCam.FreeCamera (bool.Parse (funcVars [1]));
				break;
			case "PlayerControl":
				scPlayer.ActiveControl (int.Parse (funcVars [1]), bool.Parse (funcVars [2]));
				break;
			case "Plot":
				sc_God.SetStoryPoint (funcVars [1], true);
				break;
			case "PlayerEle":
				scPlayer.DoInteractable ();
				break;
			case "SetAlpha":
				SetAlpha (float.Parse (funcVars [1]));
				break;
			case "EnableSprite":
				EnableSprite (bool.Parse (funcVars [1]));
				break;
			}

		}
		yield break;
	}

	Vector2 SplitVectorStr(string _str){
		char[] vecSplitter = new char[]{ 'X', 'Y' };
		string[] tmpVecStr = _str.Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
		return new Vector2 (float.Parse (tmpVecStr [0]), float.Parse (tmpVecStr [1]));
	}
	#endregion
}