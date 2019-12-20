using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueTree : EditorWindow {
	Texture2D tex_bg, tex_left, tex_add;
	GUIStyle style_button;
	public enum WindowState{normal, drag, popup, link, scroll};
	public WindowState nowState = WindowState.normal;

	public enum ClickType{node, leftPanel, rightPanel, popup}
	public LeftPanel leftPanel;
	public RightPanel rightPanel;
	ColorWindow colorWindow;

	//public scriptable_story story;
	public List<Character> lst_chars = new List<Character> ();
	public List<Node> lst_Node = new List<Node> ();
	public Node SelectNode = null;
	Vector2 coordinate;
	int plotNodeCount = 1;



	[MenuItem("Window/Dialogue Tree")]
	static void Init(){
		DialogueTree window = (DialogueTree)GetWindow (typeof(DialogueTree));
		window.minSize = new Vector2 (400, 250);
		window.titleContent = new GUIContent ("Dialogue Tree");
		window.Show ();

	}

	void OnEnable(){
		tex_bg = Resources.Load<Texture2D> ("GUISkin/Grid");
		//lst_chars.Add (new Character (this));
		coordinate = Vector2.zero;
		CreateNode (Vector2.zero, 0);
		colorWindow = new ColorWindow ();
		leftPanel = new LeftPanel (this);
		rightPanel = new RightPanel (this);

		GUISkin mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_button = mySkin.GetStyle ("button");

		Selection.selectionChanged = LoadStoryAsset;
	}

	void OnGUI(){
		DrawBackground ();

		ProcessEvent (Event.current);

		DrawNodes ();

		DrawPanels ();

		Repaint ();
	}

#region MainFunctions
	void DrawBackground(){
		int i = -1;
		float xOffSet = coordinate.x % 120;
		float yOffset = coordinate.y % 120;
		while ((i-1) * 120 < position.width) {
			int j = -1;
			while ((j-1) * 120 < position.height) {
				Rect bgRect = new Rect (i * 120 + xOffSet, j * 120 + yOffset, 120, 120);
				GUI.DrawTexture (bgRect, tex_bg);
				j++;
			}
			i++;
		}
	}

	void DrawNodes(){
		foreach (Node n in lst_Node)
			n.DrawMyLink ();
		foreach (Node _n in lst_Node) {
			if(_n != SelectNode)
				_n.DrawSelf (coordinate);
		}

		if (SelectNode != null)
			SelectNode.DrawSelf (coordinate);
	}

	void DrawPanels(){
		leftPanel.DrawSelf ();
		rightPanel.DrawSelf (position.size);
		BottomPanel ();
		if(nowState == WindowState.popup)
			colorWindow.DrawSelf ();
	}

	void BottomPanel(){
		if (GUI.Button (new Rect (5, position.height - 30, 70, 25), "開啟", style_button)) {
			ResetSelect ();
			OpenFile ();
		}
			
		if (GUI.Button (new Rect (80, position.height - 30, 70, 25), "儲存", style_button)) {
			ResetSelect ();
			SaveData ();
		}
	}

	void ProcessEvent(Event e){
		Vector2 mousePos = e.mousePosition;
		switch (nowState) {
		case WindowState.normal:
			if (e.type == EventType.MouseDown) {
				if (!leftPanel.HitTest (e) && !rightPanel.HitTest(mousePos)) {
					switch (e.button) {
					case 0:
						if (!ClickNode (mousePos))
							ClickLink (mousePos);
						nowState = WindowState.drag;
						break;
					case 1:
						if (ClickNode (mousePos))
							NodeDropdown ();
						else if(!ClickLink(mousePos))
							MainDropdown (mousePos);
						break;
					default:
						nowState = WindowState.drag;
						break;
					}

				}
			}
			break;
		case WindowState.drag:
			if (e.type == EventType.MouseDrag) {
				if (SelectNode == null)
					coordinate += e.delta;
				else
					SelectNode.FollowMouse (e.delta);
			}else if (e.type == EventType.MouseUp)
				nowState = WindowState.normal;
			break;
		case WindowState.popup:
			if (e.type == EventType.MouseDown && e.button == 0)
				PopupEvent (e.mousePosition);
			break;
		case WindowState.link:
			Link (SelectNode.canvasRect.center, mousePos);
			CanvasAutoMove (mousePos);
			if (e.type == EventType.MouseDown && e.button == 0) {
				Node originNode = SelectNode;
				if (ClickNode (mousePos))
					originNode.SetConnect (SelectNode);
				nowState = WindowState.normal;
				ResetSelect ();
			}
			break;
		case WindowState.scroll:
			if (e.type == EventType.MouseDrag)
				rightPanel.Scroll (e.delta.y);
			else if (e.type == EventType.MouseUp)
				nowState = WindowState.normal;
			break;
		}
		if (nowState != WindowState.scroll && e.type == EventType.ScrollWheel)
			rightPanel.Scroll (e);
	}

	void Link(Vector2 pointA, Vector2 pointB){
		Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);
	}
#endregion

#region Dropdowns
	void MainDropdown(Vector2 mousePos){
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("新增劇情"), false, () => CreateNode (mousePos, 0));
		menu.AddSeparator ("");
		menu.AddItem (new GUIContent ("新增對話集"), false, () => CreateNode (mousePos, 1));
		menu.AddItem (new GUIContent ("新增問答集"), false, () => CreateNode (mousePos, 2));
		menu.AddItem (new GUIContent ("新增分歧點"), false, () => CreateNode (mousePos, 3));
		menu.ShowAsContext ();
	}

	void NodeDropdown(){
		GenericMenu menu = new GenericMenu ();
		switch (SelectNode.GetType ().ToString ()) {
		case "StartNode":
			menu.AddItem (new GUIContent ("建立連結"), false, () => {nowState = WindowState.link;});
			if(plotNodeCount > 1)
				menu.AddItem (new GUIContent ("刪除"), false, () => DeleteNode (SelectNode));
			else
				menu.AddDisabledItem (new GUIContent ("刪除"));
			break;
		case "DialogueNode":
			menu.AddItem (new GUIContent ("建立連結"), false, () => {nowState = WindowState.link;});
			menu.AddItem (new GUIContent ("刪除"), false, () => DeleteNode (SelectNode));
			break;
		case "QuestionNode":
			menu.AddDisabledItem (new GUIContent ("建立連結"));
			menu.AddItem (new GUIContent ("刪除"), false, () => DeleteNode (SelectNode));
			break;
		case "DivergeNode":
			menu.AddDisabledItem (new GUIContent ("建立連結"));
			menu.AddItem (new GUIContent ("刪除"), false, () => DeleteNode (SelectNode));
			break;
		case "SubNode":
			menu.AddItem (new GUIContent ("建立連結"), false, () => {nowState = WindowState.link;});
			menu.AddDisabledItem (new GUIContent ("刪除"));
			break;
		}
		menu.ShowAsContext ();
	}

	void CreateNode(Vector2 mousePos, int type){
		Node n = null;
		if (lst_Node.Count == 0)
			n = new StartNode (this, new Vector2 (120, 150));
		else {
			switch (type) {
			case 0:
				n = new StartNode (this, mousePos - coordinate);
				plotNodeCount++;
				break;
			case 1:
				n = new DialogueNode (this, mousePos - coordinate);
				break;
			case 2:
				n = new QuestionNode (this, mousePos - coordinate);
				break;
			case 3:
				n = new DivergeNode (this, mousePos - coordinate);
				break;
			}
		}
		if(n != null)
			lst_Node.Add (n);
	}

	void DeleteNode(Node _n){
		_n.DeleteAllConnect ();
		if (_n.GetType () == typeof(StartNode))
			plotNodeCount--;
		lst_Node.Remove (_n);
		ResetSelect ();
	}
#endregion

#region Click
	bool ClickNode(Vector2 mousePos){
		Node ClickedNode = null;
		foreach (Node _n in lst_Node) {
			ClickedNode = _n.HitTest (mousePos);

			//Clicked node
			if (ClickedNode != null) {
				if (ClickedNode != SelectNode) {
					ResetSelect ();
					SelectNode = ClickedNode;
				}
				return true;
			}
		}
		//clicked nothing
		ResetSelect ();
		return false;

	}

	bool ClickLink(Vector2 mousePos){
		foreach (Node _n in lst_Node) {
			if (_n.HitLinkTest (mousePos))
				return true;
		}
		return false;
	}
#endregion

#region file
	public void LoadStoryAsset(){
		if (Selection.activeObject == null || Selection.activeObject.GetType () != typeof(scriptable_story))
			return;
		scriptable_story story = (scriptable_story)Selection.activeObject;
	}

	void OpenFile(){
		/*
		string path = EditorUtility.OpenFilePanel("開啟劇情檔案", "", "asset");
		if (path == "") {
			Debug.Log ("Abort Opening.");
			return;
		}
		path = path.Replace (Application.dataPath, "Assets");
		scriptable_story story = AssetDatabase.LoadAssetAtPath<scriptable_story> (path);
		lst_chars = story.lst_chars;
		lst_Node = story.lst_Node;*/
	}

	void SaveData(){
		/*
		string path = EditorUtility.SaveFilePanelInProject ("儲存劇情檔案", "new story", "asset", "");
		if (path == "") {
			Debug.Log ("Abort Saving.");
			return;
		}
		scriptable_story story = ScriptableObject.CreateInstance<scriptable_story> ();
		story.lst_chars = new List<Character>(lst_chars);
		story.lst_Node = new List<Node> (lst_Node);
		AssetDatabase.CreateAsset (story, path);*/
	}
#endregion

#region others
	void ResetSelect(){
		if (SelectNode != null) {
			SelectNode.Selected (false);
			SelectNode = null;
		}
  	}

	void CanvasAutoMove(Vector2 mousePos){
		if (mousePos.x > position.width - 50)
			coordinate.x -= 4;
		else if (mousePos.x < 50)
			coordinate.x += 4;

		if (mousePos.y > position.height - 50)
			coordinate.y -= 4;
		else if (mousePos.y < 50)
			coordinate.y += 4;
	}

	public void Popup(Vector2 mousePos, Character _char){
		colorWindow.Popup (mousePos, _char);
		nowState = WindowState.popup;
	}

	public void PopupEvent(Vector2 mousePos){
		if (!colorWindow.HitTest (mousePos)) {
			nowState = WindowState.normal;
		}
	}
#endregion
}
