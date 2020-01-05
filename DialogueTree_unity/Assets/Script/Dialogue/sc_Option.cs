using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class sc_Option : MonoBehaviour {
	public int Answer = -1;
	Camera cam;
	Transform myPlayer;
	GameObject[] myOptions = new GameObject[8];
	Animator anim;
	char[] optionSpliter = new char[]{'_'};
	Text[] optionTxt = new Text[8];
	Vector2 originRectScale;
	Coroutine lastCoroutine = null;
	int baseIndex = 0;

	void Start () {
		cam = Camera.main;
		myPlayer = transform.parent;
		anim = GetComponent<Animator> ();
		for (int i = 0; i < 8; i++) {
			myOptions [i] = transform.GetChild (i).gameObject;
			optionTxt [i] = myOptions [i].GetComponentInChildren<Text> ();
		}
		originRectScale = myOptions [0].transform.localScale;
	}

	public void ChooseAnswer(List<Option> lst_option){
		transform.localPosition = new Vector2 (-2.17f, 1.72f);
		lastCoroutine = StartCoroutine (IE_ChooseAnswer (lst_option));
	}

	public void CloseQuestion(){
		if (lastCoroutine != null) {
			for (int i = 0; i < 4; i++)
				myOptions [i].SetActive (false);
			StopCoroutine (lastCoroutine);
		}
	}

	IEnumerator IE_ChooseAnswer(List<Option> lst_option){
		int index = -1;
		int optionCount = lst_option.Count;
		//打開選項
		bool isLeft = myPlayer.position.x < cam.transform.position.x;
		transform.localPosition = isLeft ? 
			new Vector2 (-Mathf.Abs (transform.localPosition.x), transform.localPosition.y) 
			: new Vector2 (Mathf.Abs (transform.localPosition.x), transform.localPosition.y);
		baseIndex = isLeft ? 0 : 4;
		for (int i = 0; i < lst_option.Count; i++) {
			optionTxt [baseIndex + i].text = lst_option [i].text;
			myOptions [baseIndex + i].SetActive (true);
		}
		anim.SetBool ("activate", true);
		yield return new WaitForSeconds (0.8f);
		anim.enabled = false;

		#region 選選項
		GameObject pointedOption = null;
		bool mouseControl = true;
		Vector3 pre_mousePos = Input.mousePosition;
		while (index == -1) {			//選擇第一層選項
			PointMyOption(ref pointedOption, ref mouseControl, ref pre_mousePos);

			if (pointedOption != null){
				if (Input.GetMouseButtonDown (0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
					index = int.Parse (pointedOption.name.Split (optionSpliter) [0]);
				else
					ScaleOptions(pointedOption.transform);
			}else
				ScaleOptions();

			yield return null;
		}
		for(int i = 0; i < 4; i++)
			myOptions[i].transform.DOScale(originRectScale, 0.3f);
		anim.enabled = true;
		anim.SetBool ("activate", false);
		yield return new WaitForSeconds (0.5f);
		#endregion

		//關閉全部選項
		for (int i = 0; i < 4; i++)		
			myOptions [baseIndex + i].SetActive (false);
		Answer = index;
	}

	void ScaleOptions(Transform selected){
		Vector2 bigScale = 1.1f * originRectScale;

		for (int i = 0; i < 4; i++) {
			Transform nowTR = myOptions [baseIndex + i].transform;
			if (nowTR == selected) {
				float dScale = bigScale.x - nowTR.localScale.x;
				if (dScale > 0f) {
					float scaleSpd = (0.1f + Mathf.Abs(dScale * 9f)) * Time.deltaTime;
					nowTR.localScale = new Vector2 (nowTR.localScale.x + scaleSpd, nowTR.localScale.y + scaleSpd);
				}else
					nowTR.localScale = bigScale;
					
			} else {
				float dScale = originRectScale.x - nowTR.localScale.x;
				if (nowTR.localScale.x > originRectScale.x) {
					float scaleSpd = (0.1f + Mathf.Abs(dScale * 9f)) * Time.deltaTime;
					nowTR.localScale = new Vector2 (nowTR.localScale.x - scaleSpd, nowTR.localScale.y - scaleSpd);
				}else
					nowTR.localScale = originRectScale;
			}
		}
	}

	void ScaleOptions(){
		for (int i = 0; i < 4; i++) {
			Transform nowTR = myOptions [baseIndex + i].transform;
			float dScale = originRectScale.x - nowTR.localScale.x;
			if (dScale < 0f) {
				float scaleSpd = (0.1f + Mathf.Abs(dScale * 9f)) * Time.deltaTime;
				nowTR.localScale = new Vector2 (nowTR.localScale.x - scaleSpd, nowTR.localScale.y - scaleSpd);
			}else
				nowTR.localScale = originRectScale;
		}
	}

	void PointMyOption(ref GameObject pointedOption, ref bool mouseControl, ref Vector3 pre_mousePos){
		bool[] pressOptionKey = new bool[4];
		pressOptionKey[0] = Input.GetKeyDown(KeyCode.A);
		pressOptionKey[1] = Input.GetKeyDown(KeyCode.W);
		pressOptionKey[2] = Input.GetKeyDown(KeyCode.D);
		pressOptionKey[3] = Input.GetKeyDown(KeyCode.S);
		RaycastHit2D hit = MouseClick (Input.mousePosition);
		if(Input.mousePosition != pre_mousePos && hit.collider != null){
			mouseControl = true;
			pointedOption = hit.collider.gameObject;
		}else{
			for(int i = 0; i < 4; i++){
				if(pressOptionKey[i] && myOptions[i].activeSelf){
					mouseControl = false;
					pointedOption = myOptions[i];
				}
			}
			if(mouseControl){
				if(hit.collider != null)
					pointedOption = hit.collider.gameObject;
				else
					pointedOption = null;
			}
		}
		pre_mousePos = Input.mousePosition;
	}

	RaycastHit2D MouseClick(Vector3 _pos){
		Ray _ray = cam.ScreenPointToRay (_pos);
		return Physics2D.Raycast (_ray.origin, _ray.direction, 10f, 1 << 5);
  	}
}

