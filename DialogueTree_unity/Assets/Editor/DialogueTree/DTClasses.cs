using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyFunctions{
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

	public static T Cast<T>(object o){
		return (T)o;
	}
}

#region Nodes

public class Node {
	const float gridSize = 20;
	DialogueTree DTGod;
	public Rect rect;
	public Rect canvasRect;
	protected GUISkin mySkin;
	protected GUIStyle style_name;
	protected List<NodeLink> NextLink = new List<NodeLink> ();
	protected List<NodeLink> PrevLink = new List<NodeLink> ();

	protected Texture2D tex_normal, tex_selected;
	protected Vector2 size = new Vector2 (140, 58);
	protected Rect NameRect = new Rect (13, 31, 120, 20), outlineRect, boxRect;
	Vector2 dragPos;
	bool isSelected = false;

	public string nodeName = "Node Name";

	public Node(Vector2 _pos){
		SetRects (_pos);
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
			return this;
		} else
			return null;
	}

	public bool HitLinkTest(Vector2 mousePos){
		foreach (NodeLink _link in NextLink) {
			if (_link.HitTest (mousePos))
				return true;
		}
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
		bool canMultiLink = (this.GetType () == typeof(QuestionNode) || this.GetType () == typeof(DivergeNode)) ? true : false;
		NodeLink nl = new NodeLink (this, nextNode, canMultiLink);
		NextLink.Add (nl);
		nextNode.PrevLink.Add (nl);
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
#endregion
}

public class StartNode : Node{
	public StartNode(Vector2 _pos):base(_pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/NewStart");
		size = new Vector2 (120, 46);
		SetRects (_pos);
		NameRect = new Rect (13, 23, 101, 20);
		nodeName = "新劇情對話";
	}
}

public class DialogueNode : Node{
	public Character myCharacter;
	public List<Dialog> lst_dial = new List<Dialog> ();
	DialogueTree DTGod;
	Rect rect_tag = new Rect (3, 3, 95, 24);
	Rect rect_name = new Rect (3, 3, 70, 24);

	public DialogueNode(DialogueTree _dt, Vector2 _pos):base(_pos){
		nodeName = "新對話集";
		DTGod = _dt;
		myCharacter = DTGod.lst_chars [0];
		lst_dial.Add (new Dialog ("idle", 12, "X", ""));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		//rect_tag.position = rect_name.position = canvasRect.position;
		GUI.BeginGroup(canvasRect);
		GUI.DrawTexture (rect_tag, myCharacter.GetNodeTag ());
		GUI.Label (rect_name, myCharacter.name, myCharacter.GetTagStyle ());
		GUI.EndGroup ();
	}
}

public class QuestionNode : Node{
	public Character myCharacter;
	public Dialog questionDial = new Dialog ("idle", 12, "X", "");
	public List<SubNode> options = new List<SubNode> ();
	DialogueTree DTGod;
	Rect rect_tag = new Rect (3, 3, 95, 24);
	Rect rect_name = new Rect (3, 3, 70, 24);


	public QuestionNode(DialogueTree _dt, Vector2 _pos):base(_pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/QuestionNode");
		nodeName = "新問答集";
		DTGod = _dt;
		myCharacter = DTGod.lst_chars [0];
		options.Add (new SubNode (rect.center + new Vector2 (-85, 65), new Option ("選項內容", "END"), this));
		options.Add (new SubNode (rect.center + new Vector2 (10, 65), new Option ("選項內容", "END"), this));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		GUI.BeginGroup(canvasRect);
		GUI.DrawTexture (rect_tag, myCharacter.GetNodeTag ());
		GUI.Label (rect_name, myCharacter.name, myCharacter.GetTagStyle ());
		GUI.EndGroup ();
		foreach (SubNode optNode in options)
			optNode.DrawSelf (coordinate);
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
}

public class DivergeNode : Node{
	public List<SubNode> diverges = new List<SubNode> ();

	public DivergeNode(Vector2 _pos):base(_pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DivergeNode");
		size = new Vector2 (110, 50);
		SetRects (_pos);
		NameRect = new Rect (13, 28, 89, 20);
		nodeName = "新分歧點";
		List<string> defaultCondition = new List<string> ();
		defaultCondition.Add ("Else");
		diverges.Add (new SubNode (rect.center + new Vector2 (-35, 65), new DivergeUnit (defaultCondition, "END"), this));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		foreach (SubNode diverNode in diverges)
			diverNode.DrawSelf (coordinate);
	}

	override public Node HitTest(Vector2 mousePos){
		if (canvasRect.Contains (mousePos)) {
			Selected (true);
			return this;
		} else {
			Node returnNode = null;
			foreach (SubNode sn in diverges)
				returnNode = sn.HitTest (mousePos);
			return returnNode;
		}
	}

	override public void DrawMyLink(){
		base.DrawMyLink ();
		foreach (SubNode sn in diverges)
			sn.DrawMyLink ();
	}
}

public class NodeLink{
	Node nodeA, nodeB;
	Rect rectEdit;
	bool editable = true;

	Color[] arrowColors = new Color[]{ Color.white, new Color32 (250, 135, 255, 255) };
	Texture2D[] tex_arrows = new Texture2D[4];
	Texture2D[] tex_edits = new Texture2D[2];
	Vector2[] LinkDirect = new Vector2[]{ Vector2.right, Vector2.left, Vector2.up, Vector2.down };
	Vector2 arrowSize = new Vector2 (10, 10);
	Vector2 editSize = new Vector2 (15, 15);

	public NodeLink(Node na, Node nb, bool canMultiLink){
		nodeA = na;
		nodeB = nb;
		editable = !canMultiLink;

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

		//判別方向
		if (Mathf.Abs (pointA.y - pointB.y) < 70) {
		//水平分支
			if (Mathf.Abs (pointA.x - pointB.x) < 158){
				bool right = pointA.x < pointB.x;
				bool down = pointA.y < pointB.y;
				pointA.x = right ? nodeA.canvasRect.xMax : nodeA.canvasRect.xMin;
				pointB.y = down ? nodeB.canvasRect.yMin : nodeB.canvasRect.yMax;
				LinkType = down ? 2 : 3;
				tan0 = new Vector2(right? pointB.x+8 : pointB.x-8, pointA.y);
				tan1 = new Vector2(pointB.x, down? pointA.y-8 : pointA.y+8);
				rectEdit.position = new Vector2(pointB.x, pointA.y) - (0.5f * editSize);
				Handles.DrawBezier (pointA, pointB, tan0, tan1, arrowColors [LinkType % 2], null, 2f);
				if(editable){
					GUI.DrawTexture (new Rect (pointB - (6f * LinkDirect [LinkType]) - (0.5f * arrowSize), arrowSize), tex_arrows [LinkType]);
					GUI.DrawTexture (rectEdit, tex_edits [LinkType % 2]);
				}
				return;
			}
			if (pointA.x < pointB.x) {
				pointA.x = nodeA.canvasRect.xMax;
				pointB.x = nodeB.canvasRect.xMin;
				pointC = new Vector2 (Mathf.Clamp (pointB.x - 60f, pointA.x, 99999), pointA.y);
				LinkType = 0;
			} else {
				pointA.x = nodeA.canvasRect.xMin;
				pointB.x = nodeB.canvasRect.xMax;
				pointC = new Vector2 (Mathf.Clamp (pointB.x + 60f, -99999, pointA.x), pointA.y);
				LinkType = 1;
			}

		} else {
		//垂直分支
			if (pointA.y < pointB.y) {
				pointA.y = nodeA.canvasRect.yMax;
				pointB.y = nodeB.canvasRect.yMin;
				pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 80f, pointA.y, 99999));
				LinkType = 2;
			} else {
				pointA.y = nodeA.canvasRect.yMin;
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

	public void DeleteSelf(){
		nodeA.DeleteLink (false, this);
		nodeB.DeleteLink (true, this);
	}
}

public class SubNode : Node{
	public Option myOption = null;
	public DivergeUnit myDiverge = null;

	public SubNode(Vector2 _pos, Option opt, Node prevNode):base(_pos){
		myOption = opt;
		prevNode.SetConnect (this);
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/OptionUnit");
		tex_selected = Resources.Load<Texture2D> ("GUISkin/blank");
		size = new Vector2 (70, 44);
		SetRects (_pos);
		NameRect = new Rect (7, 7, 62, 36);
		nodeName = myOption.text;
		style_name = mySkin.GetStyle ("optionunit");
	}

	public SubNode(Vector2 _pos, DivergeUnit diver, Node prevNode):base(_pos){
		myDiverge = diver;
		prevNode.SetConnect (this);
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DivergeUnit");
		tex_selected = Resources.Load<Texture2D> ("GUISkin/blank");
		size = new Vector2 (70, 44);
		SetRects (_pos);
		NameRect = new Rect (7, 7, 62, 36);
		nodeName = myDiverge.conditions [0];
		style_name = mySkin.GetStyle ("divergeunit");
	}

	override public void DrawSelf(Vector2 coordinate){
		if (myOption != null)
			nodeName = myOption.text;
		else
			nodeName = myDiverge.conditions [0];
		base.DrawSelf (coordinate);
	}
}

#endregion

#region 左面板相關

public class LeftPanel{
	DialogueTree DTGod;
	GUIStyle titleStyle, labelStyle;
	Rect rect_left, rect_titleL, rect_add;
	Texture2D tex_left, tex_add;
	Character selectedChar = null;

	public LeftPanel(DialogueTree _dt){
		GUISkin skin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		titleStyle = skin.GetStyle ("panelTitle");
		labelStyle = skin.GetStyle ("label");

		rect_left = new Rect (0, 0, 120, 90);
		rect_titleL = new Rect (10, 10, 100, 30);
		rect_add = new Rect (10, 69, 75, 15);

		tex_add = Resources.Load<Texture2D> ("GUISkin/Icons/AddIcon");
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
			if (rect_add.Contains (e.mousePosition)) {
				DTGod.lst_chars.Add (new Character (DTGod));
				DTGod.rightPanel.SetNameArray ();
			} else {
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
			}
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
		DTGod.lst_chars.Remove (c);
		DTGod.rightPanel.SetNameArray ();
	}

	public void DrawSelf(){
		rect_left.height = 44 + 24 * DTGod.lst_chars.Count;
		GUI.DrawTexture (rect_left, tex_left);
		GUI.Label (rect_titleL, "對話角色名單", titleStyle);

		for (int i = 1; i < DTGod.lst_chars.Count; i++)
			DTGod.lst_chars [i].DrawSelf (i-1);
		
		rect_add = new Rect (10, 21 + DTGod.lst_chars.Count * 24, 75, 15);
		GUI.DrawTexture (new Rect (10, 21 + DTGod.lst_chars.Count * 24, 12, 12), tex_add);
		GUI.Label (new Rect (28, 16 + DTGod.lst_chars.Count * 24, 65, 20), "新增角色", labelStyle);
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

	public Character(DialogueTree _dt){
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
		name = SetName (DTGod.lst_chars.Count == 0 ? "N/A" : "新角色");
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
		EditorGUI.FocusTextInControl ("");
		editName = isEditing;
		if (!isEditing) {
			name = SetName (name);
			DTGod.rightPanel.SetNameArray ();
		}
	}

	string SetName(string _name){
		foreach (Character c in DTGod.lst_chars) {
			if (c != this && c.name == _name) {
				string[] sepName = _name.Split (new char[]{ ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
				int number;
				if (int.TryParse (sepName [sepName.Length - 1], out number)) {
					string result = "";
					for (int i = 0; i < sepName.Length - 1; i++)
						result += sepName [i] + " ";
					return SetName (result + (number + 1).ToString ());
				}else
					return SetName (_name + " 1");
			}
		}
		return _name;
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
	GUIStyle style_subtitle, style_label, style_ttf, style_tf, style_ta, style_dropdown, style_button, style_edit;
	Texture2D tex_box, tex_bg, tex_scroller, tex_outline, tex_extent, tex_unextent, tex_edit, tex_remove, tex_noRemove;
	Node prevSelect = null;
	Rect boxRect = new Rect (0, 0, 261, 70), 
	contentRect = new Rect(0, 0, 245, 60),
	scrollerRect = new Rect(0, 102, 9, 300),//y:2 ~ (boxRect.height-2)
	extentRect = new Rect (0, 0, 261, 15);

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
		tex_remove = Resources.Load<Texture2D> ("GUISkin/Icons/RemoveIcon");
		tex_noRemove = Resources.Load<Texture2D> ("GUISkin/Icons/NoRemoveIcon");

		GUISkin skin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_subtitle = skin.GetStyle ("subtitle");
		style_label = skin.GetStyle ("label");
		style_ttf = skin.GetStyle ("titletextfield");
		style_tf = skin.GetStyle ("textfield");
		style_ta = skin.GetStyle ("textarea");
		style_dropdown = skin.GetStyle ("dropdown");
		style_button = skin.GetStyle ("button");
		style_edit = skin.GetStyle ("editbutton");
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
		if (CanExtent ()) {
			if (DTGod.SelectNode != prevSelect) {
				prevSelect = DTGod.SelectNode;
				scrollerRect.y = 2;
				scrollPos = 0;
			}

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
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (50, 15, 185, 24), DTGod.SelectNode.nodeName, style_ttf);
		contentRect.height = 57;
	}

	void DrawInfo(DialogueNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 84), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "對話", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (50, 15, 185, 24), DTGod.SelectNode.nodeName, style_ttf);
		int index = DTGod.lst_chars.FindIndex (d => d == n.myCharacter);
		index = index < 0 ? 0 : index;
		GUI.Label (new Rect (15, 48, 30, 25), "角色", style_subtitle);
		n.myCharacter = DTGod.lst_chars [EditorGUI.Popup (new Rect (51, 47, 184, 27), index, nameArray, style_dropdown)];
		contentRect.height = 91;

		if (extent) {
			contentRect.height += 40;
			for (int i = 0; i < n.lst_dial.Count; i++) {
				if (GUI.Button (new Rect (contentRect.width - 15, contentRect.height + 5, 22, 22), tex_edit, style_edit))
					EditDialNode (n, i);
				DrawDialogue (n.lst_dial [i], "對話" + i.ToString ());
			}
			if (GUI.Button (new Rect (10, contentRect.height, 230, 40), "新增對話", style_button))
				n.lst_dial.Add (new Dialog ("idle", 12, "X", ""));
			contentRect.height += 50;
		}
	}

	void DrawInfo(QuestionNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 84), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "問答", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (50, 15, 185, 24), DTGod.SelectNode.nodeName, style_ttf);
		int index = DTGod.lst_chars.FindIndex (d => d == n.myCharacter);
		index = index < 0 ? 0 : index;
		GUI.Label (new Rect (15, 48, 30, 25), "角色", style_subtitle);
		n.myCharacter = DTGod.lst_chars [EditorGUI.Popup (new Rect (51, 47, 184, 27), index, nameArray, style_dropdown)];
		contentRect.height = 91;

		if (extent) {
			contentRect.height += 40;
			DrawDialogue (n.questionDial, "問題");
			float height = contentRect.height;
			for (int i = 0; i < n.options.Count; i++)
				DrawQuestion (n, i);
			contentRect.height += 20;
			if (n.options.Count < 4) {
				if (GUI.Button (new Rect (10, contentRect.height, 230, 40), "新增選項", style_button))
					n.options.Add (new SubNode (n.options [n.options.Count - 1].rect.position + new Vector2 (100, 0), new Option ("", ""), n));
			} else
				GUI.Label (new Rect (10, contentRect.height, 230, 40), "選項數已達上限", style_label);
			
			contentRect.height += 50;
		}
	}

	void DrawInfo(DivergeNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 49), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), "分歧", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (50, 15, 185, 24), DTGod.SelectNode.nodeName, style_ttf);
		contentRect.height = 57;
	}

	void DrawInfo(SubNode n){
		GUI.DrawTexture (new Rect (5, 4, 240, 49), tex_outline);
		GUI.Label (new Rect (15, 15, 30, 25), n.myOption != null ? "選項" : "分歧", style_subtitle);
		GUI.Label (new Rect (50, 15, 185, 24), DTGod.SelectNode.nodeName, style_ttf);
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
		d.animKey = EditorGUI.TextField (new Rect (40, height, 100, 18), d.animKey, style_tf);
		GUI.Label (new Rect (155, height, 30, 18), "字體", style_label);
		d.size = EditorGUI.IntField (new Rect (180, height, 60, 18), d.size, style_tf);
		height += 25;
		d.text = EditorGUI.TextArea (new Rect (10, height, 230, 50), d.text, style_ta);
		height += 60;
		GUI.Label (new Rect (10, height, 30, 18), "指令", style_label);
		d.command = EditorGUI.TextField (new Rect (40, height, 200, 18), d.command, style_tf);
		contentRect.height += 180;
	}

	void DrawQuestion(QuestionNode n, int index){
		if (n.options.Count > 2) {
			if (GUI.Button (new Rect (7, contentRect.height + 5, 12, 12), tex_remove, style_edit)) {
				n.options [index].DeleteAllConnect ();
				n.options.RemoveAt (index);
				return;
			}
		} else
			GUI.DrawTexture (new Rect (7, contentRect.height + 5, 12, 12), tex_noRemove);
			
		GUI.Label (new Rect (25, contentRect.height+1, 40, 20), "選項" + index.ToString (), style_subtitle);
		n.options [index].myOption.text = EditorGUI.TextField (new Rect (65, contentRect.height, 175, 24), n.options [index].myOption.text, style_tf);
		contentRect.height += 35;
	}
	#endregion

	void EditDialNode(DialogueNode n, int index){
		EditorGUI.FocusTextInControl ("");
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Delete"), false, () => n.lst_dial.RemoveAt (index));
		menu.AddItem (new GUIContent ("Insert New"), false, 
			() => n.lst_dial.Insert (index + 1, new Dialog ("idle", 12, "X", "")));
		menu.ShowAsContext ();
	}
 }
#endregion

