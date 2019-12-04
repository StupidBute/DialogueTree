using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class sc_DialogGod : MonoBehaviour {
	public string[] ReadFileNames;
	public Dictionary<string, DialogSheet> dc_DialogSheet = new Dictionary<string, DialogSheet>();
	public sc_Option scOpt;

	[System.NonSerialized]
	public Transform playerTR;
	[System.NonSerialized]
	public sc_player scPlayer;

	GameObject myPlayer;
	Text DebugTxt;
	string[] textUnit;
	bool hasTalked = false;
	Dictionary<string, sc_NpcDialog> NPCs = new Dictionary<string, sc_NpcDialog>();
	List<string> list_talkingNPC = new List<string>();

	void Awake () {
		DebugTxt = GetComponentInChildren<Text> ();
		myPlayer = GameObject.FindGameObjectWithTag ("Player");
		scPlayer = myPlayer.GetComponent<sc_player> ();
		scOpt = myPlayer.GetComponentInChildren<sc_Option>();
		playerTR = myPlayer.transform;

		if (ReadFileNames.Length != 0) {
			foreach (string _str in ReadFileNames)
				ReadInOneFile (true, _str);
		}
  	}

	#region 讀入與儲存對話文件
	public void ReadInOneFile(bool _loadResources, string fileName){
		if (ReadFile (_loadResources, fileName))
			StoreDialogue ();
	}

	public bool ReadFile(bool _loadResources, string fileName){
		if(DebugTxt != null)
			DebugTxt.text = "讀檔成功!";
		char[] splitChars = new char[]{'\t', '\r'};
		if (_loadResources) {
			TextAsset file = Resources.Load (fileName, typeof(TextAsset)) as TextAsset;
			if (file != null) {
				textUnit = file.text.Split (splitChars, System.StringSplitOptions.RemoveEmptyEntries);
				return true;
			} else {
				if (DebugTxt != null)
					DebugTxt.text = fileName + "讀檔失敗!";
				return false;
			}

		} else {
			bool readSuccess = true;
			string path = "";
			if (Application.platform == RuntimePlatform.WindowsEditor)
				path = Application.dataPath + "/Resources/" + fileName;
			else if (Application.platform == RuntimePlatform.WindowsPlayer)
				path = Application.dataPath + "/" + fileName;
			else
				readSuccess = false;

			if (readSuccess) {
				StreamReader sReader = new StreamReader (path);
				textUnit = sReader.ReadToEnd ().Split (splitChars, System.StringSplitOptions.RemoveEmptyEntries);
			} else {
				if(DebugTxt != null)
					DebugTxt.text = "讀檔失敗!";
			}
			return readSuccess;

		}
	}

	public void StoreDialogue(){
		char[] unitSplitChar = new char[]{ '\"', '\n' };
		int i = 0;
		while(i < textUnit.Length-3) {					//剩餘unit數量一定要大於4
			string[] dialogUnit = textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
			if (dialogUnit.Length != 0 && dialogUnit[0] == "Name") {				//讀到新的npc則開始記錄對話
				string npcName = textUnit [++i];
				List<string> list_dialogOrder = new List<string> ();
				Dictionary<string, List<Dialog>> dc_dialogs = new Dictionary<string, List<Dialog>>();
				Dictionary<string, Question> dc_questions = new Dictionary<string, Question>();
				Dictionary<string, List<DivergeUnit>> dc_diverges = new Dictionary<string, List<DivergeUnit>> ();

				//從名字的下一格開始記錄
				dialogUnit = textUnit [++i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
				do {
					if(dialogUnit.Length == 0){
						if(i <= textUnit.Length-2)
							dialogUnit = textUnit [++i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
						else
							break;
					} else if (dialogUnit [0] == "Dialog") {
						#region 讀入一行Dialog對話

						List<Dialog> list_dialogs = new List<Dialog>();
						string dialogKey = dialogUnit[1];

						dialogUnit = textUnit[++i].Split(unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
						do{
							if(dialogUnit.Length != 2){
								if(DebugTxt != null)
									DebugTxt.text = "對話集的格子行數有誤!\n此句你打的是: " + textUnit[i];
							}else if(dialogUnit[0] != "//")
								list_dialogs.Add(new Dialog(SplitAttribute(dialogUnit[0]), dialogUnit[1]));

							if(i <= textUnit.Length-2)
								dialogUnit = textUnit[++i].Split(unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
							else
								break;
						}while(IsNormalUnit(dialogUnit));
						
						if(dc_DialogSheet.ContainsKey(npcName)){
							if(dc_DialogSheet[npcName].dialogs.ContainsKey(dialogKey)){
								//if(DebugTxt != null)
									DebugTxt.text = "對話集關鍵字重複!\n此句為" + npcName + "的" + dialogKey;
							}else{
								dc_DialogSheet[npcName].dialogs.Add(dialogKey, list_dialogs);
								dc_DialogSheet[npcName].dialogOrder.Add(dialogKey);
							}
						}else{
							dc_dialogs.Add(dialogKey, list_dialogs);
							list_dialogOrder.Add(dialogKey);
						}

						#endregion

					} else if (dialogUnit [0] == "Question") {
						#region 讀入一塊Question對話
						string dialogKey = dialogUnit[1];

						List<string> list_option = new List<string>();
						List<List<DivergeQuestion>> list_d = new List<List<DivergeQuestion>>();
						if(textUnit[i+1].Split(unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries)[0] == "Diverge"){
							//分歧問答集
							i+=6;

							do{
								while(textUnit[i].Split(unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries)[0] != "DivAnswer")
									i++;
									
								ReadDivergeQuestion(ref i, ref list_option, ref list_d);//讀入一行問答，並換下一行
								if(i < textUnit.Length)
									dialogUnit = textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
								else
									break;
							}while(IsNormalUnit(dialogUnit));

						}else{
							i+=5;	//無分歧問答集，從第一個選項開始讀
							do{
								ReadInQuestion(i, ref list_option, ref list_d);		//讀完一行選項
								i+=5;												//換下一行，並判斷還是不是選項
								if(i < textUnit.Length)
									dialogUnit = textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
								else
									break;
							}while(IsNormalUnit(dialogUnit));
						}


						if(dc_DialogSheet.ContainsKey(npcName)){
							if(dc_DialogSheet[npcName].questions.ContainsKey(dialogKey)){
								if(DebugTxt != null)
									DebugTxt.text = "問答集關鍵字重複!\n此句為" + npcName + "的" + dialogKey;
							}else{
								dc_DialogSheet[npcName].questions.Add(dialogKey, new Question(list_option, list_d));
								dc_DialogSheet[npcName].dialogOrder.Add(dialogKey);
							}

						}else{
							dc_questions.Add(dialogKey, new Question(list_option, list_d));
							list_dialogOrder.Add(dialogKey);
						}
						#endregion

					} else if(dialogUnit[0] == "Diverge"){
						#region 讀入一行Diverge分歧
						List<DivergeUnit> list_divergeUnits = new List<DivergeUnit>();
						string dialogKey = dialogUnit[1];
						dialogUnit = textUnit[++i].Split(unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
						do{
							if(dialogUnit.Length < 2){
								if(DebugTxt != null)
									DebugTxt.text = "分歧點的格子行數有誤!\n此句你打的是: " + textUnit[i];
							}else if(dialogUnit[0] != "//"){
								List<string> list_con = new List<string>();
								for(int k = 0; k < dialogUnit.Length-1; k++)
									list_con.Add(dialogUnit[k]);
								list_divergeUnits.Add(new DivergeUnit(list_con, dialogUnit[dialogUnit.Length-1]));
							}

							if(i <= textUnit.Length-2)
								dialogUnit = textUnit[++i].Split(unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
							else
								break;
							//print(textUnit[i]);
						}while(IsNormalUnit(dialogUnit));

						if(dc_DialogSheet.ContainsKey(npcName)){
							if(dc_DialogSheet[npcName].diverges.ContainsKey(dialogKey)){
								//if(DebugTxt != null)
									DebugTxt.text = "分歧點關鍵字重複!\n此句為" + npcName + "的" + dialogKey;
							}else{
								dc_DialogSheet[npcName].diverges.Add(dialogKey, list_divergeUnits);
								dc_DialogSheet[npcName].dialogOrder.Add(dialogKey);
							}

						}else{//
							dc_diverges.Add(dialogKey, list_divergeUnits);
							list_dialogOrder.Add(dialogKey);
						}
						#endregion
					} else{
						dialogUnit = textUnit [++i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
					}

				} while (dialogUnit.Length == 0 || dialogUnit [0] != "Name" && i < textUnit.Length-1);
				//讀到下一個名字時則結束讀取當前npc的對話
				if(!dc_DialogSheet.ContainsKey(npcName))
					dc_DialogSheet.Add (npcName, new DialogSheet (dc_dialogs, dc_questions, dc_diverges, list_dialogOrder));
					

			} else {
				//不是讀到新的npc則跳過
				i++;
			}

		}
  	}

	void ReadInQuestion(int i, ref List<string> list_option, ref List<List<DivergeQuestion>> list_d){
		char[] unitSplitChar = new char[]{ '\"', '\n' };
		list_option.Add (textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries) [0]);
		List<Dialog>[] questionUnit = new List<Dialog>[4];
		string[] dialogUnit;

		for (int j = 1; j <= 4; j++) {
			//print(textUnit[i+j]);
			dialogUnit = textUnit [i + j].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
			List<Dialog> _d = new List<Dialog> ();
			int k = 0;
			while (k < dialogUnit.Length) {
				if (dialogUnit [k] == "//")
					break;
				if (k == dialogUnit.Length - 1) {
					if (DebugTxt != null)
						DebugTxt.text = "問答集的格子行數有誤!\n此句你打的是: " + dialogUnit [k];
					_d.Add (new Dialog (SplitAttribute (dialogUnit [k]), "此句輸入有誤!"));
				} else {
					_d.Add (new Dialog (SplitAttribute (dialogUnit [k]), dialogUnit [k + 1]));
				}

				k += 2;
			}
			questionUnit [j - 1] = _d;
		}
		List<DivergeQuestion> answerUnit = new List<DivergeQuestion> ();
		answerUnit.Add (new DivergeQuestion (questionUnit, new List<string>()));
		list_d.Add (answerUnit);

	}

	void ReadDivergeQuestion(ref int i, ref List<string> list_option, ref List<List<DivergeQuestion>> list_d){
		char[] unitSplitChar = new char[]{ '\"', '\n' };
		list_option.Add (textUnit [i-1].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries) [0]);
		List<DivergeQuestion> answerUnit = new List<DivergeQuestion> ();
		string[] dialogUnit;
		do{
			List<Dialog>[] questionUnit = new List<Dialog>[4];
			for (int j = 1; j <= 4; j++) {
				dialogUnit = textUnit [i + j].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
				List<Dialog> _d = new List<Dialog> ();
				int k = 0;
				while (k < dialogUnit.Length) {
					if (dialogUnit [k] == "//")
						break;
					if (k == dialogUnit.Length - 1) {
						if (DebugTxt != null)
							DebugTxt.text = "問答集的格子行數有誤!\n此句你打的是: " + dialogUnit [k];
						_d.Add (new Dialog (SplitAttribute (dialogUnit [k]), "此句輸入有誤!"));
					} else {
						_d.Add (new Dialog (SplitAttribute (dialogUnit [k]), dialogUnit [k + 1]));
					}

					k += 2;
				}
				questionUnit [j - 1] = _d;
			}
			dialogUnit = textUnit[i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
			List<string> conditions = new List<string>();
			for(int k = 1; k < dialogUnit.Length; k++)
				conditions.Add(dialogUnit[k]);
			//在DivAnswer那格的第二行以下都收錄到此DivQuestion的condition中

			answerUnit.Add (new DivergeQuestion (questionUnit, conditions));
			i+=5;
			dialogUnit = textUnit[i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
		}while(dialogUnit[0] == "DivAnswer");
		list_d.Add (answerUnit);

	}

	#endregion


	#region 外部啟用對話功能
	//用npc名字啟動對話
	public void NpcRunSheet(string _npcName){
		if (NPCs.ContainsKey (_npcName))
			NPCs [_npcName].RunSheet ();
		else if (DebugTxt != null)
			DebugTxt.text = "找不到此npc!";
		else
			print ("找不到此npc!");
	}
	public void NpcRunSheet(string _npcName, string dialKey){
		if (NPCs.ContainsKey (_npcName))
			NPCs [_npcName].StartSheetAt (dialKey);
		else if(DebugTxt != null)
			DebugTxt.text = "找不到此npc!";
		else
			print ("找不到此npc!");
	}

	//用二參數的key啟動npc對話(名字:對話編碼)，發生在excel中安排好的npc接話順序
	public void StartNpcDialog(string[] dialKey){
		NpcRunSheet (dialKey [0], dialKey[1]);
	}

	public void NpcRegister(string _name, sc_NpcDialog sc_dial){
		NPCs.Add (_name, sc_dial);
	}

    #endregion


	#region 其他
	public void DebugTextPrint(string _str){
		if(DebugTxt != null)
			DebugTxt.text = _str;
	}

	public void AllUnitPrint(){
		foreach (KeyValuePair<string, DialogSheet> _sheet in dc_DialogSheet) {
			print ("=========================");
			print (_sheet.Key);
			foreach (string str in _sheet.Value.dialogOrder)
				print (str);
		}
	}

	public sc_NpcDialog GetNpcDialog(string name){ return NPCs [name]; }

	bool IsNormalUnit(string[] _dialUnit){
		return (_dialUnit.Length > 0 && _dialUnit [0] != "Name" && _dialUnit [0] != "Question" 
			&& _dialUnit [0] != "Dialog" && _dialUnit[0] != "Diverge");
	}

	string[] SplitAttribute(string _att){
		string[] str = _att.Split (new char[]{ '/' });
		if (str.Length == 4 || str.Length == 5) {
			int resultInt = 0;
			if (!int.TryParse (str [1], out resultInt)) {
				//size屬性轉換失敗
				str [1] = "14";
				if(DebugTxt != null)
					DebugTxt.text = "字體大小屬性輸入錯誤!必須為純數字。\n你打的屬性是: " + _att;
			} else if (resultInt <= 0) {
				str[1] = "14";
				if(DebugTxt != null)
					DebugTxt.text = "字體大小必須為大於0的整數!\n你打的是: " + resultInt;
			}
			if (!int.TryParse (str [2], out resultInt)) {
				//好感度屬性轉換失敗
				str[2] = "0";
				if(DebugTxt != null)
					DebugTxt.text = "好感度屬性輸入錯誤!必須為純數字。\n你打的屬性是: " + _att;
			}
			return str;
				
		} else {
			if(DebugTxt != null)
				DebugTxt.text = "屬性數量有誤!此句有" + str.Length.ToString() + "個屬性。\n你打的屬性是: " + _att;
			return new string[]{"Idle", "14", "0", "X"};
		}
	}



	#endregion


	#region 讀取和設定對話狀態
	public void RegTalkingNPC(bool registing, string _npcName, Transform _npcTR){
		if (registing) {
			if(!list_talkingNPC.Contains(_npcName))
				list_talkingNPC.Add (_npcName);
			scPlayer.ActiveControl (0, false);

		} else {
			if (list_talkingNPC.Contains(_npcName))
				list_talkingNPC.Remove (_npcName);
		}

		if (list_talkingNPC.Count == 0) {
			hasTalked = true;
			scPlayer.ActiveControl (0, true);
			sc_factoryGod.MainCam.scCam.SetFollowTarget (true);
				
		} else if (list_talkingNPC.Count == 1) {
			Transform nowNpcTR = NPCs [list_talkingNPC [0]].transform;
			sc_factoryGod.MainCam.scCam.SetFollowTarget (playerTR, NPCs [list_talkingNPC [0]].transform, true);
		} else if(list_talkingNPC.Count >= 2){
			int count = list_talkingNPC.Count;
			Transform t0 = NPCs [list_talkingNPC [count - 1]].transform;
			Transform t1 = NPCs [list_talkingNPC [count - 2]].transform;
			sc_factoryGod.MainCam.scCam.SetFollowTarget (t0, t1, true);
		}

	}

	public int GetTalkingCount(){
		return list_talkingNPC.Count;
	}

	public bool CheckDialogComplete(){
		bool _talked = hasTalked;
		hasTalked = false;
		return _talked;
	}
	#endregion
}

#region Dialog Classes
public class Dialog{
	public string text;
	public string animKey = "";
	public int size;
	public int intimacy;
	public string command = "";
	public string nextDialog = "";

	public Dialog(string[] _attributes, string _text){
		text = _text;
		animKey = _attributes [0];
		size = int.Parse(_attributes[1]);
		intimacy = int.Parse(_attributes[2]);
		command = _attributes[3];
		if (_attributes.Length >= 5)
			nextDialog = _attributes[4];

	}

	public string GetNextDialog(){
		return nextDialog;
	}
}

public class Question{
	public List<string> option = new List<string>();
	public List<List<DivergeQuestion>> ansDialog = new List<List<DivergeQuestion>>();
	//玩家選項 < 分支 <問題單位> >
	public string answer = "";

	public Question(List<string> _option, List<List<DivergeQuestion>> _dial){
		option = _option;
		ansDialog = _dial;
	}
}

public class DivergeUnit{
	public List<string> conditions = new List<string>();
	public string nextDialKey;
	public DivergeUnit(List<string> _condition, string _nextDialKey){
		conditions = _condition;
		nextDialKey = _nextDialKey;
	}
}

public class DivergeQuestion{
	public List<Dialog>[] questionUnit;
	public List<string> conditions;
	public DivergeQuestion(List<Dialog>[] _qunit, List<string> _condition){
		questionUnit = _qunit;
		conditions = _condition;
	}
}

public class DialogSheet{
	public Dictionary<string, List<Dialog>> dialogs;
	public Dictionary<string, Question> questions;
	public Dictionary<string, List<DivergeUnit>> diverges;
	public List<string> dialogOrder = new List<string> ();
	public string nowDialog;
	Dictionary<string, int> emoCount = new Dictionary<string, int>(){{"J", 0}, {"M", 0}, {"S", 0}, {"N", 0}};

	public DialogSheet(Dictionary<string, List<Dialog>> _dialogs, Dictionary<string, Question> _questions, 
		Dictionary<string, List<DivergeUnit>> _diverges, List<string> _order){
		dialogs = _dialogs;
		questions = _questions;
		diverges = _diverges;
		dialogOrder = _order;
		nowDialog = dialogOrder [0];
	}

	public string GetAnswer(string key){ return questions [key].answer; }
	public int GetEmoCount(string emo){ return emoCount [emo]; }
	public void RecordAnswer(string key, int index){
		int index0 = index / 4;
		int index1 = index % 4; 
		string emo = "J";
		switch (index1) {
		case 0:
			emo = "J";
			break;
		case 1:
			emo = "M";
			break;
		case 2:
			emo = "S";
			break;
		case 3:
			emo = "N";
			break;
		}

		questions[key].answer = (index0 + 1).ToString () + emo;
		emoCount [emo] += 1;
	}
	public void SetAnswer(string key, string ans){ questions [key].answer = ans; }
	public void SetEmo(string emo, int count){ emoCount [emo] = count; }
}
#endregion
