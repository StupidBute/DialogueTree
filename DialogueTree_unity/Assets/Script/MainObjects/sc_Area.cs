using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface i_AreaListener{
	void ChangeArea (string _nowArea);
}

public class sc_Area : MonoBehaviour {
	public static string NowArea = "";
	static bool nowForceMove = false;
	[SerializeField]
	string AreaCode = "A";
	sc_AICenter AI;
	Transform playerTR;
	sc_player scPlayer;

	//上下左右	(攝影機)左右	(門出口)左右
	float[] myBorder = new float[6];
	int areaMask = 0, wallMask = 0;
	WaitForSeconds checkTime = new WaitForSeconds(0.05f);

	static List<i_AreaListener> myListeners = new List<i_AreaListener> ();

	void Awake(){
		NowArea = "";
		nowForceMove = false;
		myListeners.Clear ();
		NowArea = "";
		nowForceMove = false;
	}

	void Start () {
		AI = sc_AICenter.AI;
		playerTR = AI.GetPlayerTR ();
		scPlayer = playerTR.GetComponent<sc_player> ();
		areaMask = 1 << LayerMask.NameToLayer ("Area");
		wallMask = 1 << LayerMask.NameToLayer ("Wall");

		for (int i = 0; i < transform.childCount; i++) {
			Transform border = transform.GetChild (i);
			if (i < 2)
				myBorder [i] = border.position.y;
			else
				myBorder [i] = border.position.x;

			if (i != 2 && i != 3)
				Destroy (border.gameObject);
			else
				Destroy (border.GetComponent<SpriteRenderer> ());
		}
		StartCoroutine (CheckPlayerInArea ());
		StartCoroutine (WaitSendArea ());
	}

	static public void RegisterListener(i_AreaListener _listener){
		myListeners.Add (_listener);
	}

	IEnumerator CheckPlayerInArea(){
		if (AreaCode != NowArea) {
			if (playerTR.position.x > myBorder [2] && playerTR.position.x < myBorder [3]
				&& playerTR.position.y < myBorder [0] && playerTR.position.y > myBorder [1]){
				NowArea = AreaCode;
				AI.camLeftX = myBorder [4];
				AI.camRightX = myBorder [5];
				foreach (i_AreaListener AL in myListeners)
					AL.ChangeArea (NowArea);
			} 
		}else if (!nowForceMove && (playerTR.position.x < myBorder [2] || playerTR.position.x > myBorder [3])) {
			StartCoroutine (IE_ForcePlayerMove ());
		}
		yield return checkTime;
		StartCoroutine (CheckPlayerInArea ());
	}

	IEnumerator IE_ForcePlayerMove(){
		//change the way to determine exit pos
		if (playerTR.position.x > myBorder[3] - 0.1f) {
			RaycastHit hit;
			if (Physics.Raycast (playerTR.position - Vector3.right * 0.5f, Vector3.right, out hit, 4, wallMask) && hit.collider.tag != "Dynamic")
				yield break;
			if (Physics.Raycast (playerTR.position + Vector3.right * 0.3f, Vector3.right, out hit, 10, areaMask)) {
				nowForceMove = true;
				scPlayer.ActiveControl (1, false);
				if (hit.distance > 1f) {
					yield return StartCoroutine (scPlayer.MoveToPos (new Vector2 (playerTR.position.x + 0.8f, playerTR.position.y)));
					playerTR.Translate (hit.distance - 1f, 0, 0);
					scPlayer.SnapFloor (2);
					yield return StartCoroutine (scPlayer.MoveToPos (new Vector2 (hit.point.x + 0.5f, playerTR.position.y)));
				}else
					yield return StartCoroutine (scPlayer.MoveToPos (new Vector2 (hit.point.x + 0.5f, playerTR.position.y)));
				
			}
				
		} else {
			RaycastHit hit;
			if (Physics.Raycast (playerTR.position - Vector3.left * 0.5f, Vector3.left, out hit, 4, wallMask))
				yield break;
			if (Physics.Raycast (playerTR.position + Vector3.left * 0.3f, Vector3.left, out hit, 10, areaMask)) {
				nowForceMove = true;
				scPlayer.ActiveControl (1, false);
				if (hit.distance > 1.7f) {
					yield return StartCoroutine (scPlayer.MoveToPos (new Vector2 (playerTR.position.x - 0.8f, playerTR.position.y)));
					playerTR.Translate (-hit.distance + 1f, 0, 0);
					scPlayer.SnapFloor (2);
					yield return StartCoroutine (scPlayer.MoveToPos (new Vector2 (hit.point.x - 0.5f, playerTR.position.y)));
				} else {
					yield return StartCoroutine (scPlayer.MoveToPos (new Vector2 (hit.point.x - 0.5f, playerTR.position.y)));
				}
					

			}
		}
		nowForceMove = false;
		scPlayer.ActiveControl (1, true);
	}

	IEnumerator WaitSendArea(){
		yield return new WaitForSeconds (0.05f);
		foreach (i_AreaListener AL in myListeners)
			AL.ChangeArea (NowArea);
	}
}
