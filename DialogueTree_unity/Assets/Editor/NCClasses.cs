using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyMathf{
	public static Vector2 SnapPos(Vector2 position, float range){
		position.x = Mathf.Round (position.x / range) * range;
		position.y = Mathf.Round (position.y / range) * range;
		return position;
	}
}

public class Node {
	public Rect canvasRect;
	List<NodeLink> NextLink = new List<NodeLink> ();
	List<NodeLink> PrevLink = new List<NodeLink> ();
	public Rect rect;
	public bool isEnd = false;
	string charName;

	GUIStyle style_label, style_box, style_selectedBox;
	Texture2D tex_end;
	Vector2 dragPos;

	const float gridSize = 20;
	protected bool isSelected = false;

	public Node(Vector2 _pos, string _title, GUISkin mySkin){
		_pos = MyMathf.SnapPos (_pos, gridSize);
		rect = new Rect (_pos, new Vector2 (156, 56));
		canvasRect = rect;
		charName = _title;

		if (mySkin != null) {
			style_label = mySkin.GetStyle ("textfield");
			style_box = mySkin.GetStyle ("box");
			style_selectedBox = mySkin.GetStyle ("selectedBox");
			tex_end = Resources.Load<Texture2D> ("GUISkin/EndNode2");

		}

	}

	virtual public void DrawSelf(Vector2 coordinate){
		if (NextLink.Count == 0 && PrevLink.Count > 0) {
			isEnd = true;
			Rect endRect = new Rect (canvasRect.position + 37 * Vector2.up, new Vector2 (156, 33));
			GUI.DrawTexture (endRect, tex_end);
		} else
			isEnd = false;

		canvasRect = rect;
		canvasRect.position += coordinate;
		if (isSelected)
			GUI.Box (canvasRect, charName, style_selectedBox);
		else
			GUI.Box (canvasRect, charName, style_box);

		Rect labelRect = canvasRect;
		labelRect.position += new Vector2 (10, 25);
		labelRect.size = new Vector2 (136, 20);
		GUI.Label (labelRect, "對話內容", style_label);
	}

	public void DrawMyLink(){
		foreach (NodeLink nl in NextLink)
			nl.DrawSelf ();
	}

	public Node HitTest(Vector2 mousePos){
		if (canvasRect.Contains (mousePos)) {
			Selected (true);
			return this;
		} else
			return null;
	}

	public void Selected(bool _isSelected){
		isSelected = _isSelected;
		dragPos = rect.position;
		GUI.changed = true;
	}

	public void FollowMouse(Vector2 mouseDelta){
		dragPos += mouseDelta;
		rect.position = new Vector2 (Mathf.Round (dragPos.x / gridSize) * gridSize, Mathf.Round (dragPos.y / gridSize) * gridSize);
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

}

public class StartNode : Node{
	protected Texture2D normTex, selectTex;

	public StartNode(Vector2 _pos, string _text, GUISkin mySkin):base(_pos, _text, mySkin){
		normTex = Resources.Load<Texture2D> ("GUISkin/StartNode");
		selectTex = Resources.Load<Texture2D> ("GUISkin/StartNodeSelect");
		rect.size = new Vector2 (156, 46);
	}

	override public void DrawSelf(Vector2 coordinate){
		canvasRect = rect;
		canvasRect.position += coordinate;
		if (isSelected)
			GUI.DrawTexture (canvasRect, selectTex);
		else
			GUI.DrawTexture (canvasRect, normTex);
	}
}

public class NodeLink{
	Node nodeA, nodeB;
	Rect rectEdit;

	Texture2D[] tex_arrows = new Texture2D[4];
	Texture2D tex_setting;
	Vector2 arrowSize = new Vector2 (10, 10);
	Vector2 settingSize = new Vector2 (15, 15);
	Color arrowColors = new Color32 (250, 135, 255, 255);

	public NodeLink(Node na, Node nb){
		nodeA = na;
		nodeB = nb;

		rectEdit = new Rect (new Vector2 (nodeB.canvasRect.center.x - 0.5f * settingSize.x, nodeB.canvasRect.yMax - 13 - settingSize.y), settingSize);
		tex_arrows [0] = Resources.Load<Texture2D> ("GUISkin/ArrowD");
		tex_arrows [1] = Resources.Load<Texture2D> ("GUISkin/ArrowU");
		tex_arrows [2] = Resources.Load<Texture2D> ("GUISkin/ArrowR");
		tex_arrows [3] = Resources.Load<Texture2D> ("GUISkin/ArrowL");
		tex_setting = Resources.Load<Texture2D> ("GUISkin/SettingGear");
	}

	public void DrawSelf(){
		Vector2 pointA = nodeA.canvasRect.center;
		Vector2 pointB = nodeB.canvasRect.center;
		Vector2 pointC = pointA;
		Color resultColor = Color.white;
		float dWidth = Mathf.Abs (pointA.x - pointB.x);
		if (Mathf.Abs (pointA.y - pointB.y) < 56) {
			
		#region 水平分支
			if (dWidth < 158)
				return;
			if (pointA.x < pointB.x) {
				pointA.x = nodeA.canvasRect.xMax - 5;
				pointB.x = nodeB.canvasRect.xMin + 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-13, -5), arrowSize), tex_arrows [2]);
				pointC = new Vector2 (Mathf.Clamp (pointB.x - 60f, pointA.x, 99999), pointA.y);
			} else {
				pointA.x = nodeA.canvasRect.xMin + 5;
				pointB.x = nodeB.canvasRect.xMax - 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (3, -5), arrowSize), tex_arrows [3]);
				pointC = new Vector2 (Mathf.Clamp (pointB.x + 60f, -99999, pointA.x), pointA.y);
				resultColor = arrowColors;
			}
			Vector2 tan0 = new Vector2 (pointB.x, pointA.y);
			Vector2 tan1 = new Vector2 (pointC.x, pointB.y);
			if (Mathf.Abs (pointA.x - pointC.x) > 1f)
				Handles.DrawBezier (pointA, pointC, pointA, pointC, resultColor, null, 2f);
			Handles.DrawBezier (pointC, pointB, tan0, tan1, resultColor, null, 2f);
			rectEdit.position = Vector2.Lerp (pointC, pointB, 0.5f) - 0.5f * settingSize;
		#endregion

		} else {

		#region 垂直分支
			if (pointA.y < pointB.y) {
				pointA.y = nodeA.canvasRect.yMax - 4;
				pointB.y = nodeB.canvasRect.yMin + 4;
				pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 80f, pointA.y, 99999));
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-5, -12), arrowSize), tex_arrows [0]);
			} else {
				pointA.y = nodeA.canvasRect.yMin + 4;
				pointB.y = nodeB.canvasRect.yMax - 4;
				if (nodeB.isEnd)
					pointB.y += 14;
				pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 80f, -99999, pointA.y));
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-5, 2), arrowSize), tex_arrows [1]);
				resultColor = arrowColors;
			}
			Vector2 tan0 = new Vector2 (pointC.x, pointB.y);
			Vector2 tan1 = new Vector2 (pointB.x, pointC.y);
			if (Mathf.Abs (pointA.y - pointC.y) > 1f)
				Handles.DrawBezier (pointA, pointC, pointA, pointC, resultColor, null, 2f);
			Handles.DrawBezier (pointC, pointB, tan0, tan1, resultColor, null, 2f);
		#endregion

		} 
		rectEdit.position = Vector2.Lerp (pointC, pointB, 0.5f) - 0.5f * settingSize;
		GUI.DrawTexture (rectEdit, tex_setting);
	}

	public void DeleteSelf(){
		nodeA.DeleteLink (false, this);
		nodeB.DeleteLink (true, this);
		GUI.changed = true;
	}
}

public class LeftPanel{
	public List<Character> lst_chars = new List<Character> ();
	GUISkin mySkin;
	GUIStyle titleStyle, labelStyle;
	Rect rect_left, rect_titleL, rect_add;
	Texture2D tex_left, tex_add; 

	int hitChar = -1;

	public LeftPanel(GUISkin _skin){
		mySkin = _skin;
		titleStyle = mySkin.GetStyle ("panelTitle");
		labelStyle = mySkin.GetStyle ("label");

		rect_left = new Rect (0, 0, 120, 90);
		rect_titleL = new Rect (10, 10, 100, 30);
		rect_add = new Rect (10, 69, 75, 15);

		tex_add = Resources.Load<Texture2D> ("GUISkin/AddIcon");
		tex_left = new Texture2D (1, 1);
		tex_left.SetPixel (0, 0, new Color (0, 0, 0, 0.2f));
		tex_left.Apply ();
	}

	//process under mouse down event
	public bool HitTest(Event e){
		if (!rect_left.Contains (e.mousePosition)) {
			foreach (Character c in lst_chars)
				c.Chosen (false);
			GUI.changed = true;
			return false;
		}
		
		if (e.button == 0) {
			if (rect_add.Contains (e.mousePosition)) {
				if (hitChar != -1) {
					lst_chars [hitChar].Chosen (false);
					hitChar = -1;
				}

				lst_chars.Add (new Character ("Insert Name", new Rect (6, 37 + lst_chars.Count * 24, 110, 20), mySkin));
				GUI.changed = true;
			} else {
				hitChar = -1;
				for (int i = 0; i < lst_chars.Count; i++) {
					if(hitChar != -1)
						lst_chars [i].Chosen (false);
					else if (lst_chars [i].HitTest (e.mousePosition))
						hitChar = i;
					else
						lst_chars [i].Chosen (false);
				}

				GUI.changed = true;
			}
		}
		return true;
	}

	public void DrawSelf(){
		rect_left.height = 68 + 24 * lst_chars.Count;
		GUI.DrawTexture (rect_left, tex_left);
		GUI.Label (rect_titleL, "Character List", titleStyle);

		foreach (Character c in lst_chars)
			c.DrawSelf ();
		
		rect_add = new Rect (10, 45 + lst_chars.Count * 24, 75, 15);
		GUI.DrawTexture (new Rect (10, 45 + lst_chars.Count * 24, 12, 12), tex_add);
		GUI.Label (new Rect (28, 40 + lst_chars.Count * 24, 65, 20), "Add New", labelStyle);
	}
}

public class Character{
	public string name;
	public Color color;
	public Texture2D tex;
	GUIStyle nameStyle, fieldStyle;
	Rect rect, rect_color, rect_name, rect_nameField;
	bool chosen = false;
	bool editColor = false;
	bool editName = false;
	Texture2D tex_choose;

	public Character(string _name, Rect _rect, GUISkin _skin){
		name = _name;
		RefreshRect (_rect);
		nameStyle = _skin.GetStyle ("charName");
		fieldStyle = _skin.GetStyle ("textField");

		color = new Color (1, 0.92f, 0, 1);
		tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, color);
		tex.Apply ();

		tex_choose = new Texture2D (1, 1);
		tex_choose.SetPixel (0, 0, new Color (.35f, .35f, .35f, .9f));
		tex_choose.Apply ();
	}

	public bool HitTest(Vector2 mousePos){
		if (rect.Contains (mousePos)) {
			if (chosen) {
				if (rect_color.Contains (mousePos))
					editColor = true;
				else
					editColor = false;
				if (rect_name.Contains (mousePos))
					editName = true;
				else
					editName = false;
			} else {
				Chosen (true);
			}
			return true;
		} else {
			Chosen (false);
			return false;
		}
	}

	public void RefreshRect(Rect _rect){
		rect = _rect;
		rect_color = new Rect (rect.position.x + 4, rect.position.y + 7, 15, 6);
		rect_name = new Rect (rect.position.x + 26, rect.position.y + 3, 85, 20);
		rect_nameField = new Rect (rect_name.position + new Vector2 (-3, -2), rect_name.size + new Vector2(0, -2));
	}

	public void DrawSelf(){
		if (chosen)
			GUI.DrawTexture (rect, tex_choose);
		GUI.DrawTexture (rect_color, tex);
		if (editName) {
			GUI.skin.settings.cursorColor = Color.white;
			name = GUI.TextField (rect_nameField, name, fieldStyle);
			if (Event.current.keyCode == KeyCode.Return) {
				editName = false;
				GUI.changed = true;
			}

		}
		else
			GUI.Label (rect_name, name, nameStyle);
		//if (editColor)
			//color = EditorGUI.ColorField (new Rect (rect_color.position, new Vector2 (100, 20)), color);
	}

	public void Chosen(bool isChosen){
		if (chosen = isChosen)
			return;
		
		chosen = isChosen;
		if (chosen) {
		
		} else {
			editColor = false;
			editName = false;
		}
	}
}

public class PopUpWindow{
	Rect windowRect;

	public PopUpWindow(Rect _rect){
		windowRect = _rect;
		NodeCreator.NCGod.SetPopUp (this);
	}

	public virtual void DrawSelf(){
	
	}

	public virtual void HitTest(){
		
	}
}

