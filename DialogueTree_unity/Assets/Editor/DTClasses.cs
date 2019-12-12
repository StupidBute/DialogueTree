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
}

#region Nodes

public class Node {
	const float gridSize = 20;
	public Rect canvasRect;
	DialogueTree DTGod;
	Rect rect;
	protected GUISkin mySkin;
	protected GUIStyle style_name;
	List<NodeLink> NextLink = new List<NodeLink> ();
	List<NodeLink> PrevLink = new List<NodeLink> ();

	protected Texture2D tex_normal, tex_selected;
	protected Vector2 size = new Vector2 (140, 57);
	protected Rect NameRect = new Rect (10, 28, 120, 20);
	Vector2 dragPos;
	bool isSelected = false;

	public string nodeName = "Node Name";

	public Node(Vector2 _pos){
		SnapGrid (_pos);
		tex_selected = Resources.Load<Texture2D> ("GUISkin/Nodes/SelectedOutline");
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DialogueNode");
		mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_name = mySkin.GetStyle ("nodeName");
	}

#region Draw
	virtual public void DrawSelf(Vector2 coordinate){
		canvasRect = rect;
		canvasRect.position += coordinate;
		if (isSelected) {
			Rect outlineRect = canvasRect;
			outlineRect.position -= 3f * Vector2.one;
			outlineRect.size += 6 * Vector2.one;
			GUI.DrawTexture(outlineRect, tex_selected);
		}
		GUI.DrawTexture(canvasRect, tex_normal);


		Rect labelRect = NameRect;
		labelRect.position += canvasRect.position;
		GUI.Label (labelRect, nodeName, style_name);
	}

	public void DrawMyLink(){
		foreach (NodeLink nl in NextLink)
			nl.DrawSelf ();
  	}
#endregion

#region Hit
	public Node HitTest(Vector2 mousePos){
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
	public void Selected(bool _isSelected){
		isSelected = _isSelected;
		dragPos = rect.position;
		GUI.changed = true;
	}

	public void FollowMouse(Vector2 mouseDelta){
		dragPos += mouseDelta;
		SnapGrid (dragPos);
		GUI.changed = true;
	}

	public virtual void SetConnect(Node nextNode){
		NodeLink nl = new NodeLink (this, nextNode);
		if (NextLink.Count > 0)
			NextLink [0].DeleteSelf ();
			
		NextLink.Add (nl);
		nextNode.PrevLink.Add (nl);
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
		size = new Vector2 (121, 46);
		SnapGrid (_pos);
		NameRect = new Rect (10, 20, 101, 20);
		nodeName = "新劇情對話";
	}
}

public class DialogueNode : Node{
	public Character myCharacter;
	public List<Dialog> lst_dial = new List<Dialog> ();
	DialogueTree DTGod;
	Rect rect_tag = new Rect (0, 0, 95, 24);
	Rect rect_name = new Rect (0, 0, 70, 24);

	public DialogueNode(DialogueTree _dt, Vector2 _pos):base(_pos){
		nodeName = "新對話集";
		DTGod = _dt;
		myCharacter = DTGod.lst_chars [0];
		lst_dial.Add (new Dialog ("idle/12/0/X", ""));
	}

	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		rect_tag.position = rect_name.position = canvasRect.position;
		GUI.DrawTexture (rect_tag, myCharacter.GetNodeTag ());
		GUI.Label (rect_name, myCharacter.name, myCharacter.GetTagStyle ());
	}
}

public class QuestionNode : Node{
	public Character myCharacter;
	DialogueTree DTGod;
	Rect rect_tag = new Rect (0, 0, 95, 24);
	Rect rect_name = new Rect (0, 0, 70, 24);
	public QuestionNode(DialogueTree _dt, Vector2 _pos):base(_pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/QuestionNode");
		nodeName = "新問答集";
		DTGod = _dt;
		myCharacter = DTGod.lst_chars [0];
	}
	override public void DrawSelf(Vector2 coordinate){
		base.DrawSelf (coordinate);
		rect_tag.position = rect_name.position = canvasRect.position;
		GUI.DrawTexture (rect_tag, myCharacter.GetNodeTag ());
		GUI.Label (rect_name, myCharacter.name, myCharacter.GetTagStyle ());
	}
}

public class DivergeNode : Node{
	public DivergeNode(Vector2 _pos):base(_pos){
		tex_normal = Resources.Load<Texture2D> ("GUISkin/Nodes/DivergeNode");
		size = new Vector2 (109, 50);
		SnapGrid (_pos);
		NameRect = new Rect (10, 25, 89, 20);
		nodeName = "新分歧點";
	}
}

public class NodeLink{
	Node nodeA, nodeB;
	Rect rectEdit;

	Color[] arrowColors = new Color[]{ Color.white, new Color32 (250, 135, 255, 255) };
	Texture2D[] tex_arrows = new Texture2D[4];
	Texture2D[] tex_edits = new Texture2D[2];
	Vector2[] LinkDirect = new Vector2[]{ Vector2.right, Vector2.left, Vector2.up, Vector2.down };
	Vector2 arrowSize = new Vector2 (10, 10);
	Vector2 editSize = new Vector2 (15, 15);

	public NodeLink(Node na, Node nb){
		nodeA = na;
		nodeB = nb;

		tex_arrows [0] = Resources.Load<Texture2D> ("GUISkin/ArrowR");
		tex_arrows [1] = Resources.Load<Texture2D> ("GUISkin/ArrowL");
		tex_arrows [2] = Resources.Load<Texture2D> ("GUISkin/ArrowD");
		tex_arrows [3] = Resources.Load<Texture2D> ("GUISkin/ArrowU");
		tex_edits[0] = Resources.Load<Texture2D> ("GUISkin/EditGear1");
		tex_edits[1] = Resources.Load<Texture2D> ("GUISkin/EditGear2");
		rectEdit = new Rect (new Vector2 (nodeB.canvasRect.center.x - 0.5f * editSize.x, nodeB.canvasRect.yMax - 13 - editSize.y), editSize);
	}

	public bool HitTest(Vector2 mousePos){
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
				pointA.x = right ? nodeA.canvasRect.xMax - 5 : nodeA.canvasRect.xMin + 5;
				pointB.y = down ? nodeB.canvasRect.yMin + 4 : nodeB.canvasRect.yMax - 4;
				LinkType = down ? 2 : 3;
				tan0 = new Vector2(right? pointB.x+8 : pointB.x-8, pointA.y);
				tan1 = new Vector2(pointB.x, down? pointA.y-8 : pointA.y+8);
				rectEdit.position = new Vector2(pointB.x, pointA.y) - (0.5f * editSize);
				Handles.DrawBezier (pointA, pointB, tan0, tan1, arrowColors [LinkType % 2], null, 2f);
				GUI.DrawTexture (new Rect (pointB - (10f * LinkDirect [LinkType]) - (0.5f * arrowSize), arrowSize), tex_arrows [LinkType]);
				GUI.DrawTexture (rectEdit, tex_edits [LinkType % 2]);
				return;
			}
			if (pointA.x < pointB.x) {
				pointA.x = nodeA.canvasRect.xMax - 5;
				pointB.x = nodeB.canvasRect.xMin + 5;
				pointC = new Vector2 (Mathf.Clamp (pointB.x - 60f, pointA.x, 99999), pointA.y);
				LinkType = 0;
			} else {
				pointA.x = nodeA.canvasRect.xMin + 5;
				pointB.x = nodeB.canvasRect.xMax - 5;
				pointC = new Vector2 (Mathf.Clamp (pointB.x + 60f, -99999, pointA.x), pointA.y);
				LinkType = 1;
			}

		} else {
		//垂直分支
			if (pointA.y < pointB.y) {
				pointA.y = nodeA.canvasRect.yMax - 4;
				pointB.y = nodeB.canvasRect.yMin + 4;
				pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 80f, pointA.y, 99999));
				LinkType = 2;
			} else {
				pointA.y = nodeA.canvasRect.yMin + 4;
				pointB.y = nodeB.canvasRect.yMax - 4;
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
		GUI.DrawTexture (new Rect (pointB - (10f * LinkDirect [LinkType]) - (0.5f * arrowSize), arrowSize), tex_arrows [LinkType]);
		GUI.DrawTexture (rectEdit, tex_edits [LinkType % 2]);
	}

	public void DeleteSelf(){
		nodeA.DeleteLink (false, this);
		nodeB.DeleteLink (true, this);
		GUI.changed = true;
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

		tex_add = Resources.Load<Texture2D> ("GUISkin/AddIcon");
		MyFunctions.SetTexture (ref tex_left, new Color (0, 0, 0, 0.2f));
		DTGod = _dt;
	}

	//process under mouse down event
	public bool HitTest(Event e){
		if (!rect_left.Contains (e.mousePosition)) {
			foreach (Character c in DTGod.lst_chars)
				c.Chosen (false);
			GUI.changed = true;
			return false;
		}
		
		if (e.button == 0) {
			if (rect_add.Contains (e.mousePosition)) {
				DTGod.lst_chars.Add (new Character (DTGod));
				DTGod.rightPanel.SetNameArray ();
				GUI.changed = true;
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
				GUI.changed = true;
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
		GUI.changed = true;
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
		tex_check = Resources.Load<Texture2D> ("GUISkin/CheckIcon");
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
	GUIStyle style_title, style_subtitle, style_label, style_ttf, style_tf, style_ta, style_dropdown, style_button, style_edit;
	Texture2D tex_box, tex_bg, tex_scroller, tex_extent, tex_unextent, tex_edit;
	Node prevSelect = null;
	Rect boxRect = new Rect (0, 0, 265, 70), 
	contentRect = new Rect(0, 0, 240, 60),
	scrollerRect = new Rect(0, 102, 9, 300),//y:2 ~ (boxRect.height-2)
	extentRect = new Rect (0, 0, 260, 15);
	bool extent = false, scrollOn = false;
	float scrollPos = 0;

	string[] nameArray = new string[]{ "none" };

	public RightPanel(DialogueTree _dt){
		DTGod = _dt;
		MyFunctions.SetTexture (ref tex_box, new Color (0.1f, 0.1f, 0.1f, 0.7f));
		MyFunctions.SetTexture (ref tex_bg, new Color (0.7f, 0.7f, 0.7f, 0.18f));
		MyFunctions.SetTexture (ref tex_scroller, new Color (0.4f, 0.4f, 0.4f, 1));
		tex_extent = Resources.Load<Texture2D> ("GUISkin/extent");
		tex_unextent = Resources.Load<Texture2D> ("GUISkin/unextent");
		tex_edit = Resources.Load<Texture2D> ("GUISkin/EditGear1");
		GUISkin skin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_title = skin.GetStyle ("panelTitle");
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
		SetRects (windowSize);
		GUI.DrawTexture (boxRect, tex_box);
		if (DTGod.SelectNode != null) {
			GUI.DrawTexture (extentRect, extent ? tex_unextent : tex_extent);
			GUI.BeginGroup (boxRect);
				GUI.BeginGroup (contentRect);
					switch (DTGod.SelectNode.GetType ().ToString ()) {
					case "StartNode":
						StartNode sn = (StartNode)DTGod.SelectNode;
						DrawInfo (sn);
						break;
					case "DialogueNode":
						DialogueNode dln = (DialogueNode)DTGod.SelectNode;
						DrawInfo (dln);
						break;
					case "QuestionNode":
						QuestionNode qn = (QuestionNode)DTGod.SelectNode;
						DrawInfo (qn);
						break;
					case "DivergeNode":
						DivergeNode dvn = (DivergeNode)DTGod.SelectNode;
						DrawInfo (dvn);
						break;
					}
				GUI.EndGroup ();
			GUI.EndGroup ();
			if (extent) {
				if (contentRect.height > boxRect.height) {
					scrollOn = true;
					scrollerRect.height = (boxRect.height - 4) * boxRect.height / contentRect.height;
					scrollerRect.y = scrollPos * (boxRect.height - scrollerRect.height - 4) / (contentRect.height + 8 - boxRect.height) + 2;
					GUI.DrawTexture (scrollerRect, tex_scroller);
				} else
					scrollOn = false;
			} else {
				scrollOn = false;
				scrollerRect.y = 2;
			}
				
		} else {
			Rect titleRect = boxRect;
			titleRect.position += new Vector2 (5, 8);
			GUI.Label (titleRect, "節點檢視視窗", style_title);
		}
			
	}

	public bool HitTest(Vector2 mousePos){
		if (mousePos.x > boxRect.x) {
			if (extent || mousePos.y < boxRect.height+15) {
				HitEvent (mousePos);
				return true;
			}
		}
		return false;
	}

	void HitEvent(Vector2 mousePos){
		if (DTGod.SelectNode != null) {
			if (extentRect.Contains (mousePos))
				extent = !extent;
			else if (scrollOn && scrollerRect.Contains (mousePos))
				DTGod.nowState = DialogueTree.WindowState.scroll;
			GUI.changed = true;
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
		scrollPos = (contentRect.height + 8 - boxRect.height) * (scrollerRect.y - 2) / (boxRect.height - scrollerRect.height - 4);
		scrollPos = Mathf.Clamp (scrollPos, 0, contentRect.height + 8 - boxRect.height);
		GUI.changed = true;
	}

	public void Scroll(Event e){
		if (e.mousePosition.x > boxRect.x)
			Scroll (e.delta.y * 7);
	}

	void SetRects(Vector2 windowSize){
		boxRect.x = windowSize.x - 260;
		boxRect.height = DTGod.SelectNode == null ? 30 : (extent ? windowSize.y - 15 : contentRect.height+16);
		contentRect.position = new Vector2 (5, 8) + Mathf.Round(scrollPos) * Vector2.down;
		if (DTGod.SelectNode != null) {
			if (DTGod.SelectNode != prevSelect) {
				prevSelect = DTGod.SelectNode;
				scrollerRect.y = 2;
			}

			extentRect.x = boxRect.x;
			extentRect.y = boxRect.height;
		}
		scrollerRect.x = windowSize.x - 11;
	}
	#endregion

	#region draw info
	void DrawInfo(StartNode n){
		GUI.Label (new Rect (5, 0, 30, 25), "劇情", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (40, 0, 195, 24), DTGod.SelectNode.nodeName, style_ttf);
		contentRect.height = 25;
	}

	void DrawInfo(DialogueNode n){
		GUI.Label (new Rect (5, 0, 30, 25), "對話", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (40, 0, 195, 24), DTGod.SelectNode.nodeName, style_ttf);
		contentRect.height = 60;
		int index = DTGod.lst_chars.FindIndex (d => d == n.myCharacter);
		index = index < 0 ? 0 : index;
		GUI.Label (new Rect (5, 33, 30, 25), "角色", style_subtitle);
		n.myCharacter = DTGod.lst_chars [EditorGUI.Popup (new Rect (41, 32, 194, 27), index, nameArray, style_dropdown)];
		if (extent) {
			contentRect.height += 40;
			for (int i = 0; i < n.lst_dial.Count; i++)
				DrawDialogue (n, i);
			if (GUI.Button (new Rect (5, contentRect.height, 230, 40), "新增對話", style_button))
				n.lst_dial.Add (new Dialog ("idle/12/0/X", ""));
			contentRect.height += 50;
			GUI.changed = true;
		}
	}

	void DrawInfo(QuestionNode n){
		GUI.Label (new Rect (5, 0, 30, 25), "問答", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (40, 0, 195, 24), DTGod.SelectNode.nodeName, style_ttf);
		contentRect.height = 60;
		int index = DTGod.lst_chars.FindIndex (d => d == n.myCharacter);
		index = index < 0 ? 0 : index;
		GUI.Label (new Rect (5, 33, 30, 25), "角色", style_subtitle);
		n.myCharacter = DTGod.lst_chars [EditorGUI.Popup (new Rect (41, 32, 194, 27), index, nameArray, style_dropdown)];
	}

	void DrawInfo(DivergeNode n){
		GUI.Label (new Rect (5, 0, 30, 25), "分歧", style_subtitle);
		DTGod.SelectNode.nodeName = EditorGUI.TextField (new Rect (40, 0, 195, 24), DTGod.SelectNode.nodeName, style_ttf);
		contentRect.height = 25;
	}

	void DrawDialogue(DialogueNode n, int index){
		Dialog d = n.lst_dial [index];
		float height = contentRect.height;
		height += 5;
		GUI.Label (new Rect (2, height, 50, 20), "對話" + index.ToString (), style_subtitle);
		if (GUI.Button (new Rect (contentRect.width - 15, height, 22, 22), tex_edit, style_edit))
			EditDialNode (n, index);
		height += 25;
		GUI.DrawTexture (new Rect (0, height, 240, 115), tex_bg);
		height += 5;
		GUI.Label (new Rect (5, height, 30, 18), "動作", style_label);
		d.animKey = EditorGUI.TextField (new Rect (35, height, 60, 18), d.animKey, style_tf);
		GUI.Label (new Rect (105, height, 30, 18), "字體", style_label);
		d.size = EditorGUI.IntField (new Rect (135, height, 30, 18), d.size, style_tf);
		GUI.Label (new Rect (175, height, 30, 18), "好感", style_label);
		d.intimacy = EditorGUI.IntField (new Rect (205, height, 30, 18), d.intimacy, style_tf);
		height += 25;
		d.text = EditorGUI.TextArea (new Rect (5, height, 230, 50), d.text, style_ta);
		height += 60;
		GUI.Label (new Rect (5, height, 30, 18), "指令", style_label);
		d.command = EditorGUI.TextField (new Rect (35, height, 200, 18), d.command, style_tf);
		contentRect.height += 180;
	}

	void EditDialNode(DialogueNode n, int index){
		EditorGUI.FocusTextInControl ("");
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Delete"), false, () => n.lst_dial.RemoveAt (index));
		menu.AddItem (new GUIContent ("Insert New"), false, 
			() => n.lst_dial.Insert (index + 1, new Dialog ("idle/12/0/X", "")));
		menu.ShowAsContext ();
	}
	#endregion
 }
#endregion

