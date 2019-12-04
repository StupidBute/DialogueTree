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

	public DialogSheet mySheet;
	[System.NonSerialized]
	public GameObject myGod;
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

	string nowDialKey = "", finishedDialKey = "";
	int intimacy = 0;
	bool clickNextDialogue = false;
	bool textRun2End = false;
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
		if(scGod.dc_DialogSheet.ContainsKey(myName))
			mySheet = scGod.dc_DialogSheet [myName];
		scGod.NpcRegister (myName, this);
		cam = Camera.main;
		tr_DialogBox.gameObject.SetActive (false);
		DialogObj.SetActive (false);
	}
	#endregion


	#region 跑對話
	public void StartSheetAt(string dialogKey){
		SetNowDialog (dialogKey);
		RunSheet ();
	}

	public void RunSheet(){
		//若下一句為分歧點，則依照分歧點的規則賦予正確的下一句對話引導
		if (mySheet.diverges.ContainsKey (mySheet.nowDialog))
			FindDivergeKey ();

		if (mySheet.dialogs.ContainsKey (mySheet.nowDialog)) {
			StartCoroutine(RunMultiDialog(mySheet.dialogs[mySheet.nowDialog]));
		} else if (mySheet.questions.ContainsKey (mySheet.nowDialog)) {
			StartCoroutine (AskQuestion (mySheet.questions[mySheet.nowDialog]));
		} else {
			//print (myName + "無" + mySheet.nowDialog + "對話。");
		}

	}

	public void Wait2Start(float _t, string dialKey){
		StartCoroutine (IE_Wait2Start (_t, dialKey));
	}

	IEnumerator IE_Wait2Start(float _t, string dialKey){
		yield return new WaitForSeconds (_t);
		if (dialKey == "")
			RunSheet ();
		else
			StartSheetAt (dialKey);
	}

	IEnumerator RunMultiDialog(List<Dialog> list_dial){
		nowDialKey = mySheet.nowDialog;
		int lastIndex = list_dial.Count - 1;
		SetNowDialog (list_dial [lastIndex].nextDialog);	//準備好下一句對話
		for (int i = 0; i <= lastIndex; i++) {
			#region 顯示此句對話
			Dialog nowTalk = list_dial [i];
			TalkDialog (nowTalk, !DialogObj.activeSelf);
			if (!(i == lastIndex && mySheet.questions.ContainsKey (mySheet.nowDialog))) {
				//此句不是問句
				while (!clickNextDialogue) {
					if (textRun2End && (Input.GetMouseButtonDown (0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))) {
						RaycastHit2D hit = MouseClick (Input.mousePosition);
						if (hit.collider == null)
							clickNextDialogue = true;
					}
					yield return null;
				}
				textRun2End = false;
				clickNextDialogue = false;

			} else {
				WaitForSeconds waitTime = new WaitForSeconds(0.1f);
				while (!textRun2End)
					yield return waitTime;
				textRun2End = false;
			}
			#endregion

			#region 判斷已跑到此對話集的最後一句
			if (i == lastIndex){
				finishedDialKey = nowDialKey;
				//若下一句為分歧點，則依照分歧點的規則賦予正確的下一句對話引導
				if(mySheet.diverges.ContainsKey(mySheet.nowDialog))
					FindDivergeKey ();

				if (mySheet.nowDialog != "END") {
					string[] dialogStr = mySheet.nowDialog.Split (new char[]{ ':' });
					switch (dialogStr.Length) {
					case 1:
						RunSheet ();
						break;
					case 2:
						scGod.StartNpcDialog (dialogStr);
						StartCoroutine(IE_AnimOpenDialog(animType.End, 0f, 0f, null));
						break;
					case 3:
						scGod.StartNpcDialog (dialogStr);
						if (dialogStr [2] == "REST")
							TalkDialog(new Dialog(new string[]{"MM0", "16", "0", "X", "RESTING"}, "..."), false);
						else
							StartCoroutine (IE_AnimOpenDialog (animType.End, 0f, 0f, null));

						break;
					default:
						scGod.DebugTextPrint ("對話引導的屬性數量錯誤!你打的是: " + mySheet.nowDialog.ToString ());
						break;
					}
						
				} else {
					StartCoroutine (IE_AnimOpenDialog (animType.End, 0f, 0f, null));

				}

			}
			#endregion

		}
	}

	IEnumerator AskQuestion(Question nowQuestion){
		//問問題並等待玩家回答
		WaitForSeconds waitTime = new WaitForSeconds(0.2f);
		scGod.scOpt.ChooseAnswer (nowQuestion.option);
		while (scGod.scOpt.Answer == -1)			//玩家還沒回答則持續等待
			yield return waitTime;
		//玩家已回答，顯示npc應答
		mySheet.RecordAnswer(mySheet.nowDialog, scGod.scOpt.Answer);

		int index0 = scGod.scOpt.Answer / 4;
		int index1 = scGod.scOpt.Answer % 4;
		if (nowQuestion.ansDialog [index0].Count == 1) {
			//無分歧問答集
			StartCoroutine (RunMultiDialog (nowQuestion.ansDialog [index0][0].questionUnit[index1]));
		} else {
			//分歧問答集
			foreach (DivergeQuestion DQ in nowQuestion.ansDialog [index0]) {
				if (JudgeCondition (DQ.conditions)) {
					StartCoroutine (RunMultiDialog (DQ.questionUnit[index1]));
					break;
				}
			}
		}
		scGod.scOpt.Answer = -1;

	}

	#endregion


	#region 顯示對話

	void TalkDialog(Dialog _dial, bool isFirst){
		myDialog.text = "";
		intimacy += _dial.intimacy;
		if (_dial.animKey != "MM0")
			scTalk.SetTalkAnim (_dial.animKey);
		DoCommand (_dial.command);
		StartCoroutine (TextEffect (_dial, isFirst));
	}

	IEnumerator TextEffect(Dialog _dial, bool isFirst){
		bool isResting = _dial.nextDialog == "RESTING";
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
				textRun2End = true;
				yield break;
				//不跑字
			}
		}
		#endregion

		#region 跑字
		//WaitForSeconds waitLetterTime = new WaitForSeconds(letterTime);
		if(sc_God.fastDial){
			myDialog.text = _dial.text;
			yield return null;
		}else{
			string outputStr = "";
			char[] letters = _dial.text.ToCharArray ();
			foreach (char letter in letters) {
				outputStr += letter.ToString();
				myDialog.text = outputStr;
				yield return null;
				//yield return waitLetterTime;
			}
		}

		if(!isResting)
			textRun2End = true;
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
		if (_command == "X")
			return;
		string[] criticalStr = _command.Split (new char[]{ ';' });
		char[] keySplitter = new char[]{ ':' };
		char[] funcSplitter = new char[]{ '(', ',', ')' };
		char[] vecSplitter = new char[]{ 'X', 'Y' };
		foreach (string criticStr in criticalStr) {
			string[] functionStr = criticStr.Split (keySplitter);
			string[] funcStr;
			sc_NpcDialog owner;
			if (functionStr.Length > 1) {
				owner = scGod.GetNpcDialog (functionStr [0]);
				funcStr = functionStr [1].Split (funcSplitter, System.StringSplitOptions.RemoveEmptyEntries);
			}else{
				owner = this;
				funcStr = functionStr [0].Split (funcSplitter, System.StringSplitOptions.RemoveEmptyEntries);
			}

			switch (funcStr [0]) {
			case "Face":
				if (funcStr.Length != 2){
					scGod.DebugTextPrint ("函式參數數量錯誤!(" + (funcStr.Length-1).ToString () + ") Face函式需要1個參數。\n此句你打的是: " + criticStr);
				}else{
					if (funcStr [1] == "Player")
						owner.FaceTalker (scGod.playerTR);
					else
						owner.FaceTalker (scGod.GetNpcDialog (funcStr [1]).transform);
				}

				break;
			case "Move":
				if (!(funcStr.Length == 2 || funcStr.Length == 3)) {
					scGod.DebugTextPrint ("函式參數數量錯誤!(" + (funcStr.Length-1).ToString() + ") Move函式需要1或2個參數。\n此句你打的是: " + criticStr);
				} else {
					if (funcStr.Length == 2) {
						string[] vectorStr = funcStr [1].Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
						StartCoroutine (owner.scTalk.MoveToPos (new Vector2 (String2PosX (vectorStr [0]), float.Parse (vectorStr [1]))));
					} else if(funcStr.Length == 3){
						string[] vectorStr0 = funcStr [1].Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
						string[] vectorStr1 = funcStr [2].Split (vecSplitter, System.StringSplitOptions.RemoveEmptyEntries);
						owner.transform.position = new Vector2 (String2PosX (vectorStr0 [0]), float.Parse (vectorStr0 [1]));
						owner.scTalk.SnapFloor (1f);
						StartCoroutine (owner.scTalk.MoveToPos (new Vector2 (String2PosX (vectorStr1 [0]), float.Parse (vectorStr1 [1]))));
					}
				}
					
				break;
			case "BoxSide":
				if (funcStr [1] == "Right")
					isRightBox = true;
				else
					isRightBox = false;
				break;
			default:
				sc_God.SetStoryPoint(funcStr[0], true);
				break;
			}
		}
	}

	float String2PosX(string posXStr){
		if (posXStr == "Left") {
			return cam.ScreenToWorldPoint (new Vector2 (0, 0)).x - 0.6f;
		} else if (posXStr == "Right") {
			return cam.ScreenToWorldPoint (new Vector2 (Screen.width, 0)).x + 0.6f;
		} else {
			float num = 0;
			if (!float.TryParse (posXStr, out num))
				scGod.DebugTextPrint ("Move函式的參數設定錯誤! 你打的參數是: " + posXStr);
			return num;
		}
	}

	#endregion


	#region 分歧點相關
	void FindDivergeKey(){
		string divergeDialKey = mySheet.nowDialog;
		int maximum = 0;
		//持續分解Diverge，直到找出不是Diverge的對話引導，才賦予給nowDialog
		do{
			maximum++;
			divergeDialKey = DoDiverge(mySheet.diverges[divergeDialKey]);
		}while(mySheet.diverges.ContainsKey(divergeDialKey) && maximum < 20);

		SetNowDialog(divergeDialKey);
  	}

	string DoDiverge(List<DivergeUnit> paths){
		foreach (DivergeUnit unit in paths) {
			if (JudgeCondition (unit.conditions))
				return unit.nextDialKey;
		}
		return "END";
	}

	bool JudgeCondition(List<string> conditions){
		char[] AndSpliter = new char[]{ '+' };

		foreach (string _condition in conditions) {
			//進入每個or的條件
			if (_condition == "//")
				break;
			
			string[] conditionAnd = _condition.Split (AndSpliter);
			if (conditionAnd.Length > 1) {
				//有and的條件
				bool conditionPass = true;
				foreach (string _and in conditionAnd) {
					if (!JudgeSingleCondition (_and)) {
						conditionPass = false;
						break;
					}
				}
				if (conditionPass)
					return true;
			} else {
				//單一條件
				if (JudgeSingleCondition (_condition))
					return true;
			}

		}
		return false;
	}

	bool JudgeSingleCondition(string _condition){
		char[] conditionSpliter = new char[]{':'};
		char[] caseSpliter = new char[]{','};

		string[] conditionStr = _condition.Split (conditionSpliter, System.StringSplitOptions.RemoveEmptyEntries);
		if (conditionStr.Length == 3) {
			//角色:問題:答案
			string[] possibleAnswers = conditionStr [2].Split (caseSpliter, System.StringSplitOptions.RemoveEmptyEntries);
			string playerAnswer = scGod.GetNpcDialog(conditionStr[0]).mySheet.GetAnswer(conditionStr[1]);
			foreach (string answerKey in possibleAnswers) {
				if(answerKey == "All" && playerAnswer != "")
					return true;
				else if (playerAnswer == answerKey)
					return true;
			}
		} else if (conditionStr.Length == 2) {
			//Emo:心情(運算子)數量
			//Plot:劇情開關代碼
			if (conditionStr [0] == "Emo") {
				#region 情緒數量判斷
				char[] _con = conditionStr [1].ToCharArray ();
				string emo = _con [0].ToString ();
				string oper = "";	//運算子
				int compareCount = 0;
				string[] involvingNpcs = new string[]{ "" };
				if (myName == "葉宜樺")
					involvingNpcs = new string[]{ "戴勇誠", "蔣瑜涵", "董欣麗", "陳漢辰" };

				for (int i = 1; i < _con.Length; i++) {
					int num = 0;
					if (!int.TryParse (_con [i].ToString (), out num)) {
						if (i == _con.Length - 1 && _con [i] == '%') {
							int allEmoCount = GetEmoCountByArray ("J", involvingNpcs) + GetEmoCountByArray ("M", involvingNpcs)
								+ GetEmoCountByArray ("S", involvingNpcs) + GetEmoCountByArray ("N", involvingNpcs);
							compareCount = (int)Mathf.Round ((float)allEmoCount * (float)compareCount / 100f);
						} else {
							oper += _con [i].ToString ();
						}
					} else {
						compareCount = compareCount * 10 + num;
					}

				}
				int emoCount = 0;
				if (involvingNpcs [0] != "")
					emoCount = GetEmoCountByArray (emo, involvingNpcs);

				switch (oper) {
				case "=":
					if (emoCount == compareCount)
						return true;
					break;
				case "==":
					if (emoCount == compareCount)
						return true;
					break;
				case ">":
					if (emoCount > compareCount)
						return true;
					break;
				case ">=":
					if (emoCount >= compareCount)
						return true;
					break;
				case "<":
					if (emoCount < compareCount)
						return true;
					break;
				case "<=":
					if (emoCount <= compareCount)
						return true;
					break;
				}
				#endregion
			} else if (conditionStr [0] == "Plot") {
				//if (sc_God.StoryPoint.Contains (conditionStr [1]))
				if (sc_God.ContainsSP (conditionStr [1]))
					return true;
			}

		} else if (conditionStr.Length == 1) {
			//Else
			return true;
		} else {
			scGod.DebugTextPrint ("分歧點的條件輸入錯誤! 你輸入的是: " + _condition);
			return false;
		}
		return false;
	}

	int GetEmoCountByArray(string emo, string[] _names){
		int count = 0;
		foreach (string _name in _names)
			count += scGod.GetNpcDialog (_name).mySheet.GetEmoCount (emo);
		return count;
	}

	#endregion


	#region 其他

	public void StopSheet(){
		StopAllCoroutines ();
		clickNextDialogue = false;
		textRun2End = false;
		SetNowDialog (mySheet.dialogOrder[0]);
		DialogObj.SetActive (false);
		tr_DialogBox.gameObject.SetActive (false);
		scGod.scOpt.CloseQuestion ();
	}

	public bool DialFinished(string targetKey){
		return finishedDialKey == targetKey;
	}

	void SetNowDialog(string _next){
		if (_next == "") {
			if (mySheet.nowDialog == mySheet.dialogOrder[mySheet.dialogOrder.Count-1]) {
				mySheet.nowDialog = "END";
			}else {
				int _index = 0;
				while (mySheet.nowDialog != mySheet.dialogOrder [_index])
					_index++;
				mySheet.nowDialog = mySheet.dialogOrder [_index + 1];
			}

		} else{
			mySheet.nowDialog = _next;
		}
	}

	public void FaceTalker(Transform target){
		if (target.position.x > transform.position.x)
			isRightBox = false;
		else
			isRightBox = true;
		if(scTalk != null)
			StartCoroutine (scTalk.FaceTarget (target));
	}



	RaycastHit2D MouseClick(Vector3 _pos){
		Ray _ray = cam.ScreenPointToRay (_pos);
		return Physics2D.Raycast (_ray.origin, _ray.direction, 10f, 1 << 5);
	}

	#endregion
}
