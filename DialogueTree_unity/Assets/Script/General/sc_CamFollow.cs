using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class sc_CamFollow : MonoBehaviour {
	const float followSpd = 2f, sizeSpd = 2.6f, AccSpd = 30f;
	const float interactScale = 0.9f, talkCamScale = 0.85f, parkourScale = 0.9f;

	public float camSize = 4.8f;
	[SerializeField]
	float[] cam_dy = new float[2];
	[SerializeField]
	GameObject parkourEffect;

	sc_AICenter AI;
	Transform playerTR;
	Transform camTR;
	List<Transform> followTR = new List<Transform>();
	Camera cam;
	Image[] focusBlack;


	Tween lastDOOrtho = null;
	Sequence lastSeq = null;
	Coroutine lastCo = null;

	bool freeCamera = false;
	bool isParkourCam = false;
	float moveSpd = 0f;
	float ResolutionAdjust = 1f;


	[System.NonSerialized]
	public float targetSize;
	[System.NonSerialized]
	public Vector2 targetPos;

	void Start () {
		AI = sc_AICenter.AI;
		playerTR = AI.GetPlayerTR ();
		followTR.Add(playerTR);
		cam = Camera.main;
		if (cam.transform.parent != null)
			camTR = cam.transform.parent;
		else
			camTR = cam.transform;
		focusBlack = GetComponentsInChildren<Image> ();
		StartCoroutine (ActiveFocusBlack (false, 0f));

		#region 處理不同比例解析度
		if(cam.aspect < 1.7f){
			float expectedHeight = Screen.width / 1.7f;
			ResolutionAdjust = expectedHeight / Screen.height;
			OrthoSizeCamera(camSize, 1f);
			SetFocusBlack(0f, 1f);
		}
		#endregion

	}

	void Update () {
		if (!freeCamera) {
			if (followTR.Count > 0) {
				Vector2 targetPos = CalculateTargetPos ();
				float targetSpd = 0f;
				float dist = Vector2.Distance (targetPos, (Vector2)camTR.position);
				float dx = targetPos.x - camTR.position.x;
				float dy = targetPos.y - camTR.position.y;
				float angle = Mathf.Atan2(dy, dx);
				if (Mathf.Abs (dist) > 0.05f) {
					targetSpd = dist * followSpd;
					SmoothCamSpd (targetSpd);
					camTR.Translate (moveSpd * Mathf.Cos (angle) * Time.deltaTime, 
						moveSpd * Mathf.Sin (angle) * Time.deltaTime, 0, 
						Space.World);
				} else
					moveSpd = 0;
					
			}
		}
		
	}

	Vector2 CalculateTargetPos(){
		if (followTR.Count == 1) {
			float _x;
			if (AI != null)
				_x = Mathf.Clamp (followTR [0].position.x, AI.camLeftX, AI.camRightX);
			else
				_x = followTR [0].position.x;
			float _y = followTR [0].position.y + cam_dy [0];
			return new Vector2 (_x, _y);
		} else {
			float _x = (followTR [0].position.x + followTR [1].position.x) / 2f;
			float _y = (followTR [0].position.y + followTR [1].position.y) / 2f + cam_dy [1];
			return new Vector2 (_x, _y);
		}
	}

	void SmoothCamSpd(float targetSpd){
		if (Mathf.Abs (targetSpd - moveSpd) < 4f) {
			moveSpd = targetSpd;
		} else {
			if (moveSpd > targetSpd)
				moveSpd -= AccSpd * Time.deltaTime;
			else
				moveSpd += AccSpd * Time.deltaTime;
		}
	}

	public void CamParkour(bool _activate){
		if (_activate) {
			if (!isParkourCam) {
				isParkourCam = true;
				OrthoSizeCamera (camSize * parkourScale, 0.5f);
				parkourEffect.SetActive (true);
			}
		} else {
			if (isParkourCam) {
				isParkourCam = false;
				OrthoSizeCamera (camSize, 0.5f);
				parkourEffect.SetActive (false);
			}
		}

	}
	public void ParkourEffect(bool _activate){
		parkourEffect.SetActive (_activate);
	}

	public void SetFollowTarget(Transform target0, Transform target1, bool _orthoCam){
		followTR.Clear ();
		followTR.Add (target0);
		followTR.Add (target1);
		if (!freeCamera && _orthoCam) {
			//放大
			OrthoSizeCamera (camSize * talkCamScale, 1.6f);
		}
	}

	public void SetFollowTarget(Transform target){
		followTR.Clear ();
		followTR.Add (target);
	}

	public void SetFollowTarget(bool followPlayer){
		followTR.Clear ();
		if (followPlayer)
			followTR.Add (playerTR);
		if (!freeCamera) {
			//回復
			OrthoSizeCamera (camSize, 1.6f);
		}
			
	}

	public void SetInteractScale(bool _activate){
		float _targetSize = _activate ? camSize * interactScale : camSize;
		OrthoSizeCamera (_targetSize, 1f);
	}

	public void FreeCamera(bool _free){
		freeCamera = _free;
		if (!_free && followTR.Count <= 1)
			SetFollowTarget (true);
	}

	public void SetFocusBlack(float _amount, float _time){
		if (ResolutionAdjust <= 0.999f)
			_amount += 1f - ResolutionAdjust;
		if (lastCo != null)
			StopCoroutine (lastCo);
		if (_amount != 0)
			StartCoroutine (ActiveFocusBlack (true, 0f));
		focusBlack [0].DOFillAmount (_amount * 0.5f, _time).SetEase (Ease.OutCubic);
		focusBlack [1].DOFillAmount (_amount * 0.5f, _time).SetEase (Ease.OutCubic);
		if (_amount == 0)
			lastCo = StartCoroutine (ActiveFocusBlack (false, _time));
	}

	//進電梯&角色對話時的鏡頭縮放
	public void OrthoSizeCamera(float _size, float _time){
		if (_size < 0.0001f) {
			print ("error");
			Debug.Break ();
		}
		if (lastDOOrtho != null)
			lastDOOrtho.Kill ();
		lastDOOrtho = cam.DOOrthoSize (_size / ResolutionAdjust, _time).SetEase (Ease.OutCubic);
	}

	//自由鏡頭下的鏡頭縮放和移動(可由talkNPC和facGod.CheckPlayerInArea呼叫)
	public void CamAnim(float _size, Vector2 _pos, float _time){
		StopCamAnim ();
		targetSize = _size;
		targetPos = _pos;

		freeCamera = true;
		Vector3 cam_pos = new Vector3 (_pos.x, _pos.y, -10);
		lastSeq = DOTween.Sequence();

		Ease easeType = _size < 6f ? Ease.OutCubic : Ease.OutQuint;
		lastSeq.Append (cam.DOOrthoSize (_size / ResolutionAdjust, _time).SetEase (easeType))
			.Join (camTR.DOMove (cam_pos, _time, false).SetEase (easeType));
	}

	public void SetCamSize(float _size){
		if (camSize != _size) {
			camSize = _size;
			OrthoSizeCamera (camSize, 1.6f);
		}
	}

	public void StopCamAnim(){
		if (lastDOOrtho != null)
			lastDOOrtho.Kill ();
		if (lastSeq != null)
			lastSeq.Kill ();
	}

	IEnumerator ActiveFocusBlack(bool _activate, float _time){
		if (_time != 0f)
			yield return new WaitForSeconds (_time);
		focusBlack [0].enabled = _activate;
		focusBlack [1].enabled = _activate;
		yield break;
	}

}
