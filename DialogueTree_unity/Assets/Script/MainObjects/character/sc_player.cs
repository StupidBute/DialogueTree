using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_player : sc_character {
	[System.NonSerialized]
	public bool canInteract = true;
	bool[] controlSwitches = new bool[]{true, true, true};	//0:dialog	1:area	2.plot

	override protected void Start(){
		base.Start ();
	}

	override protected void Update(){
		if (CanControl ()) {
			bool moveRight = Input.GetKey (KeyCode.D);
			bool moveLeft = Input.GetKey (KeyCode.A);
			bool moveUp = Input.GetKey (KeyCode.W);
			bool moveDown = Input.GetKey (KeyCode.S);
			bool jogging = Input.GetKey (KeyCode.LeftShift);
			SetMove (moveRight, moveLeft, jogging);
			SetStairMove (moveUp, moveDown);
			if (Input.GetKeyDown (KeyCode.E) && canInteract)
				DoInteractable ();
				
		}
		base.Update ();
	}
	public void DoInteractable(){
		RaycastHit hit;
		if (Physics.Raycast (transform.position, Vector3.up, out hit, 2f, mask_Interactable)) {
			/*
			if (hit.collider.tag == "Tag_Elevator") {
				if (hit.transform.childCount != 0)
					hit.collider.GetComponent<sc_EleDoor> ().PlayerGetInElevator (true, this);
				else
					hit.transform.parent.gameObject.GetComponent<sc_EleDoor> ().PressButton (-1, true, true);
			} else if (hit.collider.tag == "Tag_RoomDoor") {
				hit.collider.GetComponentInParent<sc_BuildingLayer> ().EnterRoom (this);
			} else {
				i_Interactable _hitIterface;
				_hitIterface = hit.collider.GetComponent<i_Interactable> ();
				if (_hitIterface != null)
					_hitIterface.Interacted ();
				else
					hit.collider.GetComponent<sc_Interactable> ().StartInteractable ();
			}*/
			print ("interact something");
		}
	}

	public void ActiveControl(int controlIndex, bool _active){
		controlSwitches[controlIndex] = _active;
		SetMove (false, false, false);
	}

	public bool CanControl(){
		return controlSwitches[0] && controlSwitches[1] && controlSwitches[2];
	}

}
