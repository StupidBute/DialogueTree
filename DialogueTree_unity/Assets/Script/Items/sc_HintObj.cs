using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class sc_HintObj : MonoBehaviour {
	[SerializeField]
	[Range(1, 5)]
	float range = 1;
	[SerializeField]
	float distanceRatio = 1;
	[SerializeField]
	bool isOn = true;
	sc_player playerSC;
	Transform playerTR;
	Transform canvasTR;
	Text myText;

	void Start () {
		playerSC = sc_AICenter.AI.GetPlayerSC ();
		playerTR = sc_AICenter.AI.GetPlayerTR ();
		canvasTR = transform.GetChild (0);
		myText = GetComponentInChildren<Text> ();
	}

	void Update () {
		float dx = playerTR.position.x - transform.position.x;
		if (playerSC.CanControl () && Mathf.Abs (playerTR.position.y - transform.position.y) < 2 && Mathf.Abs (dx) < 0.5f * range)
			HintOn (true);
		else
			HintOn (false);
		
		float posX = transform.position.x - dx * distanceRatio;
		canvasTR.position = new Vector2 (posX, canvasTR.position.y);
	}

	void HintOn(bool _on){
		if (isOn == _on)
			return;
		isOn = _on;
		if (_on)
			myText.DOFade (1, 0.6f);
		else
			myText.DOFade (0, 0.6f);
	}
}
