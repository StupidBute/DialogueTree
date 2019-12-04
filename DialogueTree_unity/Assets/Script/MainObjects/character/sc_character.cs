using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class sc_character : MonoBehaviour {
	const float shadowAlpha = 0.5f;
	protected int mask_Interactable = 0, mask_Wall = 0, mask_Floor = 0;

	public float halfBodyWidth = 0.4f;
	public float walkSpd = 3f;
	public AudioSource walkSound;
	public string insideBuilding = "";

	//[System.NonSerialized]
	public int face = 1;
	[System.NonSerialized]
	public bool onEle = false;


	SpriteRenderer spr;
	SpriteRenderer spr_shadow = null;
	protected Animator anim;
	[SerializeField]
	bool useAcc = false;
	[System.NonSerialized]
	public bool Up = false, Down = false;
	bool Right = false, Left = false;
	protected bool canMove = true;
	protected float nowSpd = 0f;
	bool jogging = false;
	bool isNormalState = true;
	bool onSlope = false;
	int originSortOrder = 0;
	string lastAnimKey = "";
	virtual protected void Start(){
		mask_Wall = 1 << LayerMask.NameToLayer ("Wall");
		mask_Floor = 1 << LayerMask.NameToLayer ("Floor");
		mask_Interactable = 1 << LayerMask.NameToLayer ("Interactable");

		spr = GetComponent<SpriteRenderer> ();
		anim = GetComponent<Animator> ();
		if (transform.childCount > 0) {
			spr_shadow = transform.GetChild (0).GetComponent<SpriteRenderer> ();
			if(spr_shadow != null)
				spr_shadow.flipX = spr.flipX;
		}
		if(insideBuilding == "")
			insideBuilding = "HallWay";
		originSortOrder = spr.sortingOrder;
		face = spr.flipX ? -1 : 1;
		isNormalState = anim.GetBool ("normalState");
		SnapFloor (1f);
  	}

	virtual protected void Update(){
		if(canMove)
			Move (Right, Left);
		isNormalState = anim.GetBool ("normalState");
	}

	#region 位移函式
	public IEnumerator GoUpStairs(int floors, bool isRight, float Xpos){
		if (isRight) {
			while (floors > 0) {
				yield return StartCoroutine (MoveToPos (new Vector2 (12.6f, 0)));
				yield return StartCoroutine (MoveToPos (new Vector2 (17.65f, 0)));
				floors--;
			}
		} else {
			while (floors > 0) {
				yield return StartCoroutine (MoveToPos (new Vector2 (-30.5f, 0)));
				yield return StartCoroutine (MoveToPos (new Vector2 (-35.75f, 0)));
				floors--;
			}
		}
		yield return StartCoroutine (MoveToPos (new Vector2 (Xpos, 0)));
	}

	public IEnumerator MoveToPos(Vector2 _pos){
		float _dx = _pos.x - transform.position.x;
		float _dy = _pos.y - transform.position.y;

		//不同層
		if(Mathf.Abs(_dy) > 0.8f){
			//sc_AICenter.AI.
		}

		//同一層
		if (_dx > 0) {
			while (_dx > 0 && canMove) {
				if (Right == false)
					SetMove (true, false);
				_dx = _pos.x - transform.position.x;
				yield return null;
			}
		} else {
			while (_dx <= 0 && canMove) {
				if(Left == false)
					SetMove (false, true);
				_dx = _pos.x - transform.position.x;
				yield return null;
			}
		}
		SetMove (false, false);
	}

	public void SnapFloor(float snapRange){
		RaycastHit _hit;
		Physics.Raycast (transform.position + new Vector3 (0, snapRange * 0.5f, 0), Vector3.down, out _hit, snapRange, mask_Floor);
		if (_hit.collider == null)
			print ("null collision at " + transform.position.ToString());
		if (_hit.collider.transform.eulerAngles.z == 0f)
			transform.position = new Vector3 (transform.position.x, _hit.collider.transform.position.y, transform.position.z);
		else
			transform.position = _hit.point;

	}

	//接收方向指令
	public void SetMove(bool _right, bool _left, bool _jogging){
		jogging = _jogging;
		anim.SetBool ("jog", jogging);
		Right = _right;
		Left = _left;
	}

	public void SetMove(bool _right, bool _left){
		Right = _right;
		Left = _left;
	}

	//接收上下方向指令
	public void SetStairMove(bool _up, bool _down){
		Up = _up;
		Down = _down;
	}
	//透過接收到的方向指令賦予走路函式目標速度
	protected void Move(bool _right, bool _left){
		if (!_right && !_left || _right && _left) {
			Walk (0);
		} else {
			if (_right)
				Walk (walkSpd);
			if (_left)
				Walk (-walkSpd);
		}
	}
	//使當下速度漸進到目標速度，並依此切換動畫和角色面相等等
	void Walk(float _spd){
		if (jogging && isNormalState)
			_spd *= 1.75f;
		if (insideBuilding != "HallWay")
			_spd *= 0.85f;

		#region nowSpd加速到目標_spd
		if(useAcc){
			if(isNormalState && !jogging){
				nowSpd = _spd;
			}else{
				if (Mathf.Abs(nowSpd - _spd) < 0.01f || _spd == 0 && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
					nowSpd = _spd;
				} else {
					float runSwitch = isNormalState? 0:1;
					float acc;
					if(nowSpd * _spd > -0.0001f){							//同號或一邊為0 : 異號
						if(Mathf.Abs(_spd) < 0.0001f)						//減速 : 加速
							acc = 7f;
						else
							acc = 10f + 6f * runSwitch;
					}else{
						acc = 15f + 10f * runSwitch;
					}

					if (nowSpd < _spd) {
						nowSpd += Time.deltaTime * acc;
						if (nowSpd > _spd)
							nowSpd = _spd;
					} else {
						nowSpd -= Time.deltaTime * acc;
						if (nowSpd < _spd)
							nowSpd = _spd;
					}
				}
				anim.SetFloat("speed", Mathf.Abs(nowSpd));
			}

		}else
			nowSpd = _spd;
        #endregion

		#region nowSpd不為0則進行牆壁和地板的判斷
		if (Mathf.Abs (nowSpd) > 0.0001f) {
			float originSpd = nowSpd * Time.deltaTime;
			int checkDirect;
			float checkLength = 1.5f * Mathf.Abs (originSpd);
			if (_spd == 0f)
				checkDirect = face;							//有按左或按右
			else
				checkDirect = (int)Mathf.Sign (nowSpd);		//沒有按按鍵，可能為滑動狀態或起步狀態

			RaycastHit checkStairs;
			if (Physics.Raycast(transform.position + 0.5f * Vector3.up, Vector3.down, out checkStairs, 1f, mask_Floor)){
				if(checkStairs.collider.tag == "Tag_Stairs"){
					if(_spd != 0)
						checkStairs.collider.GetComponent<sc_StairsSwitch>().DoStairsSwitch(this, _spd);
					else
						checkStairs.collider.GetComponent<sc_StairsSwitch>().DoStairsSwitch(this, nowSpd);
				}
					
			}
			RaycastHit checkWall;
			if (!Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), checkDirect * Vector3.right, out checkWall, checkLength+halfBodyWidth, mask_Wall)) {
				//no wall;
				RaycastHit checkFloor;
				float heightLength = Mathf.Clamp(checkLength * 1.5f, 0.1f, 1f);
				if (Physics.Raycast(transform.position + new Vector3(checkDirect * checkLength, heightLength, 0), Vector3.down,
					out checkFloor, 2f*heightLength, mask_Floor)) {
					//have floor
					//Debug.DrawLine(transform.position + new Vector3(checkDirect * checkLength, heightLength, 0), checkFloor.point);
					float floorAngle = checkFloor.collider.transform.eulerAngles.z;
					if(onSlope == (floorAngle == 0)){
						onSlope = !onSlope;
						transform.position = checkFloor.point;
					}else{
						if(floorAngle == 0)
							transform.Translate (originSpd, 0, 0);
						else
							transform.position = transform.position = 
								Vector3.Lerp (transform.position, checkFloor.point, Mathf.Abs(Mathf.Cos(floorAngle / 180f * Mathf.PI)) * 0.7f);
					}
				} else {
					//no floor
					//Debug.DrawLine(transform.position + new Vector3(checkDirect * checkLength, heightLength, 0), checkFloor.point);
					nowSpd = 0;
				}
			}else {
				//have wall
				transform.position = new Vector3 (checkWall.point.x - checkDirect * halfBodyWidth, transform.position.y, transform.position.z);
				SnapFloor(2);
				nowSpd = 0;
			}
		}
		#endregion

		#region 設定走路動畫
		if ((_spd > 0.1f || _spd < 0.1f) && face * _spd > 0f)
			anim.SetBool ("walk", true);
		else
			anim.SetBool ("walk", false);

		if(Mathf.Abs(nowSpd) > 0.01f)
			MoveActivate(true, false);
		else
			MoveActivate(false, false);
		
		#endregion

		#region 設定角色面向方向
		if (nowSpd > 0) {
			if (spr_shadow != null)
				spr_shadow.flipX = false;
			if (spr.flipX == true) {
				anim.SetTrigger ("turn");
				spr.flipX = false;
			}
			face = 1;
		} else if (nowSpd < 0) {
			if (spr_shadow != null)
				spr_shadow.flipX = true;
			if (spr.flipX == false) {
				anim.SetTrigger ("turn");
				spr.flipX = true;
			}
			face = -1;
		}
		#endregion
	}

	public void MoveActivate(bool _activate, bool _force){
		if (_activate) {
			if (walkSound != null && !walkSound.isPlaying)
				walkSound.Play ();
		} else {
			nowSpd = 0;
			if (walkSound != null && walkSound.isPlaying)
				walkSound.Stop();
			if (_force) {
				anim.SetBool ("walk", false);
				SetMove (false, false);
			}
			
		}

	}

	#endregion

	#region 圖層排序
	public void SetSortingOrder(int _num, bool relative){
		if (relative) {
			spr.sortingOrder += _num;
			if(spr_shadow != null)
				spr_shadow.sortingOrder += _num;
		} else {
			spr.sortingOrder = _num;
			if (spr_shadow != null)
				spr_shadow.sortingOrder = _num / 10 * 10 + 1;
		}

	}
	public int GetOriginSortOrder(){ return originSortOrder; }
	public int GetNowSortOrder(){ return spr.sortingOrder; }
	#endregion

	#region 動畫相關
	public void SetTalkAnim(string _animKey){
		if(!(_animKey == "M0" && lastAnimKey == "M0"))
			SetAnim (_animKey);
		lastAnimKey = _animKey;
  	}

	public void SetAnim(string _animKey){
		anim.SetTrigger(_animKey);
		//KM: No move anim. There is NM_frontIdle, NM_backIdle, NM_interact0, etc.
		//PK: Parkour anim. Don't change spr flip
		string keyTitle = _animKey.Split (new char[]{ '_' }) [0];
		if (keyTitle == "NM" || keyTitle == "PK" || _animKey == "M0") {
			canMove = false;
			MoveActivate (false, false);
			if (keyTitle != "PK") {
				spr.flipX = false;
				if (spr_shadow != null)
					spr_shadow.flipX = false;
			}

		}else if (_animKey == "idle") {
			anim.ResetTrigger ("turn");
			canMove = true;
			if (face == 1)
				spr.flipX = false;
			else
				spr.flipX = true;
		}
	}

	public void SetNormalState(bool isNormal, float _walkSpd){
		nowSpd = 0;
		isNormalState = isNormal;
		walkSpd = _walkSpd;
		anim.SetBool ("normalState", isNormal);
	}
	#endregion

	#region 透明度相關
	public void EnableSprite(bool _enable){
		spr.enabled = _enable;
		if (spr_shadow != null)
			spr_shadow.enabled = _enable;
	}

	public IEnumerator FadeSprite(float targetAlpha, float _t){
		float shadow_a = targetAlpha * shadowAlpha;
		if(spr_shadow != null)
			spr_shadow.DOFade (shadow_a, _t).SetEase (Ease.Linear);

		Tween nowTween = spr.DOFade (targetAlpha, _t).SetEase (Ease.Linear);
		yield return nowTween.WaitForCompletion ();
	}
	public void SetAlpha(float targetAlpha){
		Color c_tmp0 = spr.color;
		c_tmp0.a = targetAlpha;
		spr.color = c_tmp0;
		if (spr_shadow != null) {
			Color c_tmp1 = spr_shadow.color;
			c_tmp1.a = targetAlpha * shadowAlpha;
			spr_shadow.color = c_tmp1;
		}

  	}
	#endregion

	public void ScaleCharacter(float scaleRate){
		//float targetScale = transform.localScale.x * scaleRate;
		//transform.localScale = targetScale * Vector2.one;
		transform.localScale = scaleRate * transform.localScale;
		if (spr_shadow != null)
			transform.GetChild (0).localScale = scaleRate * transform.GetChild (0).localScale;
			
	}


}
