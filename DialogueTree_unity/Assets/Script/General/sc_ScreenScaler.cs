using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_ScreenScaler : MonoBehaviour {
	sc_CamFollow scCam;
	float SizeScaleRate = 0.2f;

	void Start () {
		scCam = sc_God.MainCam.scCam;
		SizeScaleRate = transform.localScale.x / scCam.camSize;
	}

	public void Scaling(){
		float newScale = scCam.camSize * SizeScaleRate;
		transform.localScale = new Vector2 (newScale, newScale);
	}
}
