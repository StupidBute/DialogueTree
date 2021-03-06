﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public interface i_PlotFlag{
	void FlagAdd (string _key);
	void FlagRemove (string _key);
}

public class sc_DialogGod : MonoBehaviour {
	public sc_Option scOpt;
	public bool fastDial = false;

	[SerializeField]
	scriptable_story story;
	[System.NonSerialized]
	public Transform playerTR;
	[System.NonSerialized]
	public sc_player scPlayer;

	Dictionary<string, sc_NpcDialog> NPCs = new Dictionary<string, sc_NpcDialog>();
	public Dictionary<string, string> Plot = new Dictionary<string, string>();
	public Dictionary<string, DialogueSet> dc_dialogues = new Dictionary<string, DialogueSet> ();
	public Dictionary<string, Question> dc_questions = new Dictionary<string, Question> ();
	public Dictionary<string, List<DivergeUnit>> dc_diverges = new Dictionary<string, List<DivergeUnit>> ();

	List<string> list_talkingNPC = new List<string>();
	static List<i_PlotFlag> PF_Listener = new List<i_PlotFlag> ();
	static List<string> PlotFlags = new List<string> ();

	string[] textColumn;
	bool hasTalked = false;

	void Awake () {
		GameObject myPlayer = GameObject.FindGameObjectWithTag ("Player");
		scPlayer = myPlayer.GetComponent<sc_player> ();
		scOpt = myPlayer.GetComponentInChildren<sc_Option>();
		playerTR = myPlayer.transform;
		PlotFlags.Clear ();
		PF_Listener.Clear ();
		ReadStoryAsset ();
  	}

	#region 讀入與儲存對話文件
	void ReadStoryAsset(){
		if (story == null)
			story = Resources.Load<scriptable_story> ("DemoStory");
		foreach (StartNodeInfo info in story.lst_startNodeInfo)
			Plot.Add (info.name, info.nextKey);
		foreach (DialogueNodeInfo info in story.lst_dialogueNodeInfo)
			dc_dialogues.Add (info.name, info.myDialSet);
		foreach (QuestionNodeInfo info in story.lst_questionNodeInfo)
			dc_questions.Add (info.name, info.myQuestion);
		foreach (DivergeNodeInfo info in story.lst_divergeNodeInfo)
			dc_diverges.Add (info.name, info.myDiverges);
	}

	/*	舊版讀入對話方法
	public void ReadInOneFile(bool _loadResources, string fileName){
		if (ReadFile (_loadResources, fileName))
			StoreDialogue ();
	}

	public bool ReadFile(bool _loadResources, string fileName){
		char[] splitChars = new char[]{ '\r' };
		if (_loadResources) {
			TextAsset file = Resources.Load (fileName, typeof(TextAsset)) as TextAsset;
			if (file != null) {
				textColumn = file.text.Split (splitChars, System.StringSplitOptions.RemoveEmptyEntries);
				return true;
			} else
				return false;
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
				textColumn = sReader.ReadToEnd ().Split (splitChars, System.StringSplitOptions.RemoveEmptyEntries);
			}
			return readSuccess;
		}
	}

	public void StoreDialogue(){
		char[] lineSplitChar = new char[]{ '\t' };
		char[] unitSplitChar = new char[]{ '\"', '\n' };

		foreach (string column in textColumn) {
			string[] textUnit = column.Split (lineSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
			string[] textLine = textUnit [0].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
			if (textLine.Length > 0) {
				switch(textLine[0]){
				case "Start":
					Plot.Add (textLine [1], textLine [2]);
					break;
				case "Dialogue":
					string dialogueKey = textLine [1];
					string nextKey = textLine [2];
					List<Dialog> list_dialog = new List<Dialog> ();
					for (int i = 1; i < textUnit.Length; i++) {
						textLine = textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
						if (textLine.Length == 0 || textLine[0] == "//")
							continue;
						
						list_dialog.Add (String2Dialog (textLine [0], textLine [1]));
					}
					dc_dialogues.Add (dialogueKey, new DialogueSet (list_dialog, nextKey));
					break;
				case "Question":
					string questionKey = textLine [1];
					textLine = textUnit [1].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
					Dialog questionDial = String2Dialog (textLine [0], textLine [1]);
					List<Option> list_option = new List<Option> ();
					for (int i = 2; i < textUnit.Length; i++) {
						textLine = textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
						list_option.Add (new Option (textLine [0], textLine [1]));
					}
					dc_questions.Add (questionKey, new Question (questionDial, list_option));
					break;
				case "Diverge":
					string divergeKey = textLine [1];
					List<DivergeUnit> list_divergeUnits = new List<DivergeUnit> ();
					for (int i = 1; i < textUnit.Length; i++) {
						textLine = textUnit [i].Split (unitSplitChar, System.StringSplitOptions.RemoveEmptyEntries);
						if (textLine.Length == 0 || textLine[0] == "//")
							continue;
						List<string> list_con = new List<string>();
						for (int j = 0; j < textLine.Length - 1; j++)
							list_con.Add (textLine [j]);
						list_divergeUnits.Add (new DivergeUnit (list_con, textLine [textLine.Length - 1]));
					}
					dc_diverges.Add (divergeKey, list_divergeUnits);
					break;
				}
			}

		}
  	}
	Dialog String2Dialog(string _attribute, string _text){
		string[] str = _attribute.Split (new char[]{ '/' });
		string animKey = str [0];
		int size = int.Parse (str [1]);
		string command = str [2];
		return new Dialog (animKey, size, command, _text);
	}

	bool IsNormalUnit(string[] _dialUnit){
		return (_dialUnit.Length > 0 && _dialUnit [0] != "Start" && _dialUnit [0] != "Question" 
			&& _dialUnit [0] != "Dialogue" && _dialUnit[0] != "Diverge");
	}*/
	#endregion

	#region 開啟對話
	public void StartPlot(string _plotName){
		StartNpcDialogue (Plot [_plotName]);
	}

	public void StartNpcDialogue(string _key){
		_key = DoDiverge (_key);
		char[] splitter = new char[]{ ':' };
		string[] str = _key.Split (splitter, System.StringSplitOptions.RemoveEmptyEntries);
		if (dc_dialogues.ContainsKey (_key))
			NPCs [str [0]].StartDialogue (dc_dialogues [_key]);
		else if (dc_questions.ContainsKey (_key)) 
			NPCs [str [0]].StartDialogue (dc_questions [_key]);
		else
			return;
	}

	public bool FindCharacterDialogue(string charName, string _nextKey){
		if (dc_diverges.ContainsKey(_nextKey))
			return FindCharacterDialogue (charName, dc_diverges [_nextKey] [0].nextKey);

		char[] splitter = new char[]{ ':' };
		if (dc_dialogues.ContainsKey (_nextKey)) {
			if (dc_dialogues [_nextKey].nextKey.Split (splitter) [0] == charName)
				return true;
			else
				return FindCharacterDialogue (charName, dc_dialogues [_nextKey].nextKey);
		} else if (dc_questions.ContainsKey (_nextKey)) {
			if (dc_questions [_nextKey].options [0].nextKey.Split (splitter) [0] == charName)
				return true;
			else
				return FindCharacterDialogue (charName, dc_questions [_nextKey].options [0].nextKey);
		} else
			return false;
	}
	#endregion

	#region NPC相關
	public void NpcRegister (string _name, sc_NpcDialog _npcDial){ NPCs.Add (_name, _npcDial); }

	public sc_NpcDialog GetNpcDialog(string name){ return NPCs [name]; }

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
			sc_God.MainCam.scCam.SetFollowTarget (true);

		} else if (list_talkingNPC.Count == 1) {
			Transform nowNpcTR = NPCs [list_talkingNPC [0]].transform;
			sc_God.MainCam.scCam.SetFollowTarget (playerTR, NPCs [list_talkingNPC [0]].transform, true);
		} else if(list_talkingNPC.Count >= 2){
			int count = list_talkingNPC.Count;
			Transform t0 = NPCs [list_talkingNPC [count - 1]].transform;
			Transform t1 = NPCs [list_talkingNPC [count - 2]].transform;
			sc_God.MainCam.scCam.SetFollowTarget (t0, t1, true);
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

	#region 分歧點相關
	public string DoDiverge(string _key){
		int i = 0;
		while (dc_diverges.ContainsKey (_key) && i < 100) {
			foreach (DivergeUnit unit in dc_diverges[_key]) {
				if (JudgeCondition (unit.conditions)) {
					_key = unit.nextKey;
					break;
				}
			}
			i++;
		}
		return _key;
	}

	bool JudgeCondition(List<string> conditions){
		bool conditionPass = true;
		foreach (string con in conditions) {
			if (!JudgeSingleCondition (con)) {
				conditionPass = false;
				break;
			}
		}
		return conditionPass;
	}
	bool JudgeSingleCondition(string _condition){
		char[] conditionSpliter = new char[]{ '(', ')' };
		char[] caseSpliter = new char[]{','};

		string[] conditionStr = _condition.Split (conditionSpliter, System.StringSplitOptions.RemoveEmptyEntries);

		if (conditionStr [0] == "Plot") {
			//Plot(劇情開關代碼)
			if (conditionStr.Length < 2)
				return false;
			string[] possibleCases = conditionStr [1].Split (caseSpliter, System.StringSplitOptions.RemoveEmptyEntries);
			foreach(string plotFlag in possibleCases){
				if (ContainsPF (plotFlag))
					return true;
			}
		} else if (conditionStr [0] == "Else"){
			//Else預設路線
			return true;
		} else {
			//問題(答案)
			if (conditionStr.Length < 2)
				return false;
			string[] possibleCases = conditionStr [1].Split (caseSpliter, System.StringSplitOptions.RemoveEmptyEntries);
			int playerAnswer = dc_questions [conditionStr [0]].answer;
			foreach (string answerKey in possibleCases) {
				if(answerKey == "All" && playerAnswer != -1)
					return true;
				else if (playerAnswer == int.Parse(answerKey))
					return true;
			}
		}
		return false;
	}
    #endregion

	#region PlotFlag相關
	static public void RegisterListener(i_PlotFlag SPL){
		PF_Listener.Add (SPL);
	}

	static public void SetPlotFlag(string _key, bool _isAdd){
		if (_isAdd) {
			if (!PlotFlags.Contains (_key))
				PlotFlags.Add (_key);
			foreach (i_PlotFlag PFL in PF_Listener)
				PFL.FlagAdd (_key);
		} else {
			if (PlotFlags.Contains (_key))
				PlotFlags.Remove (_key);
			foreach (i_PlotFlag PFL in PF_Listener)
				PFL.FlagRemove (_key);
		}
	}

	static public bool ContainsPF(string _key){
		return PlotFlags.Contains (_key);
	}
	#endregion
}

#region Dialogue Classes
[System.Serializable]
public class DialogueSet{
	public List<Dialog> dialogs = new List<Dialog> ();
	public string nextKey = "";

	public DialogueSet(List<Dialog> _dialogs, string _next){
		dialogs = _dialogs;
		nextKey = _next;
	}
}

[System.Serializable]
public class Dialog{
	public string text;
	public string animKey = "";
	public int size;
	public string command = "";

	public Dialog(string _anim, int _size, string _command, string _txt){
		animKey = _anim;
		size = _size;
		command = _command;
		text = _txt;
	}
}

[System.Serializable]
public class Question{
	public Dialog questionDial;
	public List<Option> options = new List<Option>();
	public int answer = -1;

	public Question(Dialog _dial, List<Option> _option){
		questionDial = _dial;
		options = _option;
	}
}

[System.Serializable]
public class Option{
	public string text;
	public string nextKey;
	public Option(string _text, string _key){
		text = _text;
		nextKey = _key;
	}
}

[System.Serializable]
public class DivergeUnit{
	public List<string> conditions = new List<string>();
	public string nextKey;
	public DivergeUnit(List<string> _condition, string _nextKey){
		conditions = _condition;
		nextKey = _nextKey;
	}
}

#endregion
