using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class sc_NpcDialog : MonoBehaviour {
	#region 初始化
	public static float letterTime = 0.01f;
	const float UI2VectorRate = 0.02f;
	const float dialogBox_Xscale = 0.0078f;
	const float dialogBox_Yscale = 0.0078f;

	public string myName;
	[System.NonSerialized]
	GameObject myGod;
	[System.NonSerialized]
	public sc_DialogGod scGod;
	[System.NonSerialized]
	public sc_talkNPC scTalk;

	GameObject DialogObj;
	sc_BoxOutline scOutline;
	Transform tr_DialogBox, tr_BoxOutline, tr_DialCanvas;
	SpriteRenderer spr_DialogBox;
	Text myDialog;
	RectTransform tr_MyDialog;
	Camera cam;
	Color boxColor;

	[System.NonSerialized]
	public bool isRightBox = true;

	public enum animType{Start, End, Update, Jump, Rest};

	void Start () {
		myGod = GameObject.FindGameObjectWithTag ("God");
		scGod = myGod.GetComponent<sc_DialogGod> ();
		scTalk = GetComponent<sc_talkNPC> ();
		DialogObj = transform.GetChild (1).gameObject;
		myDialog = DialogObj.GetComponentInChildren<Text>();
		tr_DialogBox = DialogObj.transform.GetChild (0);
		tr_DialCanvas = DialogObj.transform.GetChild (1);
		tr_BoxOutline = DialogObj.transform.GetChild (2);
		tr_MyDialog = tr_DialCanvas.GetChild (0).GetComponent<RectTransform> ();
		scOutline = tr_BoxOutline.GetComponent<sc_BoxOutline> ();
		spr_DialogBox = tr_DialogBox.GetComponent<SpriteRenderer> ();
		boxColor = spr_DialogBox.color;
		scGod.NpcRegister (myName, this);
		cam = Camera.main;
		tr_DialogBox.gameObject.SetActive (false);
		DialogObj.SetActive (false);
	}
	#endregion

	#region 跑對話
	public void StartDialogue(DialogueSet _dialogue){
		StartCoroutine(TalkMultiDialog(_dialogue));
	}

	public void StartDialogue(Question _question){
		StartCoroutine (AskQuestion (_question));
	}

	IEnumerator TalkMultiDialog(DialogueSet _dialSet){
		for(int i = 0; i < _dialSet.dialogs.Count; i++) {
			yield return StartCoroutine(IE_TalkDialog (_dialSet.dialogs[i], !DialogObj.activeSelf));
			bool clickNextDialogue = false;
			while (!clickNextDialogue) {
				if (Input.GetMouseButtonDown (0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)) {
					RaycastHit2D hit = MouseClick (Input.mousePosition);
					if (hit.collider == null)	//沒有點擊到其他UI則繼續對話
						clickNextDialogue = true;
				}
				yield return null;
			}
		}
		StartNextDialogue(_dialSet.nextKey);
	}

	IEnumerator AskQuestion(Question nowQuestion){
		yield return StartCoroutine (IE_TalkDialog (nowQuestion.questionDial, !DialogObj.activeSelf));
		//問問題並等待玩家回答
		WaitForSeconds waitTime = new WaitForSeconds(0.2f);
		scGod.scOpt.ChooseAnswer (nowQuestion.options);
		while (scGod.scOpt.Answer == -1)			//玩家還沒回答則持續等待
			yield return waitTime;
		//玩家已回答，顯示npc應答
		nowQuestion.answer = scGod.scOpt.Answer;
		scGod.scOpt.Answer = -1;
		StartNextDialogue (nowQuestion.options [nowQuestion.answer].nextKey);
		//scGod.StartNpcDialogue (nowQuestion.options [nowQuestion.answer].nextKey);
	}

	void StartNextDialogue(string _key){
		if(_key != "END") {
			scGod.StartNpcDialogue(_key);

			_key = scGod.DoDiverge (_key);
			char[] splitter = new char[]{':'};
			string[] keyStr = _key.Split(splitter);
			//若下一句不是自己講的
			if(keyStr[0] != myName){
				if(scGod.FindCharacterDialogue(myName, _key))
					StartCoroutine(IE_TalkDialog(new Dialog ("", 12, "REST", "..."), false));
				else
					StartCoroutine (IE_AnimOpenDialog (animType.End, 0f, 0f, null));
			}

		} else
			StartCoroutine (IE_AnimOpenDialog (animType.End, 0f, 0f, null));
	}
	#endregion

	#region 顯示對話
	IEnumerator IE_TalkDialog(Dialog _dial, bool isFirst){
		myDialog.text = "";
		SetTalkAnim (_dial.animKey);
		DoCommand (_dial.command);
		yield return StartCoroutine (TextEffect (_dial, isFirst));
	}

	IEnumerator TextEffect(Dialog _dial, bool isFirst){
		bool isResting = _dial.command == "REST";
		#region 決定對話框大小
		float _width = 0f, _height = 0f, boxMargin = 0f, box_width = 0f, box_height = 0f;
		if(isResting){
			_width = 50f;
			_height = 21f;
			myDialog.alignment = TextAnchor.MiddleCenter;
		}else{
			int letterCount = _dial.text.Length;
			int lineLength = Mathf.Clamp(200 / _dial.size, 8, 25);
			int spareLetter = letterCount % lineLength;
			if(letterCount > 2){
				while (spareLetter <= 2 && spareLetter > 0) {
					lineLength++;
					spareLetter = letterCount % lineLength;
				}
			}
			int lineCount = letterCount % lineLength == 0 ? letterCount / lineLength : letterCount / lineLength + 1;

			if (lineCount >= 2) {
				_width = _dial.size * lineLength;
				_height = Mathf.Round(_dial.size * lineCount * 1.35f);
			} else {
				_width = _dial.size * letterCount;
				_height = Mathf.Round(_dial.size * 1.35f);
			}

			myDialog.alignment = lineCount >= 2? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
		}
		boxMargin = _height*0.22f;
		box_width = _width+2f*boxMargin;
		box_height = _height+2f*boxMargin;
		tr_MyDialog.sizeDelta = new Vector2 (_width, _height);
		tr_MyDialog.anchoredPosition = new Vector2(boxMargin, boxMargin);
		myDialog.fontSize = _dial.size;
		#endregion

		#region 改變對話框大小
		if(isResting){
			yield return StartCoroutine(IE_AnimOpenDialog(animType.Rest, box_width, box_height, _dial));
		}else if(isFirst){
			yield return StartCoroutine(IE_AnimOpenDialog(animType.Start, box_width, box_height, _dial));
			yield return new WaitForSeconds(0.55f);
			//對話框的黑底跑出來後，等0.55秒再開始跑字
		}else{
			if(_dial.size < 20){
				yield return StartCoroutine(IE_AnimOpenDialog(animType.Update, box_width, box_height, _dial));
			}else{
				yield return StartCoroutine(IE_AnimOpenDialog(animType.Jump, box_width, box_height, _dial));
				yield break;
				//不跑字
			}
		}
		#endregion

		#region 跑字
		if(scGod.fastDial){
			myDialog.text = _dial.text;
			yield return null;
		}else{
			string outputStr = "";
			char[] letters = _dial.text.ToCharArray ();
			foreach (char letter in letters) {
				outputStr += letter.ToString();
				myDialog.text = outputStr;
				yield return null;
			}
		}
		#endregion
	}

	IEnumerator IE_AnimOpenDialog(animType _type, float _width, float _height, Dialog _dial){
		Vector2 targetScale = new Vector2 (_width * dialogBox_Xscale, _height * dialogBox_Yscale);
		switch (_type) {
		case animType.Start:
			scGod.RegTalkingNPC (true, myName, transform);
			BoxSwitchSide (false, 0f, _width, _height);
			scOutline.OpenOutline (_type, _width, _height);
			tr_DialogBox.localScale = targetScale;
			DialogObj.SetActive (true);
			spr_DialogBox.color = boxColor;
			myDialog.color = Color.white;
			yield return new WaitForSeconds (0.35f);
			tr_DialogBox.gameObject.SetActive (true);
			break;

		case animType.End:
			scGod.RegTalkingNPC (false, myName, transform);
			scOutline.OpenOutline (_type, _width, _height);
			spr_DialogBox.DOFade (0f, 0.3f);
			myDialog.DOFade (0f, 0.25f);
			yield return new WaitForSeconds (0.7f);
			tr_DialogBox.gameObject.SetActive (false);
			DialogObj.SetActive (false);
			break;

		case animType.Update:
			scOutline.OpenOutline (_type, _width, _height);
			Sequence seq0 = DOTween.Sequence ();
			seq0.Append (tr_DialogBox.DOScale (targetScale, 0.2f)).Join (spr_DialogBox.DOFade (boxColor.a, 0.15f)).Join (myDialog.DOFade (1f, 0.15f));
			BoxSwitchSide (true, 0.2f, _width, _height);
			yield return seq0.WaitForCompletion();
			break;

		case animType.Jump:
			myDialog.text = _dial.text;
			float scale = DialogObj.transform.localScale.x * (1.04f + 0.006f * (float)(_dial.size - 20));
			float LerpTime = scale > 1.08f ? 0.08f : 0.06f;
			scOutline.OpenOutline (tr_DialogBox.localScale.x / dialogBox_Xscale, tr_DialogBox.localScale.y / dialogBox_Yscale, _width, _height, LerpTime);
			Vector2 loudScale = new Vector2 (scale, scale);
			Vector2 originScale = DialogObj.transform.localScale;
			float midX = tr_DialogBox.localScale.x > targetScale.x ? tr_DialogBox.localScale.x : targetScale.x;
			float midY = tr_DialogBox.localScale.y > targetScale.y ? tr_DialogBox.localScale.y : targetScale.y;
			Vector2 midScale = new Vector2 (midX, midY);

			Sequence seq1 = DOTween.Sequence ();
			seq1.Append (DialogObj.transform.DOScale (loudScale, LerpTime)).Join (tr_DialogBox.DOScale (midScale, LerpTime))
				.Append (DialogObj.transform.DOScale (originScale, 0.16f)).Join (tr_DialogBox.DOScale (targetScale, 0.16f));
			seq1.Insert (0f, myDialog.DOFade (1f, 0.15f)).Insert (0f, spr_DialogBox.DOFade (boxColor.a, 0.15f));

			if (!isRightBox) {
				float midWidth = midX / dialogBox_Xscale;
				seq1.Insert(0, tr_DialogBox.DOLocalMoveX(-midWidth * UI2VectorRate, LerpTime, false))
					.Insert(LerpTime, tr_DialogBox.DOLocalMoveX(-_width * UI2VectorRate, 0.15f, false));
				tr_DialCanvas.localPosition = new Vector2 (-_width * UI2VectorRate, 0f);
			}

			yield return seq1.WaitForCompletion();

			break;
		
		case animType.Rest:
			scOutline.OpenOutline (_type, _width, _height);
			Sequence seq2 = DOTween.Sequence ();
			seq2.Append (tr_DialogBox.DOScale (targetScale, 0.3f)).Join (spr_DialogBox.DOFade (boxColor.a * 0.5f, 0.3f)).Join (myDialog.DOFade (0.5f, 0.3f));
			BoxSwitchSide (true, 0.3f, _width, _height);
			yield return seq2.WaitForCompletion();
			break;

		}
		yield break;
	}

	void BoxSwitchSide(bool doAnimation, float _time, float _width, float _height){
		Vector2 pos0, pos1, pos2, scale;
		if (isRightBox) {
			pos0 = new Vector2 (Mathf.Abs (DialogObj.transform.localPosition.x), DialogObj.transform.localPosition.y);
			pos1 = Vector2.zero;
			pos2 = Vector2.zero;
			scale = new Vector2 (Mathf.Abs (tr_BoxOutline.localScale.x), tr_BoxOutline.localScale.y);
		} else {
			pos0 = new Vector2 (-Mathf.Abs (DialogObj.transform.localPosition.x), DialogObj.transform.localPosition.y);
			pos1 = new Vector2 (-_width * UI2VectorRate, 0);
			pos2 = new Vector2 (-_width * UI2VectorRate, 0);
			scale = new Vector2 (-Mathf.Abs (tr_BoxOutline.localScale.x), tr_BoxOutline.localScale.y);
		}
		if (!doAnimation) {
			DialogObj.transform.localPosition = pos0;
			tr_DialogBox.localPosition = pos1;
			tr_DialCanvas.localPosition = pos2;
			tr_BoxOutline.localScale = scale;
		} else {
			DialogObj.transform.DOLocalMove (pos0, _time, false);
			tr_DialogBox.DOLocalMove (pos1, _time, false);
			tr_DialCanvas.DOLocalMove (pos2, _time, false);
			tr_BoxOutline.DOScale (scale, _time);
		}

	}

	void DoCommand(string _command){
		if (_command == "" || _command == "REST")
			return;
		string[] criticalStr = _command.Split (new char[]{ ';' });
		char[] keySplitter = new char[]{ ':' };
		char[] funcSplitter = new char[]{ '(', ',', ')' };
		char[] vecSplitter = new char[]{ 'X', 'Y' };
		foreach (string criticStr in criticalStr) {
			string[] functionStr = criticStr.Split (keySplitter);
			string[] funcStr;
			sc_NpcDialog owner;
			if (functionStr.Length > 1) {	//owner:funcStr
				owner = scGod.GetNpcDialog (functionStr [0]);
				funcStr = functionStr [1].Split (funcSplitter, System.StringSplitOptions.RemoveEmptyEntries);
			}else{
				owner = this;
				funcStr = functionStr [0].Split (funcSplitter, System.StringSplitOptions.RemoveEmptyEntries);
			}

			switch (funcStr [0]) {
			case "Face":	//Face(target)
				if (funcStr.Length != 2){
					print ("函式參數數量錯誤!(" + (funcStr.Length-1).ToString () + ") Face函式需要1個參數。\n此句你打的是: " + criticStr);
				}else{
					if (funcStr [1] == "Player")
						owner.FaceTalker (scGod.playerTR);
					else
						owner.FaceTalker (scGod.GetNpcDialog (funcStr [1]).transform);
				}

				break;
			case "Move":
				if (!(funcStr.Length == 2 || funcStr.Length == 3)) {
					print ("函式參數數量錯誤!(" + (funcStr.Length-1).ToString() + ") Move函式需要1或2個參數。\n此句你打的是: " + criticStr);
				} else {
					if (funcStr.Length == 2) {			//Move(X?Y?)
						string[] vectorStr = funcStr [1].Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
						StartCoroutine (owner.scTalk.MoveToPos (new Vector2 (float.Parse (vectorStr [0]), float.Parse (vectorStr [1]))));
					} else if(funcStr.Length == 3){		//Move(X?Y?,X?Y?)	teleport before moving
						string[] vectorStr0 = funcStr [1].Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
						string[] vectorStr1 = funcStr [2].Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
						owner.transform.position = new Vector2 (float.Parse (vectorStr0 [0]), float.Parse (vectorStr0 [1]));
						owner.scTalk.SnapFloor (1f);
						StartCoroutine (owner.scTalk.MoveToPos (new Vector2 (float.Parse (vectorStr1 [0]), float.Parse (vectorStr1 [1]))));
					}
				}
					
				break;
			case "Anim"://Anim(key)
				if (funcStr.Length != 2)
					return;
				owner.SetTalkAnim (funcStr [1]);
				break;
			case "BoxSide":		//BoxSide(Right) or BoxSide(Left)
				if (funcStr [1] == "Right")
					isRightBox = true;
				else
					isRightBox = false;
				break;
			case "Plot":		//Plot(key)
				if (funcStr.Length != 2)
					return;
				sc_DialogGod.SetPlotFlag (funcStr [1], true);
				break;
			default:
				break;
			}
		}
	}

	#endregion

	#region 其他

	public void StopSheet(){
		StopAllCoroutines ();
		DialogObj.SetActive (false);
		tr_DialogBox.gameObject.SetActive (false);
		scGod.scOpt.CloseQuestion ();
	}

	public void FaceTalker(Transform target){
		if (target.position.x > transform.position.x)
			isRightBox = false;
		else
			isRightBox = true;
		if(scTalk != null && scTalk.MovableCharacter)
			StartCoroutine (scTalk.FaceTarget (target));
	}

	public void SetTalkAnim(string key){
		scTalk.SetAnim (key);
	}

	RaycastHit2D MouseClick(Vector3 _pos){
		Ray _ray = cam.ScreenPointToRay (_pos);
		return Physics2D.Raycast (_ray.origin, _ray.direction, 10f, 1 << 5);
	}

	#endregion
}
