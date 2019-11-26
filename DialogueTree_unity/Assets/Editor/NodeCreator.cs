using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeCreator : EditorWindow {
	GUISkin mySkin;
	GUIStyle nameStyle;
	Texture2D tex_bg, tex_left, tex_add;

	enum WindowState{normal, drag, popup, link};
	//enum DropdownType{close, normal, select};
	WindowState nowState = WindowState.normal;
	//DropdownType dType = DropdownType.close;

	public enum ClickType{node, leftPanel, rightPanel, popup}
	LeftPanel leftPanel;
	RightPanel rightPanel;

	List<Node> lst_Node = new List<Node> ();
	Node SelectNode = null;
	Vector2 coordinate;
	//bool linking = false;
                       	//int downButton = -1;

	[MenuItem("Window/Node Creator")]
	static void Init(){
		NodeCreator window = (NodeCreator)GetWindow (typeof(NodeCreator));
		window.minSize = new Vector2 (400, 250);
		window.titleContent = new GUIContent ("Node Creator");
		window.Show ();

	}

	void OnEnable(){
		mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		tex_bg = Resources.Load<Texture2D> ("GUISkin/Grid4");
		coordinate = Vector2.zero;
		CreateNode (Vector2.zero, 0);
		leftPanel = new LeftPanel (mySkin);
		rightPanel = new RightPanel ();
	}

	void OnGUI(){
		DrawBackground ();

		ProcessEvent (Event.current);

		DrawNodes ();

		leftPanel.DrawSelf ();
		rightPanel.DrawSelf (position.size);

		if (GUI.changed)
			Repaint ();
		
	}

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
				if (SelectNode == null) {
					coordinate += e.delta;
					GUI.changed = true;
				} else
					SelectNode.FollowMouse (e.delta);
			}else if (e.type == EventType.MouseUp)
				nowState = WindowState.normal;
			break;
		case WindowState.popup:
			break;
		case WindowState.link:
			Link (SelectNode.canvasRect.center, mousePos);
			CanvasAutoMove (mousePos);
			GUI.changed = true;
			if (e.type == EventType.MouseDown && e.button == 0) {
				Node originNode = SelectNode;
				if (ClickNode (mousePos))
					originNode.SetConnect (SelectNode);
				nowState = WindowState.normal;
				ResetSelect ();
			}
			break;
		}
	}

	void Link(Vector2 pointA, Vector2 pointB){
		Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);
	}

#region Dropdowns
	void MainDropdown(Vector2 mousePos){
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Create Dialogue"), false, () => CreateNode (mousePos, 0));
		menu.AddItem (new GUIContent ("Create Question"), false, () => CreateNode (mousePos, 1));
		menu.AddItem (new GUIContent ("Create Diverge"), false, () => CreateNode (mousePos, 2));
		menu.ShowAsContext ();
	}

	void NodeDropdown(){
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Make Connection"), false, () => {nowState = WindowState.link;});
		if (SelectNode.GetType () != typeof(StartNode))
			menu.AddItem (new GUIContent ("Delete"), false, () => DeleteNode (SelectNode));
		else
			menu.AddDisabledItem (new GUIContent ("Delete"));
		menu.ShowAsContext ();
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

	void CreateNode(Vector2 mousePos, int type){
		Node n = null;
		if (lst_Node.Count == 0)
			n = new StartNode (new Vector2 (120, 150), "", null);
		else {
			switch (type) {
			case 0:
				n = new DialogueNode (mousePos - coordinate, "Dialog" + lst_Node.Count.ToString (), mySkin);
				break;
			case 1:
				n = new QuestionNode (mousePos - coordinate, "Dialog" + lst_Node.Count.ToString (), mySkin);
				break;
			case 2:
				n = new DivergeNode (mousePos - coordinate, "Dialog" + lst_Node.Count.ToString (), mySkin);
				break;
			}
		}
		if(n != null)
			lst_Node.Add (n);
	}

	void DeleteNode(Node _n){
		_n.DeleteAllConnect ();
		lst_Node.Remove (_n);
		SelectNode = null;
		GUI.changed = true;
	}
}
