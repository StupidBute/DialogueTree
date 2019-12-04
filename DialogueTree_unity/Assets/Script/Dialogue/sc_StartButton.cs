using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sc_StartButton : MonoBehaviour {
	public static int NpcIndex = -1;
	public int myIndex = 0;
	sc_NpcDialog myNpcDial;
	int lastIndex = -1;
	GameObject DropdownObj, StartObj, StopObj;
	Dropdown myDropDown;

	// Use this for initialization
	void Start () {
		NpcIndex = -1;
		myNpcDial = GetComponent<sc_NpcDialog> ();
		Transform NpcCanvas = transform.GetChild (1);
		DropdownObj = NpcCanvas.GetChild (1).gameObject;
		StartObj = NpcCanvas.GetChild (2).gameObject;
		StopObj = NpcCanvas.GetChild (3).gameObject;
		myDropDown = DropdownObj.GetComponent<Dropdown> ();
		StopObj.SetActive (false);
		StartCoroutine (WaitSetValue ());

	}
	
	// Update is called once per frame
	void Update () {
		if (lastIndex != NpcIndex) {
			lastIndex = NpcIndex;
			if (NpcIndex == -1) {
				myNpcDial.StopSheet ();
				SetMyObjActive (true, true, false);
			} else if (NpcIndex == myIndex) {
				SetMyObjActive (false, false, true);
			} else {
				SetMyObjActive (false, false, false);
			}
		}

	}

	void SetMyObjActive(bool _drop, bool _start, bool _stop){
		DropdownObj.SetActive (_drop);
		StartObj.SetActive (_start);
		StopObj.SetActive (_stop);
	}

	public void Change2myIndex(){
		NpcIndex = myIndex;
	}

	public void PressStart(){
		if (NpcIndex == myIndex) {
			NpcIndex = -1;
		} else {
			NpcIndex = myIndex;
			myNpcDial.StartSheetAt (myNpcDial.mySheet.dialogOrder[myDropDown.value]);
		}
	}

	IEnumerator WaitSetValue (){
		yield return null;
		myDropDown.AddOptions (myNpcDial.mySheet.dialogOrder);
		transform.GetChild (1).GetChild (0).GetComponent<Text> ().text = myNpcDial.myName;
	}
}
