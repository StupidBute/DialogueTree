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
	public List<Node> NextNode = new List<Node> ();
	public List<Node> PrevNode = new List<Node> ();
	public Rect rect;
	string title;

	GUIStyle style_label, style_box, style_selectedBox;
	Texture2D tex_end;
	Vector2 dragPos;

	const float gridSize = 20;
	protected bool isSelected = false;

	public Node(Vector2 _pos, string _title, GUISkin mySkin, Node _prevNode){
		rect = new Rect (_pos, new Vector2 (156, 56));
		canvasRect = rect;
		title = _title;
		SetConnect (_prevNode);

		if (mySkin != null) {
			style_label = mySkin.GetStyle ("textfield");
			style_box = mySkin.GetStyle ("box");
			style_selectedBox = mySkin.GetStyle ("selectedBox");
			tex_end = Resources.Load<Texture2D> ("GUISkin/EndNode2");

		}

	}

	virtual public void DrawSelf(Vector2 coordinate){
		if (NextNode.Count == 0 && PrevNode.Count > 0){
			Rect endRect = new Rect (canvasRect.position + 37 * Vector2.up, new Vector2 (156, 33));
			GUI.DrawTexture (endRect, tex_end);
		}

		canvasRect = rect;
		canvasRect.position += coordinate;
		if (isSelected)
			GUI.Box (canvasRect, title, style_selectedBox);
		else
			GUI.Box (canvasRect, title, style_box);

		Rect labelRect = canvasRect;
		labelRect.position += new Vector2 (10, 25);
		labelRect.size = new Vector2 (136, 20);
		GUI.Label (labelRect, "對話內容", style_label);
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

	public void SetConnect(Node _prevNode){
		if (_prevNode != null) {
			_prevNode.NextNode.Add (this);
			PrevNode.Add (_prevNode);
		}
	}

	public void DeleteAllConnect(){
		foreach (Node pn in PrevNode)
			pn.NextNode.Remove (this);
		foreach (Node nn in NextNode)
			nn.PrevNode.Remove (this);
	}

}

public class StartNode : Node{
	protected Texture2D normTex, selectTex;

	public StartNode(Vector2 _pos, string _text, GUISkin mySkin):base(_pos, _text, mySkin, null){
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
	Vector2 settingSize = new Vector2 (20, 20);

	public NodeLink(Node na, Node nb){
		nodeA = na;
		nodeB = nb;
		rectEdit = new Rect (new Vector2 (nb.canvasRect.center.x - 28, nb.canvasRect.yMin - 25), new Vector2 (56, 17));
		tex_arrows [0] = Resources.Load<Texture2D> ("GUISkin/ArrowD");
		tex_arrows [1] = Resources.Load<Texture2D> ("GUISkin/ArrowU");
		tex_arrows [2] = Resources.Load<Texture2D> ("GUISkin/ArrowR");
		tex_arrows [3] = Resources.Load<Texture2D> ("GUISkin/ArrowL");
		tex_setting = Resources.Load<Texture2D> ("GUISkin/SettingGear");
	}

	void DrawSelf(){
		Vector2 pointA = nodeA.canvasRect.center;
		Vector2 pointB = nodeB.canvasRect.center;
		float dWidth = Mathf.Abs (pointA.x - pointB.x);

		if (Mathf.Abs (pointA.y - pointB.y) < 56) {
			if (dWidth < 158)
				return;
			if (pointA.x < pointB.x) {
				pointA.x = nodeA.canvasRect.xMax - 5;
				pointB.x = nodeB.canvasRect.xMin + 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-13, -5), arrowSize), tex_arrows [2]);
				GUI.DrawTexture (new Rect (pointB + new Vector2 (-settingSize.x - 20, -0.5f * settingSize.y), settingSize), tex_setting);
			} else {
				pointA.x = nodeA.canvasRect.xMin + 5;
				pointB.x = nodeB.canvasRect.xMax - 5;
				GUI.DrawTexture (new Rect (pointB + new Vector2 (3, -5), arrowSize), tex_arrows [3]);
				GUI.DrawTexture (new Rect (pointB + new Vector2 (20, -0.5f * settingSize.y), settingSize), tex_setting);
			}
			Handles.DrawBezier (pointA, pointB, pointA, pointB, Color.white, null, 2f);

		} else if (pointA.y < pointB.y) {
			pointA.y = nodeA.canvasRect.yMax - 4;
			pointB.y = nodeB.canvasRect.yMin + 4;

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
			GUI.DrawTexture (new Rect (pointB + new Vector2 (-0.5f * settingSize.x, -settingSize.y - 20), settingSize), tex_setting);

		} else {
			pointA.y = nodeA.canvasRect.yMin + 4;
			pointB.y = nodeB.canvasRect.yMax - 4;
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
			GUI.DrawTexture (new Rect (pointB + new Vector2 (-0.5f * settingSize.x, 20), settingSize), tex_setting);
		}

	}
}

