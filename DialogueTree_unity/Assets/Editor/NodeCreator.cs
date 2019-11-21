using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeCreator : EditorWindow {
	public static NodeCreator NCGod;
	List<Node> lst_Node = new List<Node> ();
	Node SelectNode = null;
	Vector2 coordinate;

	GUISkin mySkin;
	GUIStyle nameStyle;
	Texture2D tex_bg, tex_left, tex_add;

	enum DropdownType{close, normal, select};
	DropdownType dType = DropdownType.close;

	PopUpWindow nowPopUp = null;
	LeftPanel leftPanel;
	bool linking = false;
	int downButton = -1;

	[MenuItem("Window/Node Creator")]
	static void Init(){
		NodeCreator window = (NodeCreator)GetWindow (typeof(NodeCreator));
		window.minSize = new Vector2 (400, 250);
		window.titleContent = new GUIContent ("Node Creator");
		window.Show ();

	}

	void OnEnable(){
		NCGod = this;
		mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		tex_bg = Resources.Load<Texture2D> ("GUISkin/Grid4");
		coordinate = Vector2.zero;
		CreateNode (Vector2.zero);
		leftPanel = new LeftPanel (mySkin);

	}

	void OnGUI(){
		DrawBackground ();

		ProcessEvent (Event.current);

		DrawConnect ();

		DrawNodes ();

		leftPanel.DrawSelf ();

		if (GUI.changed)
			Repaint ();
		
	}

	public void SetPopUp(PopUpWindow _pu){
		nowPopUp = _pu;
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

	void DrawConnect(){
		foreach (Node n in lst_Node)
			n.DrawMyLink ();
	}
	void DrawNodes(){
		if (lst_Node.Count > 0) {
			foreach (Node _n in lst_Node) {
				if(_n != SelectNode)
					_n.DrawSelf (coordinate);
			}

			if (SelectNode != null)
				SelectNode.DrawSelf (coordinate);
		}
	}


	void ProcessEvent(Event e){
		DropdownMenu (e);
		dType = DropdownType.close;

		if (e.type == EventType.MouseUp) {
			downButton = -1;
			return;
		}
			
		if (linking) {
			Vector2 mousePos = e.mousePosition;
			Link (SelectNode.canvasRect.center, mousePos);
			CanvasAutoMove (mousePos);

			GUI.changed = true;
			if (e.type == EventType.MouseDown && e.button == 0) {
				Node originNode = SelectNode;
				if (ClickCheck (mousePos))
					originNode.SetConnect (SelectNode);

				downButton = -1;
				linking = false;
				ResetSelect ();
			}
		} else {
			switch(e.type){
			case EventType.MouseDown:
				if (!leftPanel.HitTest (e)) {
					downButton = e.button;
					if (downButton == 0) {
						ClickCheck (e.mousePosition);
					} else if (downButton == 1) {
						if (ClickCheck (e.mousePosition))
							dType = DropdownType.select;
						else
							dType = DropdownType.normal;
					}
				}

				break;


			case EventType.MouseDrag:
				if (downButton == 2 || (downButton != -1 && SelectNode == null)) {
					coordinate += e.delta;
					GUI.changed = true;
				}else if(SelectNode != null){
					SelectNode.FollowMouse (e.delta);
				}
				break;
			case EventType.KeyDown:
				if (e.keyCode == KeyCode.Space) {
					coordinate = -lst_Node [0].rect.position + 0.5f * position.size - new Vector2 (80, 80);
					GUI.changed = true;
				}
				break;
			}
		}


	}

	void Link(Vector2 pointA, Vector2 pointB){
		Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);
	}

	void DropdownMenu(Event e){
		if (dType == DropdownType.close)
			return;
		
		GenericMenu menu = new GenericMenu ();
		if (dType == DropdownType.normal) {
			menu.AddItem (new GUIContent ("Create New"), false, () => CreateNode (e.mousePosition));
			menu.AddSeparator ("");
			menu.AddDisabledItem (new GUIContent ("Create Child"));
			menu.AddDisabledItem (new GUIContent ("Make Connection"));
			menu.AddDisabledItem (new GUIContent ("Edit"));
			menu.AddDisabledItem (new GUIContent ("Delete"));
		} else {
			menu.AddItem (new GUIContent ("Create New"), false, () => CreateNode (e.mousePosition));
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Create Child"), false, () => CreateChild ());
			menu.AddItem (new GUIContent ("Make Connection"), false, () => MakeConnect (e.mousePosition));
			menu.AddDisabledItem (new GUIContent ("Edit"));
			if (SelectNode.GetType () != typeof(StartNode))
				menu.AddItem (new GUIContent ("Delete"), false, () => DeleteNode (SelectNode));
			else
				menu.AddDisabledItem (new GUIContent ("Delete"));
		}
		menu.ShowAsContext ();
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

	bool ClickCheck(Vector2 mousePos){
		Node ClickedNode = null;
		for (int i = lst_Node.Count-1; i >=0; i--) {
			ClickedNode = lst_Node [i].HitTest (mousePos);

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

	void ResetSelect(){
		if (SelectNode != null) {
			SelectNode.Selected (false);
			SelectNode = null;
		}
  	}

	void MakeConnect(Vector2 mousePos){
		linking = true;
  	}

	void CreateChild(){
		Node n;
		n = new Node (SelectNode.rect.position + new Vector2 (0, 100), "Dialog" + lst_Node.Count.ToString (), mySkin);
		SelectNode.SetConnect (n);
		lst_Node.Add (n);
	}

	void CreateNode(Vector2 mousePos){
		Node n;
		if(lst_Node.Count == 0)
			n = new StartNode (new Vector2 (120, 150), "", null);
		else
			n = new Node (mousePos - coordinate, "Dialog" + lst_Node.Count.ToString (), mySkin);
		lst_Node.Add (n);
	}

	void DeleteNode(Node _n){
		_n.DeleteAllConnect ();
		lst_Node.Remove (_n);
		SelectNode = null;
		GUI.changed = true;
	}
}
