using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class sc_AreaBlack : MonoBehaviour, i_AreaListener {
	const float originAlpha = 0.9f;
	const float fadeTime = 0.5f;
	[SerializeField]
	string AreaCode = "A";
	[SerializeField]
	string[] OverlapArea;
	Transform playerTR;
	SpriteRenderer spr;

	void Start () {
		spr = GetComponent<SpriteRenderer> ();
		playerTR = sc_AICenter.AI.GetPlayerTR ();
		sc_Area.RegisterListener (this);
	}

	public void ChangeArea(string _nowArea){
		if (spr.enabled && Mathf.Abs (playerTR.position.z - transform.position.z) > 1f)
			spr.enabled = false;
		else if (!spr.enabled && Mathf.Abs (playerTR.position.z - transform.position.z) < 1f)
			spr.enabled = true;

		bool isOverlap = false;
		for (int i = 0; i < OverlapArea.Length; i++) {
			if (_nowArea == OverlapArea [i]) {
				isOverlap = true;
				break;
			}
		}
		if (_nowArea == AreaCode || isOverlap)
			spr.DOFade (0f, fadeTime);
		else
			spr.DOFade (originAlpha, fadeTime);
	}
}
