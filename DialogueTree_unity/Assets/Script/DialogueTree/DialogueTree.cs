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

	//public scriptable_story story = null;
	public Node SelectNode = null;
	GUIStyle style_button, style_title;
	Texture2D tex_bg;
	Rect rect_hint = new Rect (0, 0, 150, 100), 
	rect_label = new Rect(20, 15, 110, 30), 
	rect_button = new Rect(10, 50, 130, 30);
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
		colorWindow = new ColorWindow ();
		leftPanel = new LeftPanel (this);
		rightPanel = new RightPanel (this);

		GUISkin mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		style_title = mySkin.GetStyle ("midtitle");
		style_button = mySkin.GetStyle ("button");

		Selection.selectionChanged = LoadStoryAsset;
		LoadStoryAsset ();
	}

	void OnGUI(){
		DrawBackground ();

		DrawStory ();

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

	void DrawStory(){
		/*
		if (story == null) {
			DrawDefault ();
			return;
		}*/

		ProcessEvent (Event.current);

		DrawNodes ();

		DrawPanels ();

		//EditorUtility.SetDirty (story);
	}

	void DrawDefault(){
		Vector2 midPos = 0.5f * position.size - 0.5f * rect_hint.size;
		midPos.x = Mathf.Round (midPos.x);
		midPos.y = Mathf.Round (midPos.y);
		rect_hint.position = midPos;
		GUI.BeginGroup (rect_hint);
		GUI.Label (rect_label, "點選一劇情檔，或", style_title);
		if (GUI.Button (rect_button, "創建新劇情檔", style_button))
			CreateStoryAsset ();
		GUI.EndGroup ();
	}

	void DrawNodes(){
		foreach (Node n in story.lst_Node)
			n.DrawMyLink ();
		foreach (Node _n in story.lst_Node) {
			if(_n != SelectNode)
				_n.DrawSelf (coordinate);
		}

		if (SelectNode != null)
			SelectNode.DrawSelf (coordinate);
	}

	void DrawPanels(){
		leftPanel.DrawSelf ();
		rightPanel.DrawSelf (position.size);
		if(nowState == WindowState.popup)
			colorWindow.DrawSelf ();
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
			menu.AddItem (new GUIContent ("刪除"), false, () => DeleteNode (SelectNode));
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
		switch (type) {
		case 0:
			n = new StartNode (this, mousePos - coordinate);
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
		if(n != null)
			story.lst_Node.Add (n);
	}

	void DeleteNode(Node _n){
		_n.DeleteAllConnect ();
		story.lst_Node.Remove (_n);
		ResetSelect ();
	}
#endregion

#region Click
	bool ClickNode(Vector2 mousePos){
		Node ClickedNode = null;
		foreach (Node _n in story.lst_Node) {
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
		foreach (Node _n in story.lst_Node) {
			if (_n.HitLinkTest (mousePos))
				return true;
		}
		return false;
	}
          #endregion

#region StoryAsset
	public void LoadStoryAsset(){
		if (Selection.activeObject == null || Selection.activeObject.GetType () != typeof(scriptable_story)) {
			story = null;
			return;
		}
		story = (scriptable_story)Selection.activeObject;
		coordinate = Vector2.zero;
	}

	void CreateStoryAsset(){
		string path = EditorUtility.SaveFilePanelInProject ("創建新劇情檔", "new story", "asset", "選擇儲存位置");
		if (path == "")
			return;
		story = ScriptableObject.CreateInstance<scriptable_story> ();
		story.lst_chars.Add (new Character (this));
		story.lst_Node.Add (new StartNode (this, new Vector2 (120, 150)));
		AssetDatabase.CreateAsset (story, path);
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
		Selection.activeObject = AssetDatabase.LoadAssetAtPath<scriptable_story> (path);
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

	void PopupEvent(Vector2 mousePos){
		if (!colorWindow.HitTest (mousePos)) {
			nowState = WindowState.normal;
		}
	}
#endregion
}
