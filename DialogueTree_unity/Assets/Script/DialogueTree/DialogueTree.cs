using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueTree : EditorWindow {
	public enum WindowState{normal, drag, popup, link, scroll};
	public WindowState nowState = WindowState.normal;

	public LeftPanel leftPanel;
	public RightPanel rightPanel;
	ColorWindow colorWindow;

	public List<Character> lst_chars = new List<Character> ();
	public List<Node> lst_node = new List<Node> ();
	public Node SelectNode = null;
	public int plotNodeCount = 0;
	Texture2D tex_bg, tex_left, tex_add;
	GUIStyle style_button;
	Vector2 coordinate;

	[MenuItem("Window/Dialogue Tree")]
	static void Init(){
		DialogueTree window = (DialogueTree)GetWindow (typeof(DialogueTree));
		window.minSize = new Vector2 (400, 250);
		window.titleContent = new GUIContent ("Dialogue Tree");
		window.Show ();

	}

	void OnEnable(){
		tex_bg = Resources.Load<Texture2D> ("GUISkin/Grid");
		lst_chars.Add (new Character (this, "N/A", 7));
		coordinate = Vector2.zero;
		colorWindow = new ColorWindow ();
		leftPanel = new LeftPanel (this);
		rightPanel = new RightPanel (this);
		GUISkin mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_button = mySkin.GetStyle ("button");

		CreateNode (Vector2.zero, 0);
		//Selection.selectionChanged = LoadStoryAsset;
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
		foreach (Node n in lst_node)
			n.DrawMyLink ();
		foreach (Node _n in lst_node) {
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
				if (ClickNode (mousePos)) {
					if(SelectNode.GetType () != typeof(SubNode) && SelectNode.GetType () != typeof(StartNode))
						originNode.SetConnect (SelectNode);
				}
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
		if (lst_node.Count == 0)
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
		if (n != null) {
			lst_node.Add (n);
			rightPanel.SetQNodeList ();
		}
	}

	void DeleteNode(Node _n){
		_n.DeleteAllConnect ();
		if (_n.GetType () == typeof(StartNode))
			plotNodeCount--;
		if (_n.GetType () == typeof(QuestionNode))
			RemoveQuestion (_n);
		else
			lst_node.Remove (_n);
		rightPanel.SetQNodeList ();
		ResetSelect ();
	}
#endregion

#region Click
	bool ClickNode(Vector2 mousePos){
		Node ClickedNode = null;
		foreach (Node _n in lst_node) {
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
		foreach (Node _n in lst_node) {
			if (_n.HitLinkTest (mousePos))
				return true;
		}
		return false;
	}
#endregion

#region file
	void OpenFile(){
		string path = EditorUtility.OpenFilePanel("開啟劇情檔案", "", "asset");
		if (path == "")
			return;
		path = path.Replace (Application.dataPath, "Assets");
		scriptable_story story = AssetDatabase.LoadAssetAtPath<scriptable_story> (path);
		CharInfoCast (false, ref story.lst_charInfo);
		NodeInfoCast (false, ref story);
		rightPanel.SetNameArray ();
		rightPanel.SetQNodeList ();
	}

	void SaveData(){
		string path = EditorUtility.SaveFilePanelInProject ("儲存劇情檔案", "new story", "asset", "");
		if (path == "")
			return;
		scriptable_story story = ScriptableObject.CreateInstance<scriptable_story> ();
		CharInfoCast (true, ref story.lst_charInfo);
		NodeInfoCast (true, ref story);
		AssetDatabase.CreateAsset (story, path);
		EditorUtility.SetDirty (story);
	}

	void CharInfoCast(bool char2Info, ref List<CharInfo> charInfo){
		if (char2Info) {
			//Cast Character to CharInfo, used for saving.
			charInfo = new List<CharInfo> ();
			foreach (Character c in lst_chars)
				charInfo.Add (new CharInfo (c));
		} else {
			//Cast CharInfo to Character, used for opening.
			lst_chars = new List<Character> ();
			foreach (CharInfo info in charInfo)
				lst_chars.Add (new Character (this, info.name, info.colorIndex));
		}
	}

	void NodeInfoCast(bool node2Info, ref scriptable_story story){
		if (node2Info) {
			//Cast Node to NodeInfo, used for saving.
			story.lst_startNodeInfo = new List<StartNodeInfo> ();
			story.lst_dialogueNodeInfo = new List<DialogueNodeInfo> ();
			story.lst_questionNodeInfo = new List<QuestionNodeInfo> ();
			story.lst_divergeNodeInfo = new List<DivergeNodeInfo> ();

			foreach (Node n in lst_node) {
				switch (n.GetType ().ToString()) {
				case "StartNode":
					story.lst_startNodeInfo.Add (new StartNodeInfo (n));
					break;
				case "DialogueNode":
					story.lst_dialogueNodeInfo.Add (new DialogueNodeInfo (n));
					break;
				case "QuestionNode":
					story.lst_questionNodeInfo.Add (new QuestionNodeInfo (n));
					break;
				case "DivergeNode":
					story.lst_divergeNodeInfo.Add (new DivergeNodeInfo (n));
					break;
				default:
					break;
				}
			}
		} else {
			//Cast NodeInfo to Node, used for opening.

			//Create Nodes from nodeInfo.
			lst_node = new List<Node> ();
			foreach (StartNodeInfo info in story.lst_startNodeInfo)
				lst_node.Add (info.Cast2Node (this));
			plotNodeCount = lst_node.Count;
			foreach (DialogueNodeInfo info in story.lst_dialogueNodeInfo)
				lst_node.Add (info.Cast2Node (this));
			foreach (QuestionNodeInfo info in story.lst_questionNodeInfo)
				lst_node.Add (info.Cast2Node (this));
			foreach (DivergeNodeInfo info in story.lst_divergeNodeInfo)
				lst_node.Add (info.Cast2Node (this));

			//After all nodes are created, reconnect all links.
			foreach (StartNodeInfo info in story.lst_startNodeInfo)
				info.Reconnect (this);
			foreach (DialogueNodeInfo info in story.lst_dialogueNodeInfo)
				info.Reconnect (this);
			foreach (QuestionNodeInfo info in story.lst_questionNodeInfo)
				info.Reconnect (this);
			foreach (DivergeNodeInfo info in story.lst_divergeNodeInfo)
				info.Reconnect (this);
				
		}
	}

	public Node GetNodeByName(string _name){
		return lst_node.Find (n => n.nodeName == _name);
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

	public void RemoveChar(Character c){
		foreach (Node n in lst_node) {
			switch (n.GetType ().ToString ()) {
			case "DialogueNode":
				DialogueNode dn = (DialogueNode)n;
				if (dn.myCharacter == c)
					dn.myCharacter = lst_chars [0];
				break;
			case "QuestionNode":
				QuestionNode qn = (QuestionNode)n;
				if (qn.myCharacter == c)
					qn.myCharacter = lst_chars [0];
				break;
			default:
				break;
			}
		}
		lst_chars.Remove (c);
	}

	public void RemoveQuestion(Node qn){
		foreach (Node n in lst_node) {
			if (n.GetType () == typeof(DivergeNode)) {
				DivergeNode dn = (DivergeNode)n;
				foreach (SubNode diver in dn.diverges) {
					foreach (ConditionUnit c in diver.myDiverge) {
						if (c.myQuestion == qn)
							c.myQuestion = null;
					}
				}
			}
		}
		lst_node.Remove (qn);
	}
#endregion
}
