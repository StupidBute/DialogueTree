using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface i_Interactable{
	void Interacted ();
}

public class sc_Interactable : MonoBehaviour {
	public bool canInteract = true;
	[SerializeField]
	string playerAnimKey = "";
	[SerializeField]
	float playerAnimTime = 0f;
	[SerializeField]
	float playerXOffset = 0f;
	[Header("互動時需要腳色面朝方向(-1/0/1)")]
	[Range(-1, 1)]
	[SerializeField]
	int requiredDirect = 0;

	[Space(5)]
	[SerializeField]
	protected string plotKey = "";
	[SerializeField]
	bool newCanInteract = false;

	sc_player scPlayer;
	sc_CamFollow scCam;
	Animator anim_EHint;
	int playerMask = 0;

	virtual protected void Start () {
		scPlayer = sc_AICenter.AI.GetPlayerSC ();
		scCam = sc_God.MainCam.scCam;
		anim_EHint = GetComponentInChildren<Animator> ();
		playerMask = 1 << LayerMask.NameToLayer ("Player");
	}

	void Update(){
		//檢測互動物件需求方向和玩家面相方向是否一致
		bool playerFacing = requiredDirect * scPlayer.face < 0 ? false : true;

		if (canInteract && scPlayer.CanControl() && playerFacing) {
			RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0.2f, 0, 0), Vector3.left, 0.4f, playerMask);
			anim_EHint.SetBool ("on", hit.collider != null);
		} else {
			anim_EHint.SetBool ("on", false);
		}
	}

	public void StartInteractable(){
		if (canInteract) {
			StartCoroutine (IE_StartInteractable ());
			canInteract = newCanInteract;
		}
	}

	IEnumerator IE_StartInteractable(){
		scPlayer.ActiveControl (1, false);
		if (playerAnimTime > 0.01f) {
			scCam.SetInteractScale (true);
			if (playerAnimKey != "")
				scPlayer.SetAnim (playerAnimKey);
			Vector3 playerPos = scPlayer.transform.position;
			scPlayer.transform.position = new Vector3 (transform.position.x + playerXOffset, playerPos.y, playerPos.z);
			yield return new WaitForSeconds (playerAnimTime);
			scPlayer.SetAnim ("idle");
			scCam.SetInteractScale (false);
		}

		if(plotKey != "")
			sc_DialogGod.SetPlotFlag (plotKey, true);
		
		scPlayer.ActiveControl (1, true);
		yield break;
	}
}
