using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Detector : sc_Interactable, i_Interactable {
	static int openCount = 0;

	void Awake(){
		openCount = 0;
	}

	public void Interacted(){
		StartInteractable ();
		sc_DialogGod.SetPlotFlag ("DetectorOpenCount" + openCount.ToString (), false);
		openCount++;
		sc_DialogGod.SetPlotFlag ("DetectorOpenCount" + openCount.ToString (), true);
	}

}
