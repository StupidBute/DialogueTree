using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Console : MonoBehaviour, i_PlotFlag {
	Animator anim;

	void Start(){
		anim = GetComponent<Animator> ();
		sc_DialogGod.RegisterListener (this);
	}

	public void FlagAdd (string _key){
		if (_key == "DetectorOpenCount1")
			anim.SetTrigger ("mid");
		else if (_key == "DetectorOpenCount4")
			anim.SetTrigger ("slow");
	}
	public void FlagRemove (string _key){}
}
