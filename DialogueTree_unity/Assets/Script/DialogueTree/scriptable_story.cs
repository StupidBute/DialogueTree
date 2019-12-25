using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "new story", menuName = "Story")]
public class scriptable_story : ScriptableObject {
	public List<CharInfo> lst_charInfo;
	public List<StartNodeInfo> lst_startNodeInfo;
	public List<DialogueNodeInfo> lst_dialogueNodeInfo;
	public List<QuestionNodeInfo> lst_questionNodeInfo;
	public List<DivergeNodeInfo> lst_divergeNodeInfo;
}

[System.Serializable]
public class CharInfo{
	public string name;
	public int colorIndex;

	public CharInfo(Character c){
		name = c.name;
		colorIndex = c.colorIndex;
	}
}
[System.Serializable]
public class NodeInfo{
	public string name;
	public Vector2 myPos;

	public NodeInfo(Node n){
		name = n.nodeName;
		myPos = n.rect.position;
	}

	virtual public Node Cast2Node(DialogueTree _dt){
		return new Node (_dt, myPos);
	}

	virtual public void Reconnect (DialogueTree _dt){}
}

[System.Serializable]
public class StartNodeInfo : NodeInfo{
	public string nextKey;
	public StartNodeInfo(Node n) : base(n){
		nextKey = n.GetNextNodeName ();
	}

	override public Node Cast2Node(DialogueTree _dt){
		StartNode n = new StartNode (_dt, myPos);
		n.nodeName = name;
		return n;
	}

	override public void Reconnect(DialogueTree _dt){
		if (nextKey == "END")
			return;
		_dt.GetNodeByName (name).SetConnect (_dt.GetNodeByName (nextKey));
	}
}

[System.Serializable]
public class DialogueNodeInfo : NodeInfo{
	public string charName;
	public DialogueSet myDialSet;


	public DialogueNodeInfo(Node n) : base(n){
		DialogueNode dn = (DialogueNode)n;
		charName = dn.myCharacter.name;
		myDialSet = new DialogueSet (dn.lst_dial, dn.GetNextNodeName ());
	}

	override public Node Cast2Node(DialogueTree _dt){
		DialogueNode n = new DialogueNode (_dt, myPos);
		n.nodeName = name;
		n.myCharacter = _dt.lst_chars.Find (c => c.name == charName);
		n.lst_dial = myDialSet.dialogs;
		return n;
	}

	override public void Reconnect(DialogueTree _dt){
		if (myDialSet.nextKey == "END")
			return;
		_dt.GetNodeByName (name).SetConnect (_dt.GetNodeByName (myDialSet.nextKey));
	}
}


[System.Serializable]
public class QuestionNodeInfo : NodeInfo{
	public string charName;
	public Question myQuestion;
	public List<SubNodeInfo> optionInfos = new List<SubNodeInfo> ();

	public QuestionNodeInfo(Node n) : base(n){
		QuestionNode qn = (QuestionNode)n;
		charName = qn.myCharacter.name;
		List<Option> lst_opt = new List<Option> ();
		foreach (SubNode opt in qn.options) {
			//opt.myOption.nextKey = opt.GetNextNodeName ();
			optionInfos.Add (new SubNodeInfo (opt));
			lst_opt.Add (new Option (opt.myOption, opt.GetNextNodeName ()));
		}
		myQuestion = new Question (qn.questionDial, lst_opt);
	}

	override public Node Cast2Node(DialogueTree _dt){
		QuestionNode n = new QuestionNode (_dt, myPos);
		n.nodeName = name;
		n.myCharacter = _dt.lst_chars.Find (c => c.name == charName);
		n.questionDial = myQuestion.questionDial;
		n.DeleteOption (0);
		foreach (SubNodeInfo info in optionInfos)
			n.options.Add (info.Cast2SubNode (_dt, n));
		return n;
	}

	override public void Reconnect(DialogueTree _dt){
		QuestionNode qn = (QuestionNode)_dt.GetNodeByName (name);
		for (int i = 0; i < myQuestion.options.Count; i++) {
			if(myQuestion.options [i].nextKey != "END")
				qn.options [i].SetConnect (_dt.GetNodeByName (myQuestion.options [i].nextKey));
		}
	}
}
[System.Serializable]
public class DivergeNodeInfo : NodeInfo{
	public List<DivergeUnit> myDiverges = new List<DivergeUnit> ();
	public List<SubNodeInfo> diverInfos = new List<SubNodeInfo> ();

	public DivergeNodeInfo(Node n) : base(n){
		DivergeNode dn = (DivergeNode)n;
		foreach (SubNode diver in dn.diverges) {
			diverInfos.Add (new SubNodeInfo (diver));
			List<string> conditions = new List<string> ();
			foreach (ConditionUnit con in diver.myDiverge)
				conditions.Add (con.condition);
			myDiverges.Add (new DivergeUnit (conditions, diver.GetNextNodeName ()));
		}
			
	}

	override public Node Cast2Node(DialogueTree _dt){
		DivergeNode n = new DivergeNode (_dt, myPos);
		n.nodeName = name;
		n.DeleteDiverge (0);
		foreach (SubNodeInfo info in diverInfos)
			n.diverges.Add (info.Cast2SubNode (_dt, n));
		return n;
	}

	override public void Reconnect(DialogueTree _dt){
		DivergeNode dn = (DivergeNode)_dt.GetNodeByName (name);
		for (int i = 0; i < myDiverges.Count; i++) {
			if (myDiverges [i].nextKey != "END")
				dn.diverges [i].SetConnect (_dt.GetNodeByName (myDiverges [i].nextKey));
		}
	}
}

[System.Serializable]
public class SubNodeInfo : NodeInfo{
	public string myOption = "";
	public List<string> diverConditions = new List<string> ();

	public SubNodeInfo(Node n) : base(n){
		SubNode sn = (SubNode)n;
		if (sn.myOption != "")
			myOption = sn.myOption;
		else {
			foreach (ConditionUnit con in sn.myDiverge)
				diverConditions.Add (con.condition);
		}
	}

	public SubNode Cast2SubNode(DialogueTree _dt, Node preNode){
		if (myOption != "")
			return new SubNode (_dt, myPos, myOption, preNode);
		else {
			List<ConditionUnit> conditionUnits = new List<ConditionUnit> ();
			foreach (string str in diverConditions)
				conditionUnits.Add (new ConditionUnit (str));
			foreach (ConditionUnit con in conditionUnits)
				con.myQuestion = (QuestionNode)_dt.GetNodeByName (con.condition.Split (new char[]{ '(', ')' }) [0]);
			return new SubNode (_dt, myPos, conditionUnits, preNode);
		}
			
	}
}
