using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeCreator : EditorWindow {
	List<Node> lst_Node = new List<Node> ();
	List<NodeLink> lst_Link = new List<NodeLink> ();
	Node End = null;
	Node SelectNode = null;
	Vector2 coordinate;

	GUISkin mySkin;
	Texture2D bgTexture;
	Texture2D[] tex_arrows = new Texture2D[4];
	Vector2 arrowSize = new Vector2(10, 10);
	enum DropdownType{close, normal, select};
	DropdownType dType = DropdownType.close;
	bool linking = false;

	[MenuItem("Window/Node Creator")]
	static void Init(){
		NodeCreator window = (NodeCreator)GetWindow (typeof(NodeCreator));
		window.minSize = new Vector2 (400, 250);
		window.titleContent = new GUIContent ("Node Creator");
		window.Show ();

	}

	void OnEnable(){
		mySkin = Resources.Load<GUISkin> ("GUISkin/NodeSkin");
		bgTexture = Resources.Load<Texture2D> ("GUISkin/Grid4");
		tex_arrows [0] = Resources.Load<Texture2D> ("GUISkin/ArrowD");
		tex_arrows [1] = Resources.Load<Texture2D> ("GUISkin/ArrowU");
		tex_arrows [2] = Resources.Load<Texture2D> ("GUISkin/ArrowR");
		tex_arrows [3] = Resources.Load<Texture2D> ("GUISkin/ArrowL");
		coordinate = Vector2.zero;
		CreateStartNode ();
	}

	void OnGUI(){
		DrawBackground ();

		ProcessEvent (Event.current);

		DrawConnect ();

		DrawNodes ();

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
				GUI.DrawTexture (bgRect, bgTexture);
				j++;
			}
			i++;
		}
	}

	void DrawConnect(){
		foreach (Node n in lst_Node) {
			if (n.NextNode.Count > 0) {
				foreach (Node nn in n.NextNode)
					Link (n.canvasRect, nn.canvasRect);
			}
		}
	}

	void Link(Rect rectA, Rect rectB){
		Vector2 pointA = rectA.center;
		Vector2 pointB = rectB.center;
		float dWidth = Mathf.Abs (pointA.x - pointB.x);

		if (Mathf.Abs (pointA.y - pointB.y) < 56) {
			if (dWidth < 158)
				return;
			if (pointA.x < pointB.x) {
				pointA.x = rectA.xMax - 5;
				pointB.x = rectB.xMin + 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-13, -5), arrowSize), tex_arrows [2]);
			} else {
				pointA.x = rectA.xMin + 5;
				pointB.x = rectB.xMax - 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (3, -5), arrowSize), tex_arrows [3]);
			}
			Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);

		} else if (pointA.y < pointB.y) {
			pointA.y = rectA.yMax - 4;
			pointB.y = rectB.yMin + 4;

			if (dWidth < 5)
				Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);
			else {
				Vector2 tan0 = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 20f, pointA.y + 15f, 99999));
				Vector2 tan1 = new Vector2 (pointB.x + 0.06f * dWidth, Mathf.Clamp (pointB.y - 80f, pointA.y - 15f, 99999));
				Vector2 pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 90f, pointA.y, 99999));
				if (pointC.y - pointA.y >= 1f)
					Handles.DrawBezier (pointA, pointC, pointA, pointC, Color.white, null, 2f);
				Handles.DrawBezier (pointC, pointB, tan0, tan1, Color.white, null, 2f);
			}

			GUI.DrawTexture (new Rect (pointB + new Vector2 (-5, -12), arrowSize), tex_arrows [0]);

		} else {
			pointA.y = rectA.yMin + 4;
			pointB.y = rectB.yMax - 4;
			if (dWidth < 5)
				Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);
			else {
				Vector2 tan0 = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 20f, -99999, pointA.y - 15f));
				Vector2 tan1 = new Vector2 (pointB.x + 0.06f * dWidth, Mathf.Clamp (pointB.y + 80f, -99999, pointA.y + 15f));
				Vector2 pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 90f, -99999, pointA.y));
				if (pointA.y - pointC.y >= 1f)
					Handles.DrawBezier (pointA, pointC, pointA, pointC, Color.white, null, 2f);
				Handles.DrawBezier (pointC, pointB, tan0, tan1, Color.white, null, 2f);
			}

			GUI.DrawTexture (new Rect (pointB + new Vector2 (-5, 2), arrowSize), tex_arrows [1]);
		}

	}

	void Link(Rect rectA, Vector2 pointB){
		Vector2 pointA = rectA.center;
		float dWidth = Mathf.Abs (pointA.x - pointB.x);

		if (Mathf.Abs (pointA.y - pointB.y) < 34) {
			if (dWidth < 80)
				return;
			if (pointA.x < pointB.x) {
				pointA.x = rectA.xMax - 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-13, -5), arrowSize), tex_arrows [2]);
			} else {
				pointA.x = rectA.xMin + 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (3, -5), arrowSize), tex_arrows [3]);
			}

			Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);

		} else if (pointA.y < pointB.y) {
			pointA.y = rectA.yMax - 4;
			Vector2 tan0 = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 20f, pointA.y + 15f, 99999));
			Vector2 tan1 = new Vector2 (pointB.x + 0.06f * dWidth, Mathf.Clamp (pointB.y - 80f, pointA.y - 15f, 99999));
			Vector2 pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 90f, pointA.y, 99999));

			if (pointC.y - pointA.y >= 1f)
				Handles.DrawBezier (pointA, pointC, pointA, pointC, Color.white, null, 2f);
			Handles.DrawBezier (pointC, pointB, tan0, tan1, Color.white, null, 2f);
			GUI.DrawTexture (new Rect (pointB + new Vector2 (-5, -12), arrowSize), tex_arrows [0]);
		} else {
			pointA.y = rectA.yMin + 4;
			Vector2 tan0 = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 20f, -99999, pointA.y - 15f));
			Vector2 tan1 = new Vector2 (pointB.x + 0.06f * dWidth, Mathf.Clamp (pointB.y + 80f, -99999, pointA.y + 15f));
			Vector2 pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 90f, -99999, pointA.y));

			if (pointA.y - pointC.y >= 1f)
				Handles.DrawBezier (pointA, pointC, pointA, pointC, Color.white, null, 2f);
			Handles.DrawBezier (pointC, pointB, tan0, tan1, Color.white, null, 2f);
			GUI.DrawTexture (new Rect (pointB + new Vector2 (-5, 2), arrowSize), tex_arrows [1]);
		}
	}

	void DrawNodes(){
		if (lst_Node.Count > 0) {
			foreach (Node _n in lst_Node) {
				if(_n != SelectNode)
					_n.DrawSelf (coordinate);
			}

			if (End != null)
				End.DrawSelf (coordinate);

			if (SelectNode != null)
				SelectNode.DrawSelf (coordinate);
		}
	}

	void ProcessEvent(Event e){
		DropdownMenu (e);
		dType = DropdownType.close;

		if (linking) {
			Vector2 mousePos = e.mousePosition;
			Link (SelectNode.canvasRect, mousePos);
			CanvasAutoMove (mousePos);

			GUI.changed = true;
			if (e.type == EventType.MouseDown && e.button == 0) {
				Node originNode = SelectNode;
				if (ClickCheck (mousePos))
					SelectNode.SetConnect (originNode);
				
				linking = false;
				ResetSelect ();
			}
		} else {
			switch(e.type){
			case EventType.MouseDown:
				if (e.button == 0) {
					ClickCheck (e.mousePosition);
				} else if (e.button == 1) {
					if (ClickCheck(e.mousePosition))
						dType = DropdownType.select;
					else
						dType = DropdownType.normal;
				}
				break;


			case EventType.MouseDrag:
				if (SelectNode != null) {
					SelectNode.FollowMouse (e.delta);
				}else {
					coordinate += e.delta;
					GUI.changed = true;
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

	void DropdownMenu(Event e){
		if (dType == DropdownType.close)
			return;
		
		GenericMenu menu = new GenericMenu ();
		if (dType == DropdownType.normal) {
			menu.AddItem (new GUIContent ("Create New"), false, () => CreateNode (false));
			menu.AddSeparator ("");
			menu.AddDisabledItem (new GUIContent ("Create Child"));
			menu.AddDisabledItem (new GUIContent ("Make Connection"));
			menu.AddDisabledItem (new GUIContent ("Edit"));
			menu.AddDisabledItem (new GUIContent ("Delete"));
		} else {
			menu.AddItem (new GUIContent ("Create New"), false, () => CreateNode (false));
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Create Child"), false, () => CreateNode (true));
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

	void CreateStartNode(){
		Node n = new StartNode (new Vector2 (60, 100), "", null);
		lst_Node.Add (n);
	}

	void CreateNode(bool isChild){
		Node n;
		if (isChild) {
			n = new Node (SelectNode.rect.position + new Vector2 (SelectNode.NextNode.Count * 180, 100), "Dialog" + lst_Node.Count.ToString (), mySkin, SelectNode);
		}else{
			Node lastNode = lst_Node[lst_Node.Count - 1];
			n = new Node (lastNode.rect.position + 100 * Vector2.up, "Dialog" + lst_Node.Count.ToString (), mySkin, null);
		}
		lst_Node.Add (n);
	}

	void DeleteNode(Node _n){
		_n.DeleteAllConnect ();
		lst_Node.Remove (_n);
		SelectNode = null;
		GUI.changed = true;
	}
}

#region 暫存
/*
Vector2 SnapPosition(float _range, Vector2 _originPos){
		return new Vector2 (Mathf.Round (_originPos.x / _range) * _range, Mathf.Round (_originPos.y / _range) * _range);
	}
	void DrawConnectLine (){
		if (lst_Node.Count >= 2) {
			int i;
			for (i = 0; i < lst_Node.Count - 1; i++)
				Connect (lst_Node [i], lst_Node [i + 1]);

			Connect (lst_Node [i], End);
		}
			
	}

	void Connect(Node nodeA, Node nodeB){
		Vector2 pointA = new Vector2 (nodeA.canvasRect.center.x, nodeA.canvasRect.yMax);
		Vector2 pointB = new Vector2 (nodeB.canvasRect.center.x, nodeB.canvasRect.yMin);
		float dWidth = Mathf.Abs (pointA.x - pointB.x);

		if (pointA.y > pointB.y)
			return;

		if (dWidth < 8f) {
			Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);
		}else {
			Vector2 tan0 = new Vector2 (pointA.x, Mathf.Clamp (pointB.y + 20f, pointA.y + 15f, 99999));
			Vector2 tan1 = new Vector2 (pointB.x + 0.06f * dWidth, Mathf.Clamp (pointB.y - 80f, pointA.y - 15f, 99999));
			Vector2 pointC = new Vector2 (pointA.x, Mathf.Clamp (pointB.y - 90f, pointA.y, 99999));
			if(pointC.y - pointA.y >= 1f)
				Handles.DrawBezier (pointA, pointC, pointA, pointC, Color.white, null, 2f);
			Handles.DrawBezier (pointC, pointB, tan0, tan1, Color.white, null, 2f);
		}
	}
*/
#endregion
