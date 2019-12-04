using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_AICenter : MonoBehaviour {
	//攝影機移動所用
	public float camLeftX = -10f, camRightX = 10f;
	GameObject MyPlayer;
	sc_player playerSC;
	Transform playerTR;

	static public sc_AICenter AI;

	void Awake () {
		AI = this;
		MyPlayer = GameObject.FindGameObjectWithTag ("Player");
		playerTR = MyPlayer.transform;
		playerSC = MyPlayer.GetComponent<sc_player> ();

	}

	/*
	public void HitPlayer(){
		playerSC.Hit ();
	}*/

	public Transform GetPlayerTR(){ return playerTR; }
	public sc_player GetPlayerSC(){	return playerSC; }
}

