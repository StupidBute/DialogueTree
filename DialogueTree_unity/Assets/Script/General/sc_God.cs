using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CamInfo{
	public Camera cam;
	public sc_CamFollow scCam;
	public Transform camTR;
	public Animator camAnim;

	public CamInfo(Camera _cam, sc_CamFollow _scCam, Transform _camTR, Animator _camAnim){
		cam = _cam;
		scCam = _scCam;
		camTR = _camTR;
		camAnim = _camAnim;
	}
}

public class sc_God : MonoBehaviour {

	static public CamInfo MainCam;

	protected AsyncOperation AO;
	int loadState = 0;
	[SerializeField]
	Image progressBar;

	[SerializeField]
	protected Image SceneFadeImage;
	[SerializeField]
	protected string nextScene = "";

	void Awake () {
		Camera cam = Camera.main;
		Transform camTR;
		sc_CamFollow scCam;
		Animator camAnim;
		if (cam.transform.parent != null) {
			camTR = cam.transform.parent;
			scCam = cam.transform.parent.GetComponent<sc_CamFollow> ();
			camAnim = cam.transform.parent.GetComponent<Animator> ();
		} else {
			camTR = cam.transform;
			scCam = cam.GetComponent<sc_CamFollow> ();
			camAnim = null;
		}
		MainCam = new CamInfo (cam, scCam, camTR, camAnim);
		ChangeScene (true, 2f, 0);
	}

	protected virtual void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();

		if (loadState == 1) {
			progressBar.fillAmount = AO.progress / 0.9f;
			if (AO.progress > 0.89f) {
				loadState = 2;
				StartCoroutine (IE_FadeOut ());
			}
		}
	}
	#region 換場相關
	protected void ChangeScene(bool _fadeInScene, float _fadeTime, float _changeTime){
		if (_fadeInScene) {
			SceneFadeImage.color = new Color (0, 0, 0, 1);
			SceneFadeImage.DOFade (0, _fadeTime);
		}else{
			SceneFadeImage.color = Color.clear;
			SceneFadeImage.DOFade (1, _fadeTime);
			StartCoroutine (IE_ChangeScene (_changeTime));
		}
	}

	IEnumerator IE_ChangeScene(float _changeTime){
		if (_changeTime < 0.01f) {
			while (SceneFadeImage.color.a < 0.999f)
				yield return null;
		} else {
			yield return new WaitForSeconds (_changeTime);
		}
		StartLoadScene ();
	}

	void StartLoadScene(){
		if (loadState == 0) {
			AO = SceneManager.LoadSceneAsync (nextScene);
			AO.allowSceneActivation = false;
			loadState = 1;
		}

	}

	IEnumerator IE_FadeOut(){
		Tween nowTween = progressBar.DOFade (0f, 1f);
		yield return nowTween.WaitForCompletion ();
		AO.allowSceneActivation = true;
	}
	#endregion
}