using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Detector : sc_Interactable, i_Interactable {
	static int openCount = 0;
	Animator anim;

	override protected void Start(){
		base.Start ();
		if (!sc_DialogGod.ContainsPF ("DetectorOpenCount0")) {
			openCount = 0;
			sc_DialogGod.SetPlotFlag ("DetectorOpenCount0", true);
		}
		anim = GetComponent<Animator> ();
		StartCoroutine (IE_RandomStart ());
	}

	public void Interacted(){
		StartInteractable ();
		anim.SetTrigger ("off");
		sc_DialogGod.SetPlotFlag ("DetectorOpenCount" + openCount.ToString (), false);
		openCount++;
		sc_DialogGod.SetPlotFlag ("DetectorOpenCount" + openCount.ToString (), true);
	}

	IEnumerator IE_RandomStart(){
		yield return new WaitForSeconds (Random.Range (0f, 0.99f));
		anim.SetTrigger ("on");
	}
}
