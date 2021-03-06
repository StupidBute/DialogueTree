﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyFunctions{
	static GUIStyle style_dropdown = Resources.Load<GUISkin>("GUISkin/NodeSkin").GetStyle ("dropdown");
	static Texture2D tex_dropdownArrow = Resources.Load<Texture2D> ("GUISkin/Icons/dropdownArrow");

	public static int GUIPopup(Rect rect, int index, string[] strs){
		int value = EditorGUI.Popup (rect, index, strs, style_dropdown);
		Rect rect_arrow = new Rect (0, 0, 13, 9);
		rect_arrow.size *= rect.height / 40;
		rect_arrow.position = new Vector2 (rect.xMax - rect_arrow.width - 10, rect.center.y - rect_arrow.height * 0.5f);
		GUI.DrawTexture (rect_arrow, tex_dropdownArrow);
		return value;
	}

	public static bool PopupMenu(Rect rect, string title){
		bool click = GUI.Button (rect, title, style_dropdown);
		Rect rect_arrow = new Rect (0, 0, 13, 9);
		rect_arrow.size *= rect.height / 40;
		rect_arrow.position = new Vector2 (rect.xMax - rect_arrow.width - 10, rect.center.y - rect_arrow.height * 0.5f);
		GUI.DrawTexture (rect_arrow, tex_dropdownArrow);
		return click;
	}

	public static Vector2 SnapPos(Vector2 size, Vector2 position, float range){
		Vector2 center = position + 0.5f * size;
		center.x = Mathf.Round (center.x / range) * range;
		center.y = Mathf.Round (center.y / range) * range;
		return center - 0.5f * size;
	}

	public static void SetTexture(ref Texture2D _tex, Color _c){
		_tex = new Texture2D (1, 1);
		_tex.SetPixel (0, 0, _c);
		_tex.Apply ();
	}

	public static string ClampString(string target, string value){
		char[] splitter = new char[]{ ' ' };
		return (target.Split (splitter, System.StringSplitOptions.RemoveEmptyEntries).Length == 0) ? value : target;
	}

	public static string SetName(string _name, Node target, List<Node> nodeList){
		foreach(Node n in nodeList) {
			if (target != null && target == n)
				continue;
			if (_name == n.nodeName) {
				string[] sepName = _name.Split (new char[]{ ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
				int number = 0;
				if (int.TryParse (sepName [sepName.Length - 1], out number)) {
					string result = "";
					for (int i = 0; i < sepName.Length - 1; i++)
						result += sepName [i] + " ";
					return SetName (result + (number + 1).ToString (), target, nodeList);
				} else
					return SetName (_name + " 1", target, nodeList);
			}
		}
		return _name;
	}

	public static string SetName(string _name, Character target, List<Character> charList){
		foreach(Character c in charList) {
			if (_name == c.name && c != target) {
				string[] sepName = _name.Split (new char[]{ ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
				int number = 0;
				if (int.TryParse (sepName [sepName.Length - 1], out number)) {
					string result = "";
					for (int i = 0; i < sepName.Length - 1; i++)
						result += sepName [i] + " ";
					return SetName (result + (number + 1).ToString (), target, charList);
				} else
					return SetName (_name + " 1", target, charList);
			}
		}
		return _name;
	}
}

#region Nodes

public class Node {
	const float gridSize = 20;
	public Rect rect;
	public Rect canvasRect;
	protected DialogueTree DTGod;
	protected GUISkin mySkin;
	protected GUIStyle style_name;
	protected List<NodeLink> NextLink = new List<NodeLink> ();
	protected List<NodeLink> PrevLink = new List<NodeLink> ();

	protected Texture2D tex_normal, tex_selected;
	protected Vector2 size = new Vector2 (140, 58);
	protected Rect NameRect = new Rect (13, 31, 120, 20), outlineRect, boxRect;
	Vector2 dragPos;
	bool isSelected = false;

	public string nodeName = "新節點";
	protected string defaultName = "新節點";

	public Node(DialogueTree _dt, Vector2 _pos){
		SetRects (_pos);
		DTGod = _dt;
		tex_selected = Resources.Load<Texture2D> ("GUISkin/Nodes/SelectedOutline");
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DialogueNode");
		mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_name = mySkin.GetStyle ("nodeName");
	}

#region Draw
	virtual public void DrawSelf(Vector2 coordinate){
		canvasRect = rect;
		canvasRect.position += coordinate;
		GUI.BeginGroup(canvasRect);
		if (isSelected)
			GUI.DrawTexture(outlineRect, tex_selected);
		GUI.DrawTexture(boxRect, tex_normal);
		GUI.Label (NameRect, nodeName, style_name);
		GUI.EndGroup ();
	}

	virtual public void DrawMyLink(){
		foreach (NodeLink nl in NextLink)
			nl.DrawSelf ();
  	}
#endregion

#region Hit
	virtual public Node HitTest(Vector2 mousePos){
		if (canvasRect.Contains (mousePos)) {
			Selected (true);
			EditorGUI.FocusTextInControl ("");
			return this;
		} else
			return null;
	}

	virtual public bool HitLinkTest(Vector2 mousePos){
		if (NextLink.Count > 0 && NextLink[0].HitTest (mousePos))
			return true;
		return false;
	}
#endregion

#region others
	protected void SetRects(Vector2 _pos){
		boxRect = new Rect (3 * Vector2.one, size);
		size += 6 * Vector2.one;	//加上outline的大小
		outlineRect = new Rect (Vector2.zero, size);
		SnapGrid(_pos);
	}

	public void Selected(bool _isSelected){
		isSelected = _isSelected;
		dragPos = rect.position;
	}

	public void FollowMouse(Vector2 mouseDelta){
		dragPos += mouseDelta;
		SnapGrid (dragPos);
	}

	public void SetConnect(Node nextNode){
		NodeLink nl = new NodeLink (this, nextNode);
		NextLink.Add (nl);
		nextNode.PrevLink.Add (nl);
		bool canMultiLink = this.GetType () == typeof(QuestionNode) || this.GetType () == typeof(DivergeNode);
		if (!canMultiLink && NextLink.Count > 1)
			NextLink [0].DeleteSelf ();
	}

	public void DeleteAllConnect(){
		for (int i = 0; i < PrevLink.Count; i++)
			PrevLink [i].DeleteSelf ();
		for (int i = 0; i < NextLink.Count; i++)
			NextLink [i].DeleteSelf ();
	}

	public void DeleteLink(bool isPrev, NodeLink target){
		if (isPrev)
			PrevLink.Remove (target);
		else
			NextLink.Remove (target);
	}

	protected void SnapGrid(Vector2 _pos){
		rect = new Rect (MyFunctions.SnapPos (size, _pos, gridSize), size);
	}

	public void SetName(string _name){
		nodeName = MyFunctions.SetName (MyFunctions.ClampString (_name, defaultName), this, DTGod.lst_node);
	}

	public string GetNextNodeName(){
		//無下一條連結或多於一條連結，則回傳END
		string nextName;
		if(NextLink.Count == 1){
			switch (NextLink [0].nodeB.GetType ().ToString()) {
			case "DialogueNode":
				DialogueNode dn = (DialogueNode)NextLink [0].nodeB;
				nextName = dn.myCharacter.name + ":" + dn.nodeName;
				break;
			case "QuestionNode":
				QuestionNode qn = (QuestionNode)NextLink [0].nodeB;
				nextName = qn.myCharacter.name + ":" + qn.nodeName;
				break;
			default:
				nextName = NextLink [0].nodeB.nodeName;
				break;
			}
		}else
			nextName = "END";
		return nextName;
	}
#endregion
}

public class StartNode : Node{
	public StartNode(DialogueTree _dt, Vector2 _pos):base(_dt, _pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/NewStart");
		size = new Vector2 (120, 46);
		SetRects (_pos);
		NameRect = new Rect (13, 23, 101, 20);
		defaultName = "新劇情對話";
		SetName (defaultName);
	}
}

public class DialogueNode : Node{
	public Character myCharacter;
	public List<Dialog> lst_dial = new List<Dialog> ();
	Rect rect_tag = new Rect (3, 3, 94, 22);
	Rect rect_name = new Rect (3, 3, 70, 24);

	public DialogueNode(DialogueTree _dt, Vector2 _pos):base(_dt, _pos){
		defaultName = "新對話集";
		SetName (defaultName);
		myCharacter = DTGod.lst_chars [0];
		lst_dial.Add (new Dialog ("", 12, "", ""));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		GUI.BeginGroup(canvasRect);
		GUI.DrawTexture (rect_tag, myCharacter.GetNodeTag ());
		GUI.Label (rect_name, myCharacter.name, myCharacter.GetTagStyle ());
		GUI.EndGroup ();
	}
}

public class QuestionNode : Node{
	public Character myCharacter;
	public Dialog questionDial = new Dialog ("", 12, "", "");
	public List<SubNode> options = new List<SubNode> ();
	Rect rect_tag = new Rect (3, 3, 94, 22);
	Rect rect_name = new Rect (3, 3, 70, 24);

	public QuestionNode(DialogueTree _dt, Vector2 _pos):base(_dt, _pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/QuestionNode");
		defaultName = "新問答集";
		SetName (defaultName);
		myCharacter = DTGod.lst_chars [0];
		options.Add (new SubNode (_dt, rect.center + new Vector2 (-35, 65), "選項內容", this));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		GUI.BeginGroup(canvasRect);
		GUI.DrawTexture (rect_tag, myCharacter.GetNodeTag ());
		GUI.Label (rect_name, myCharacter.name, myCharacter.GetTagStyle ());
		GUI.EndGroup ();
		for (int i = 0; i < options.Count; i++)
			options [i].Draw (coordinate, i);
	}

	override public Node HitTest(Vector2 mousePos){
		if (canvasRect.Contains (mousePos)) {
			Selected (true);
			return this;
		} else {
			Node returnNode = null;
			foreach (SubNode sn in options) {
				returnNode = sn.HitTest (mousePos);
				if (returnNode != null)
					return returnNode;
			}
			return null;
		}
	}

	override public void DrawMyLink(){
		base.DrawMyLink ();
		foreach (SubNode sn in options)
			sn.DrawMyLink ();
	}

	override public bool HitLinkTest(Vector2 mousePos){
		foreach (SubNode optNode in options) {
			if (optNode.HitLinkTest (mousePos))
				return true;
		}
		return false;
	}

	public void DeleteOption(int index){
		options [index].DeleteAllConnect ();
		options.RemoveAt (index);
	}
}

public class DivergeNode : Node{
	public List<SubNode> diverges = new List<SubNode> ();

	public DivergeNode(DialogueTree _dt, Vector2 _pos):base(_dt, _pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DivergeNode");
		size = new Vector2 (110, 50);
		SetRects (_pos);
		NameRect = new Rect (13, 28, 89, 20);
		defaultName = "新分歧點";
		SetName (defaultName);
		diverges.Add (new SubNode (_dt, rect.center + new Vector2 (-35, 65), new List<ConditionUnit>{ new ConditionUnit ("Else") }, this));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		for (int i = 0; i < diverges.Count; i++) {
			diverges [i].Draw (coordinate, i);
		}
	}

	override public Node HitTest(Vector2 mousePos){
		if (canvasRect.Contains (mousePos)) {
			Selected (true);
			return this;
		} else {
			Node returnNode = null;
			foreach (SubNode sn in diverges) {
				returnNode = sn.HitTest (mousePos);
				if (returnNode != null)
					return returnNode;
			}
			return null;
		}
	}

	override public void DrawMyLink(){
		base.DrawMyLink ();
		foreach (SubNode sn in diverges)
			sn.DrawMyLink ();
	}

	override public bool HitLinkTest(Vector2 mousePos){
		foreach (SubNode diverNode in diverges) {
			if (diverNode.HitLinkTest (mousePos))
				return true;
		}
		return false;
	}

	public void DeleteDiverge(int index){
		diverges [index].DeleteAllConnect ();
		diverges.RemoveAt (index);
	}
}

public class SubNode : Node{
	public string myOption = "";
	public List<ConditionUnit> myDiverge = new List<ConditionUnit>();

	public string[] conditionType = new string[]{ "劇情", "回答" };

	public SubNode(DialogueTree _dt, Vector2 _pos, string opt, Node prevNode):base(_dt, _pos){
		myOption = opt;
		prevNode.SetConnect (this);
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/OptionUnit");
		tex_selected = Resources.Load<Texture2D> ("GUISkin/blank");
		size = new Vector2 (70, 44);
		SetRects (_pos);
		NameRect = new Rect (7, 7, 62, 36);
		nodeName = myOption;
		style_name = mySkin.GetStyle ("optionunit");
	}

	public SubNode(DialogueTree _dt, Vector2 _pos, List<ConditionUnit> diver, Node prevNode):base(_dt, _pos){
		myDiverge = diver;
		prevNode.SetConnect (this);
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DivergeUnit");
		tex_selected = Resources.Load<Texture2D> ("GUISkin/blank");
		size = new Vector2 (70, 44);
		SetRects (_pos);
		NameRect = new Rect (7, 7, 62, 36);
		nodeName = myDiverge [0].condition;
		style_name = mySkin.GetStyle ("divergeunit");
	}

	public void Draw(Vector2 coordinate, int index){
		if (myOption != "")
			nodeName = "選項" + index.ToString () + "\n" + myOption;
		else {
			foreach (ConditionUnit c in myDiverge)
				c.UpdateQuestion ();
			nodeName = "分歧" + index.ToString () + "\n" + myDiverge [0].condition;
		}
			
		base.DrawSelf (coordinate);
	}
}

public class NodeLink{
	public Node nodeA, nodeB;
	Rect rectEdit;
	bool editable = true;

	Color[] arrowColors = new Color[]{ Color.white, new Color32 (250, 135, 255, 255) };
	Texture2D[] tex_arrows = new Texture2D[4];
	Texture2D[] tex_edits = new Texture2D[2];
	Vector2[] LinkDirect = new Vector2[]{ Vector2.right, Vector2.left, Vector2.up, Vector2.down };
	Vector2 arrowSize = new Vector2 (10, 10);
	Vector2 editSize = new Vector2 (15, 15);

	public NodeLink(Node na, Node nb){
		nodeA = na;
		nodeB = nb;
		editable = nodeA.GetType () != typeof(QuestionNode) && nodeA.GetType () != typeof(DivergeNode);

		tex_arrows [0] = Resources.Load<Texture2D> ("GUISkin/Icons/ArrowR");
		tex_arrows [1] = Resources.Load<Texture2D> ("GUISkin/Icons/ArrowL");
		tex_arrows [2] = Resources.Load<Texture2D> ("GUISkin/Icons/ArrowD");
		tex_arrows [3] = Resources.Load<Texture2D> ("GUISkin/Icons/ArrowU");
		tex_edits[0] = Resources.Load<Texture2D> ("GUISkin/Icons/EditGear1");
		tex_edits[1] = Resources.Load<Texture2D> ("GUISkin/Icons/EditGear2");
		rectEdit = new Rect (new Vector2 (nodeB.canvasRect.center.x - 0.5f * editSize.x, nodeB.canvasRect.yMax - 13 - editSize.y), editSize);
	}

	public bool HitTest(Vector2 mousePos){
		if (!editable)
			return false;
		bool hit = rectEdit.Contains (mousePos);
		if (hit)	Dropdown ();
		return rectEdit.Contains (mousePos);
	}

	void Dropdown(){
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Delete"), false, () => DeleteSelf());
		menu.ShowAsContext ();
	}

	public void DrawSelf(){
		#region 設定數值
		Vector2 pointA = nodeA.canvasRect.center;
		Vector2 pointB = nodeB.canvasRect.center;
		Vector2 pointC = pointA;
		Vector2 tan0, tan1;
		int LinkType = 0;	//	0:A左B右		1:A右B左		2:A上B下		3:A下B上

		float min_dy = 0.5f * (nodeA.rect.height + nodeB.rect.height) + 15;
		float min_dx = 0.5f * (nodeA.rect.width + nodeB.rect.width) + 15;

		//判別方向
		if (Mathf.Abs (pointA.y - pointB.y) < min_dy) {
		//水平分支
			if (Mathf.Abs (pointA.x - pointB.x) < min_dx){
				DrawShortLink();
				return;
			}
			if (pointA.x < pointB.x) {
				pointA.x = nodeA.canvasRect.xMax-3;
				pointB.x = nodeB.canvasRect.xMin;
				pointC = new Vector2 (Mathf.Clamp (pointB.x - 60f, pointA.x, 99999), pointA.y);
				LinkType = 0;
			} else {
				pointA.x = nodeA.canvasRect.xMin+3;
				pointB.x = nodeB.canvasRect.xMax;
				pointC = new Vector2 (Mathf.Clamp (pointB.x + 60f, -99999, pointA.x), pointA.y);
				LinkType = 1;
			}

		} else {
		//垂直分支
			if (pointA.y < pointB.y) {
				pointA.y = nodeA.canvasRect.yMax-3;
				pointB.y = nodeB.canvasRect.yMin;
				pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 80f, pointA.y, 99999));
				LinkType = 2;
			} else {
				pointA.y = nodeA.canvasRect.yMin+3;
				pointB.y = nodeB.canvasRect.yMax;
				pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 80f, -99999, pointA.y));
				LinkType = 3;
			}

		}

		tan0 = LinkType < 2 ? new Vector2 (pointB.x, pointA.y) : new Vector2 (pointC.x, pointB.y);
		tan1 = LinkType < 2 ? new Vector2 (pointC.x, pointB.y) : new Vector2 (pointB.x, pointC.y);
		rectEdit.position = Vector2.Lerp (pointC, pointB, 0.5f) - 0.5f * editSize;
		#endregion

		if (Vector2.Distance (pointA, pointC) > 1f)
			Handles.DrawBezier (pointA, pointC, pointA, pointC, arrowColors [LinkType % 2], null, 2f);
		Handles.DrawBezier (pointC, pointB, tan0, tan1, arrowColors [LinkType % 2], null, 2f);
		if (editable) {
			GUI.DrawTexture (new Rect (pointB - (6f * LinkDirect [LinkType]) - (0.5f * arrowSize), arrowSize), tex_arrows [LinkType]);
			GUI.DrawTexture (rectEdit, tex_edits [LinkType % 2]);
		}
	}

	void DrawShortLink(){
		Vector2 pointA = nodeA.canvasRect.center;
		Vector2 pointB = nodeB.canvasRect.center;
		bool A_Bigger = nodeA.rect.width > nodeB.rect.width;
		float min_dx = A_Bigger ? 0.5f * nodeB.rect.width + 5 : 0.5f * nodeA.rect.width + 5;
		float min_dy = A_Bigger ? 0.5f * nodeB.rect.height + 5 : 0.5f * nodeA.rect.height + 5;
		Vector2 distance = pointA - pointB;
		if (Mathf.Abs (distance.x) < min_dx || Mathf.Abs (distance.y) < min_dy) {
			if (nodeB.canvasRect.xMin > nodeA.canvasRect.xMax) {
				pointA.x = nodeA.canvasRect.xMax - 3;
				pointB.x = nodeB.canvasRect.xMin + 3;
			} else if (nodeB.canvasRect.xMax < nodeA.canvasRect.xMin) {
				pointA.x = nodeA.canvasRect.xMin + 3;
				pointB.x = nodeB.canvasRect.xMax - 3;
			} else if (nodeB.canvasRect.yMin > nodeA.canvasRect.yMax) {
				pointA.y = nodeA.canvasRect.yMax - 3;
				pointB.y = nodeB.canvasRect.yMin + 3;
			} else if (nodeB.canvasRect.yMax < nodeA.canvasRect.yMin) {
				pointA.y = nodeA.canvasRect.yMin + 3;
				pointB.y = nodeB.canvasRect.yMax - 3;
			} else
				return;
			Handles.DrawBezier (pointA, pointB, pointA, pointB, arrowColors [0], null, 2f);
			return;
		}
			

		Vector2 tan0, tan1;
		int LinkType = 0;		//	0:A左B右		1:A右B左		2:A上B下		3:A下B上
		bool right = pointA.x < pointB.x;
		bool down = pointA.y < pointB.y;

		if (A_Bigger) {
			pointA.y = down ? nodeA.canvasRect.yMax - 3 : nodeA.canvasRect.yMin + 3;
			pointB.x = right ? nodeB.canvasRect.xMin : nodeB.canvasRect.xMax;
			LinkType = right ? 0 : 1;		//right:left
			tan0 = new Vector2 (pointA.x, down ? pointB.y + 8 : pointB.y - 8);
			tan1 = new Vector2 (right ? pointA.x - 8 : pointA.x + 8, pointB.y);
			rectEdit.position = new Vector2(pointA.x, pointB.y) - (0.5f * editSize);
		} else {
			pointA.x = right ? nodeA.canvasRect.xMax - 3 : nodeA.canvasRect.xMin + 3;
			pointB.y = down ? nodeB.canvasRect.yMin : nodeB.canvasRect.yMax;
			LinkType = down ? 2 : 3;		//down:up
			tan0 = new Vector2 (right ? pointB.x + 8 : pointB.x - 8, pointA.y);
			tan1 = new Vector2 (pointB.x, down ? pointA.y - 8 : pointA.y + 8);
			rectEdit.position = new Vector2(pointB.x, pointA.y) - (0.5f * editSize);
		}

		Handles.DrawBezier (pointA, pointB, tan0, tan1, arrowColors [LinkType % 2], null, 2f);

		if(editable){
			GUI.DrawTexture (new Rect (pointB - (6f * LinkDirect [LinkType]) - (0.5f * arrowSize), arrowSize), tex_arrows [LinkType]);
			GUI.DrawTexture (rectEdit, tex_edits [LinkType % 2]);
		}
	}

	public void DeleteSelf(){
		nodeA.DeleteLink (false, this);
		nodeB.DeleteLink (true, this);
	}
}

public class ConditionUnit{
	public string condition;
	public QuestionNode myQuestion = null;
	char[] splitter = new char[]{'(', ')'};

	public ConditionUnit(string _con){
		condition = _con;
	}

	public void UpdateQuestion(){
		string[] conditionSplit = condition.Split (splitter);
		if (conditionSplit [0] == "Plot" || conditionSplit [0] == "Else")
			return;
		string newNodeName = myQuestion == null ? "N/A" : myQuestion.nodeName;
		condition = newNodeName + "(" + conditionSplit [1] + ")";
	}
}
#endregion

#region 左面板相關

public class LeftPanel{
	DialogueTree DTGod;
	GUIStyle titleStyle, editStyle;
	Rect rect_left, rect_titleL, rect_add;
	Texture2D tex_left, tex_add;
	Character selectedChar = null;

	public LeftPanel(DialogueTree _dt){
		GUISkin skin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		titleStyle = skin.GetStyle ("panelTitle");
		editStyle = skin.GetStyle ("editbutton");

		rect_left = new Rect (0, 0, 120, 90);
		rect_titleL = new Rect (10, 10, 100, 30);
		rect_add = new Rect (10, 69, 70, 11);

		tex_add = Resources.Load<Texture2D> ("GUISkin/Icons/AddCharacter");
		MyFunctions.SetTexture (ref tex_left, new Color (0, 0, 0, 0.2f));
		DTGod = _dt;
	}

	//process under mouse down event
	public bool HitTest(Event e){
		if (!rect_left.Contains (e.mousePosition)) {
			foreach (Character c in DTGod.lst_chars)
				c.Chosen (false);
			return false;
		}
		
		if (e.button == 0) {
			bool hitChar = false;
			for (int i = 1; i < DTGod.lst_chars.Count; i++) {
				Character _c = DTGod.lst_chars [i];
				if (_c.HitTest (e)) {
					if (_c != selectedChar)
						ResetSelected ();
					selectedChar = _c;
					hitChar = true;
					break;
				}
			}
			if (!hitChar)
				ResetSelected ();
		} else if (e.button == 1) {
			if (selectedChar != null && selectedChar.HitTest (e)) {
				GenericMenu menu = new GenericMenu ();
				menu.AddItem (new GUIContent ("Delete"), false, () => DeleteChar ());
				menu.ShowAsContext ();
			}
		}
		return true;
	}

	void ResetSelected(){
		if (selectedChar != null) {
			selectedChar.Chosen (false);
			selectedChar = null;
		}
	}

	void DeleteChar(){
		Character c = selectedChar;
		ResetSelected ();
		DTGod.RemoveChar (c);
		DTGod.rightPanel.SetNameArray ();
	}

	public void DrawSelf(){
		rect_left.height = 44 + 24 * DTGod.lst_chars.Count;
		GUI.DrawTexture (rect_left, tex_left);
		GUI.Label (rect_titleL, "對話角色名單", titleStyle);

		for (int i = 1; i < DTGod.lst_chars.Count; i++)
			DTGod.lst_chars [i].DrawSelf (i-1);

		rect_add.position = new Vector2 (10, 21 + DTGod.lst_chars.Count * 24);
		if (GUI.Button (rect_add, tex_add, editStyle)) {
			DTGod.lst_chars.Add (new Character (DTGod, "新角色", 0));
			DTGod.rightPanel.SetNameArray ();
		}
	}
}

public class Character{
	public string name;
	Texture2D[] tex_charColors = new Texture2D[8];
	Texture2D[] tex_nodeColors = new Texture2D[8];
	Texture2D tex_choose;
	GUIStyle nameStyle, fieldStyle, tagWStyle, tagBStyle;
	Rect rect = new Rect(6, 37, 110, 20), 
	rect_color = new Rect(8, 40, 18, 9), 
	rect_name = new Rect(32, 40, 85, 20), 
	rect_nameField = new Rect(29, 38, 85, 18);
	bool chosen = false;
	bool editName = false;

	public int colorIndex = 0;
	DialogueTree DTGod;

	public Character(DialogueTree _dt, string _name, int _index){
		GUISkin skin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		nameStyle = skin.GetStyle ("charName");
		fieldStyle = skin.GetStyle ("textfield");
		tagWStyle = skin.GetStyle ("tagnameW");
		tagBStyle = skin.GetStyle ("tagnameB");
		for (int i = 0; i < 8; i++) {
			tex_charColors[i] = Resources.Load<Texture2D> ("GUISkin/NodeColor/CharColor" + i.ToString ());
			tex_nodeColors[i] = Resources.Load<Texture2D> ("GUISkin/NodeColor/NodeColor" + i.ToString ());
		}
		MyFunctions.SetTexture (ref tex_choose, new Color (.35f, .35f, .35f, .9f));
		DTGod = _dt;
		name = MyFunctions.SetName (_name, this, DTGod.lst_chars);
		colorIndex = _index;
	}

	public bool HitTest(Event e){
		if (rect.Contains (e.mousePosition)) {
			if (chosen) {
				if (e.button == 0) {
					if (rect_color.Contains (e.mousePosition)) {
						DTGod.Popup (e.mousePosition, this);
					}
					if (rect_name.Contains (e.mousePosition))
						EditName (true);
					else
						EditName (false);
				}
			} else {
				Chosen (true);
			}
			return true;
		} else {
			Chosen (false);
			return false;
		}
	}

	public Texture2D GetNodeTag(){ return tex_nodeColors [colorIndex]; }
	public GUIStyle GetTagStyle(){
		if (colorIndex == 2 || colorIndex == 3 || colorIndex == 4)
			return tagWStyle;
		else
			return tagBStyle;
	}

	public void DrawSelf(int index){
		RefreshRect (index);
		if (chosen)
			GUI.DrawTexture (rect, tex_choose);
		GUI.DrawTexture (rect_color, tex_charColors [colorIndex]);

		if (editName) {
			GUI.skin.settings.cursorColor = Color.white;
			name = EditorGUI.TextField (rect_nameField, name, fieldStyle);
		} else
			GUI.Label (rect_name, name, nameStyle);
			
	}

	public void RefreshRect(int index){
		rect.y = 37 + index * 24;
		rect_color.y = rect.y + 6;
		rect_name.y = rect.y + 3;
		rect_nameField.y = rect.y + 1;
	}

	public void Chosen(bool isChosen){
		chosen = isChosen;
		if (!chosen && editName)
			EditName (false);
	}

	void EditName(bool isEditing){
		if (editName == isEditing)
			return;
		EditorGUI.FocusTextInControl ("");
		editName = isEditing;
		if (!isEditing) {
			name = MyFunctions.SetName (MyFunctions.ClampString (name, "新角色"), this, DTGod.lst_chars);
			DTGod.rightPanel.SetNameArray ();
		}
	}
}

public class ColorWindow{
	Texture2D tex_main, tex_check;
	Rect windowRect = new Rect (0, 0, 269, 19), 
	checkRect = new Rect(0, 0, 14, 14);
	Character nowChar = null;

	public ColorWindow(){
		tex_main = Resources.Load<Texture2D> ("GUISkin/NodeColor/ColorSelect");
		tex_check = Resources.Load<Texture2D> ("GUISkin/Icons/CheckIcon");
	}

	public void Popup(Vector2 _mousePos, Character _char){
		windowRect.position = _mousePos + 8 * Vector2.one;
		nowChar = _char;
		checkRect.position = windowRect.position + new Vector2 (19 + nowChar.colorIndex * 33, 8) - 0.5f * checkRect.size;
	}

	public void DrawSelf(){
		GUI.DrawTexture (windowRect, tex_main);
		GUI.DrawTexture (checkRect, tex_check);
	}

	public bool HitTest(Vector2 mousePos){
		if (windowRect.Contains (mousePos)) {
			int selectIndex = Mathf.Clamp ((int)((mousePos.x - windowRect.position.x - 2.5f) / 33f), 0, 7);
			if (selectIndex == nowChar.colorIndex)
				return true;
			nowChar.colorIndex = selectIndex;
			return false;
		} else
			return false;
	}
}

#endregion

#region 右面版相關
public class RightPanel{
	DialogueTree DTGod;
	GUIStyle style_subtitle, style_label, style_ttf, style_tf, style_dt, style_ta, style_disdropdown, style_button, style_edit;
	Texture2D tex_box, tex_bg, tex_scroller, tex_outline, tex_extent, tex_unextent, tex_edit, tex_add, tex_remove, tex_noRemove;
	Node prevSelect = null;
	Rect boxRect = new Rect (0, 0, 261, 70), 
	contentRect = new Rect(0, 0, 245, 60),
	scrollerRect = new Rect(0, 102, 9, 300),//y:2 ~ (boxRect.height-2)
	extentRect = new Rect (0, 0, 261, 15);

	List<QuestionNode> foundQNode;
	bool extent = false, scrollOn = false;
	float scrollPos = 0;
	string[] nameArray = new string[]{ "none" };

	public RightPanel(DialogueTree _dt){
		DTGod = _dt;
		MyFunctions.SetTexture (ref tex_box, new Color (0.1f, 0.1f, 0.1f, 0.7f));
		MyFunctions.SetTexture (ref tex_bg, new Color (0.7f, 0.7f, 0.7f, 0.18f));
		MyFunctions.SetTexture (ref tex_scroller, new Color (0.4f, 0.4f, 0.4f, 1));

		tex_outline = Resources.Load<Texture2D> ("GUISkin/ui_outline");
		tex_extent = Resources.Load<Texture2D> ("GUISkin/extent");
		tex_unextent = Resources.Load<Texture2D> ("GUISkin/unextent");
		tex_edit = Resources.Load<Texture2D> ("GUISkin/Icons/EditGear1");
		tex_add = Resources.Load<Texture2D> ("GUISkin/Icons/AddCondition");
		tex_remove = Resources.Load<Texture2D> ("GUISkin/Icons/RemoveIcon");
		tex_noRemove = Resources.Load<Texture2D> ("GUISkin/Icons/NoRemoveIcon");

		GUISkin skin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_subtitle = skin.GetStyle ("subtitle");
		style_label = skin.GetStyle ("label");
		style_ttf = skin.GetStyle ("titletextfield");
		style_tf = skin.GetStyle ("textfield");
		style_dt = skin.GetStyle ("disabledtext");
		style_ta = skin.GetStyle ("textarea");
		style_disdropdown = skin.GetStyle ("disableddropdown");
		style_button = skin.GetStyle ("button");
		style_edit = skin.GetStyle ("editbutton");

		SetQNodeList ();
	}

	#region main
	public void DrawSelf(Vector2 windowSize){
		if (DTGod.SelectNode == null)
			return;
		SetRects (windowSize);
		GUI.DrawTexture (boxRect, tex_box);
		GUI.BeginGroup (boxRect);
		GUI.BeginGroup (contentRect);
		DrawInfo (DTGod.SelectNode);
		GUI.EndGroup ();
		GUI.EndGroup ();
		if (extent) {
			if (contentRect.height > boxRect.height) {
				scrollOn = true;
				scrollerRect.height = (boxRect.height - 4) * boxRect.height / contentRect.height;
				float maxRange = contentRect.height - boxRect.height;
				scrollPos = Mathf.Clamp (scrollPos, 0, maxRange);
				//捲動調高度 = 捲動條最大移距*(現在移距/最大移距)
				scrollerRect.y = (boxRect.height - scrollerRect.height - 4) * scrollPos / maxRange + 2;
				GUI.DrawTexture (scrollerRect, tex_scroller);
			} else {
				scrollOn = false;
				scrollPos = 0;
			}
		} else {
			scrollOn = false;
			scrollerRect.y = 2;
		}
		if(CanExtent ())
			GUI.DrawTexture (extentRect, extent ? tex_unextent : tex_extent);
	}

	public bool HitTest(Vector2 mousePos){
		if (DTGod.SelectNode == null)
			return false;
		
		if (mousePos.x > boxRect.x) {
			if (extent || mousePos.y < boxRect.height+15) {
				HitEvent (mousePos);
				return true;
			}
		}
		return false;
	}

	void HitEvent(Vector2 mousePos){
		if (CanExtent ()) {
			if (extentRect.Contains (mousePos))
				extent = !extent;
			else if (scrollOn && scrollerRect.Contains (mousePos))
				DTGod.nowState = DialogueTree.WindowState.scroll;
		}
	}
	#endregion

	#region others
	public void SetNameArray(){
		nameArray = new string[DTGod.lst_chars.Count];
		nameArray[0] = "none";
		int i;
		for (i = 1; i < DTGod.lst_chars.Count; i++) {
			nameArray [i] = DTGod.lst_chars [i].name;
		}
	}

	public void SetQNodeList(){
		List<Node> tmp = new List<Node> (DTGod.lst_node.FindAll (n => n.GetType () == typeof(QuestionNode)));
		foundQNode = new List<QuestionNode> ();
		foreach (Node n in tmp)
			foundQNode.Add ((QuestionNode)n);
	}

	public void Scroll(float dHeight){
		scrollerRect.y += dHeight;
		scrollPos = (contentRect.height - boxRect.height) * (scrollerRect.y - 2) / (boxRect.height - scrollerRect.height - 4);
		scrollPos = Mathf.Clamp (scrollPos, 0, contentRect.height - boxRect.height);
	}

	public void Scroll(Event e){
		if (scrollOn && e.mousePosition.x > boxRect.x)
			Scroll (e.delta.y * 7);
	}

	void SetRects(Vector2 windowSize){
		boxRect.x = windowSize.x - 260;
		boxRect.height = extent ? windowSize.y - 15 : contentRect.height;
		contentRect.position = Mathf.Round(scrollPos) * Vector2.down;
		if (DTGod.SelectNode != prevSelect) {
			prevSelect = DTGod.SelectNode;
			scrollerRect.y = 2;
			scrollPos = 0;
		}
		if (CanExtent ()) {
			extentRect.x = boxRect.x;
			extentRect.y = boxRect.height;
		} else
			extent = false;
		scrollerRect.x = windowSize.x - 11;
	}

	bool CanExtent(){
		return (DTGod.SelectNode.GetType () != typeof(StartNode) && DTGod.SelectNode.GetType () != typeof(SubNode));
	}
	#endregion

	#region draw info
	void DrawInfo(Node n){
		switch (n.GetType ().ToString ()) {
		case "StartNode":
			StartNode sn = (StartNode)n;
			DrawInfo (sn);
			break;
		case "DialogueNode":
			DialogueNode dln = (DialogueNode)n;
			DrawInfo (dln);
			break;
		case "QuestionNode":
			QuestionNode qn = (QuestionNode)n;
			DrawInfo (qn);
			break;
		case "DivergeNode":
			DivergeNode dvn = (DivergeNode)n;
			DrawInfo (dvn);
			break;
		case "SubNode":
			SubNode sbn = (SubNode)n;
			DrawInfo (sbn);
			break;
		}
	}

	void DrawInfo(StartNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 49), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "劇情", style_subtitle);
		n.SetName (EditorGUI.TextField (new Rect (50, 15, 185, 24), n.nodeName, style_ttf));
		contentRect.height = 57;
	}

	void DrawInfo(DialogueNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 84), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "對話", style_subtitle);
		n.SetName (EditorGUI.TextField (new Rect (50, 15, 185, 24), n.nodeName, style_ttf));
		int index = DTGod.lst_chars.FindIndex (d => d == n.myCharacter);
		index = index < 0 ? 0 : index;
		GUI.Label (new Rect (15, 48, 30, 25), "角色", style_subtitle);
		n.myCharacter = DTGod.lst_chars [MyFunctions.GUIPopup (new Rect (51, 47, 184, 27), index, nameArray)];
		contentRect.height = 91;

		if (extent) {
			contentRect.height += 10;
			for (int i = 0; i < n.lst_dial.Count; i++) {
				if (GUI.Button (new Rect (contentRect.width - 15, contentRect.height + 5, 22, 22), tex_edit, style_edit))
					EditDialNode (n, i);
				DrawDialogue (n.lst_dial [i], "對話" + i.ToString ());
			}
			if (GUI.Button (new Rect (10, contentRect.height, 230, 40), "新增對話", style_button))
				n.lst_dial.Add (new Dialog ("", 12, "", ""));
			contentRect.height += 50;
		}
	}

	void DrawInfo(QuestionNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 84), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "問答", style_subtitle);
		n.SetName (EditorGUI.TextField (new Rect (50, 15, 185, 24), n.nodeName, style_ttf));
		int index = DTGod.lst_chars.FindIndex (d => d == n.myCharacter);
		index = index < 0 ? 0 : index;
		GUI.Label (new Rect (15, 48, 30, 25), "角色", style_subtitle);
		n.myCharacter = DTGod.lst_chars [MyFunctions.GUIPopup (new Rect (51, 47, 184, 27), index, nameArray)];
		contentRect.height = 91;

		if (extent) {
			contentRect.height += 10;
			DrawDialogue (n.questionDial, "問題");
			for (int i = 0; i < n.options.Count; i++)
				DrawOption (n, i);
			contentRect.height += 20;
			if (n.options.Count < 4) {
				if (GUI.Button (new Rect (10, contentRect.height, 230, 40), "新增選項", style_button))
					n.options.Add (new SubNode (DTGod, n.options [n.options.Count - 1].rect.position + new Vector2 (100, 0), "選項內容", n));
			} else
				GUI.Label (new Rect (10, contentRect.height, 230, 40), "選項數已達上限", style_label);
			
			contentRect.height += 50;
		}
	}

	void DrawInfo(DivergeNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 49), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "分歧", style_subtitle);
		n.SetName (EditorGUI.TextField (new Rect (50, 15, 185, 24), n.nodeName, style_ttf));
		contentRect.height = 57;
		if (extent) {
			contentRect.height += 10;
			for (int i = 0; i < n.diverges.Count; i++)
				DrawDiverge (n, i);
			contentRect.height += 20;
			if (GUI.Button (new Rect (10, contentRect.height, 230, 40), "新增分歧", style_button)) {
				int insertIndex = n.diverges.Count - 1;
				n.diverges.Insert (
					insertIndex, 
					new SubNode (
						DTGod, 
						n.diverges [Mathf.Clamp (insertIndex-1, 0, 100)].rect.position + new Vector2 (100, 0), 
						new List<ConditionUnit>{ new ConditionUnit ("Plot") }, 
						n
					)
				);

			}
			contentRect.height += 50;
		}
	}

	void DrawInfo(SubNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 49), tex_outline);
		string[] nameSplit = n.nodeName.Split (new char[]{ '\n' });
		GUI.Label (new Rect (15, 15, 40, 25), nameSplit[0], style_subtitle);
		GUI.Label (new Rect (55, 15, 180, 24), nameSplit[1], style_dt);
		contentRect.height = 57;
	}
	#endregion

	#region DrawElement
	void DrawDialogue(Dialog d, string _tag){
		float height = contentRect.height;
		height += 5;
		GUI.Label (new Rect (7, height, 50, 20), _tag, style_subtitle);
		height += 25;
		GUI.DrawTexture (new Rect (5, height, 240, 115), tex_bg);
		height += 5;
		GUI.Label (new Rect (10, height, 30, 18), "動作", style_label);
		d.animKey = EditorGUI.TextField (new Rect (40, height, 100, 18), d.animKey, style_tf).Replace (" ", "");
		GUI.Label (new Rect (150, height, 30, 18), "字體", style_label);
		d.size = EditorGUI.IntField (new Rect (180, height, 60, 18), d.size, style_tf);
		height += 25;
		d.text = EditorGUI.TextArea (new Rect (10, height, 230, 50), d.text, style_ta);
		height += 60;
		GUI.Label (new Rect (10, height, 30, 18), "指令", style_label);
		d.command = EditorGUI.TextField (new Rect (40, height, 200, 18), d.command, style_tf).Replace (" ", "");
		contentRect.height += 180;
	}

	void DrawOption(QuestionNode n, int index){
		SubNode optNode = n.options [index];
		if (n.options.Count > 1) {
			if (GUI.Button (new Rect (7, contentRect.height + 5, 12, 12), tex_remove, style_edit)) {
				n.DeleteOption (index);
				return;
			}
		} else
			GUI.DrawTexture (new Rect (7, contentRect.height + 5, 12, 12), tex_noRemove);
		GUI.Label (new Rect (25, contentRect.height+1, 40, 20), optNode.nodeName.Split(new char[]{'\n'})[0], style_subtitle);
		optNode.myOption = MyFunctions.ClampString(EditorGUI.TextField (new Rect (65, contentRect.height, 175, 24), optNode.myOption, style_tf), "選項內容");
		contentRect.height += 35;
	}

	//繪製分歧Unit
	void DrawDiverge(DivergeNode n, int index){
		SubNode diverNode = n.diverges [index];
		//右上角編輯鈕，僅用來移除
		bool elseCondition = diverNode.myDiverge [0].condition == "Else";
		if (!elseCondition) {
			if (GUI.Button (new Rect (contentRect.width - 15, contentRect.height, 22, 22), tex_edit, style_edit)) {
				n.DeleteDiverge (index);
				return;
			}
		} else
			GUI.Button (new Rect (contentRect.width - 15, contentRect.height, 22, 22), tex_edit, style_edit);
		GUI.Label (new Rect (7, contentRect.height + 1, 40, 20), diverNode.nodeName.Split(new char[]{'\n'})[0], style_subtitle);

		//繪製此分歧Unit的背景和各條件unit
		contentRect.height += 25;
		if (elseCondition) {
			GUI.DrawTexture (new Rect (5, contentRect.height, 240, 42), tex_bg);
			contentRect.height += 7;
			DrawCondition (diverNode, 0);
		} else {
			//背景
			GUI.DrawTexture (new Rect (5, contentRect.height, 240, 37 + diverNode.myDiverge.Count * 30), tex_bg);
			contentRect.height += 7;

			//各條件unit
			for(int i = 0; i < diverNode.myDiverge.Count; i++)
				DrawCondition (diverNode, i);
			contentRect.height += 5;

			//新增條件
			if (GUI.Button (new Rect (12, contentRect.height, 76, 13), tex_add, style_edit))
				diverNode.myDiverge.Add (new ConditionUnit ("Plot"));
		}
		contentRect.height += 60;
	}

	void DrawCondition(SubNode diverNode, int index){
		ConditionUnit nowCondition = diverNode.myDiverge [index];

	#region 讀取舊狀態
		string[] conditionSplit = nowCondition.condition.Split (new char[]{ '(', ')' }, System.StringSplitOptions.RemoveEmptyEntries);
		int typeIndex;
		if (conditionSplit [0] == "Plot")
			typeIndex = 0;
		else if (conditionSplit [0] == "Else")
			typeIndex = 2;
		else
			typeIndex = 1;
	#endregion

	#region 不可更改屬性的Else狀態
		if (typeIndex == 2) {
			GUI.DrawTexture (new Rect (12, contentRect.height + 6, 13, 13), tex_noRemove);
			GUI.Label (new Rect (32, contentRect.height, 63, 25), "Else", style_disdropdown);
			GUI.Label (new Rect (100, contentRect.height, 135, 25), "預設路線", style_dt);
			contentRect.height += 30;
			return;
		}
	#endregion

	#region 非Else狀態
		//移除按鈕
		if(diverNode.myDiverge.Count > 1){
			if(GUI.Button(new Rect (12, contentRect.height + 6, 13, 13), tex_remove, style_edit))
				diverNode.myDiverge.RemoveAt(index);
		}else
			GUI.DrawTexture (new Rect (12, contentRect.height + 6, 13, 13), tex_noRemove);

		//(劇情)(回答)下拉選單
		int newTypeIndex = typeIndex;
		newTypeIndex = MyFunctions.GUIPopup (new Rect (32, contentRect.height, 63, 25), newTypeIndex, diverNode.conditionType);


		if (newTypeIndex != typeIndex) {		//若選擇了新的狀態，則設定條件初始值
			if(newTypeIndex == 0)
				nowCondition.condition = "Plot";
			else
				nowCondition.condition = "N/A(-1)";
		}else{									//若沒有選擇新狀態，則根據現有狀態顯示內容
			if (typeIndex == 0) {					//Plot(Key)
				string key = EditorGUI.TextField (new Rect (100, contentRect.height, 135, 25), conditionSplit.Length > 1? conditionSplit [1]:"", style_tf);
				key = key.Split(new char[]{' '}, System.StringSplitOptions.RemoveEmptyEntries).Length == 0? "" : "(" + key + ")";
				nowCondition.condition = "Plot" + key;
			} else {								//Character:Question(key)
				int keyNumber = EditorGUI.IntField (new Rect (205, contentRect.height, 30, 25), "", int.Parse (conditionSplit [1]), style_tf);

				QuestionNode nowQNode = foundQNode.Find(n => n == nowCondition.myQuestion);
				string nowQNodeStr = "";
				if(nowQNode == null){
					nowQNodeStr = "N/A";
					nowCondition.myQuestion = null;
				}else
					nowQNodeStr = nowQNode.nodeName;
				nowCondition.condition = nowQNodeStr + "(" + keyNumber + ")";	

				if (MyFunctions.PopupMenu (new Rect (100, contentRect.height, 100, 25), nowQNodeStr))
					ConditionDropdown (nowCondition);
			}
			contentRect.height += 30;
		}
	#endregion

	}

	#endregion

	#region Dropdown
	void ConditionDropdown(ConditionUnit unit){
		GenericMenu menu = new GenericMenu ();
		List<Node> foundNode = new List<Node> (DTGod.lst_node.FindAll (n => n.GetType () == typeof(QuestionNode)));
		if (foundNode.Count == 0) {
			menu.AddDisabledItem (new GUIContent ("無已創建的問答集"));
		} else {
			foreach (Node n in foundNode) {
				QuestionNode qn = (QuestionNode)n;
				menu.AddItem (new GUIContent (qn.nodeName), false, () => unit.myQuestion = qn);
			}
		}
		menu.ShowAsContext ();
	}

	void EditDialNode(DialogueNode n, int index){
		EditorGUI.FocusTextInControl ("");
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("刪除"), false, () => n.lst_dial.RemoveAt (index));
		menu.AddItem (new GUIContent ("插入新對話"), false, 
			() => n.lst_dial.Insert (index + 1, new Dialog ("", 12, "", "")));
		menu.ShowAsContext ();
	}
	#endregion
 }
#endregion


