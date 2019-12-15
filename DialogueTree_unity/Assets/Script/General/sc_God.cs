using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public interface i_PlotFlag{
	void FlagAdd (string _key);
	void FlagRemove (string _key);
}

public class sc_God : MonoBehaviour {

	static public CamInfo MainCam;

	protected AsyncOperation AO;
	int loadState = 0;
	[SerializeField]
	Image progressBar;

	protected sc_DialogGod dialGod;
	[SerializeField]
	protected Image SceneFadeImage;
	[SerializeField]
	protected string nextScene = "";
	public enum State{
		開始, 

		/*阿凱N1Q1, 等電梯, 阿凱N2Q3, 阿凱N4, 阿凱N5Q6, 阿凱N7, 阿凱N71, 阿凱N8, 進實驗室, 阿凱NQ8, 阿凱NQ81, 開發電機, 阿凱N9, 阿凱N10, 
		阿寧N1, 阿寧N2, 阿凱N11D, ENDING, PURSUE, LevelEnd, */
		李哥N1N9, 李哥N10, 等電梯, 阿凱NQ10, 阿凱N112, 阿凱N134, 阿凱N15, 阿凱N151, 阿凱N16D, 進實驗室, 阿凱N17, 阿凱N18, 
		開發電機, 阿凱N19, 阿凱N20, 阿寧N1, 阿寧N2, 阿凱N22D, ENDING, PURSUE, LevelEnd,

		葉宜樺N1N7, 鄭緯N1N2, 鄭緯N3N4, 鄭緯N5N6, 鄭緯N7, 鄭緯N8N12, 何欣潔N1N2, 何欣潔N3N4, 何欣潔N5N6, 
		戴勇誠All, 蔣瑜涵All, 吳晉峰N2N3, 廖吉興N4, 董欣麗All, 陳漢辰All, 吳晉峰N5, 葉宜樺N84A, 

		結束
	};
	static public State StoryState;
	static public bool fastDial = false;
	static protected List<i_PlotFlag> SP_Listener = new List<i_PlotFlag> ();
	static protected List<string> PlotFlags = new List<string> ();

	void Awake () {
		dialGod = GetComponent<sc_DialogGod>();
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
		PlotFlags.Clear ();
		SP_Listener.Clear ();
		StoryState = State.開始;
	}

	protected virtual void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();
		if (Input.GetKeyDown (KeyCode.F12))
			fastDial = !fastDial;

		if (loadState == 1) {
			progressBar.fillAmount = AO.progress / 0.9f;
			if (AO.progress > 0.89f) {
				loadState = 2;
				StartCoroutine (IE_FadeOut ());
			}
		}
	}


	static public void RegisterListener(i_PlotFlag SPL){
		SP_Listener.Add (SPL);
	}

	static public void SetPlotFlag(string _key, bool _isAdd){
		if (_isAdd) {
			if (!PlotFlags.Contains (_key))
				PlotFlags.Add (_key);
			foreach (i_PlotFlag SPL in SP_Listener)
				SPL.FlagAdd (_key);
		} else {
			if (PlotFlags.Contains (_key))
				PlotFlags.Remove (_key);
			foreach (i_PlotFlag SPL in SP_Listener)
				SPL.FlagRemove (_key);
		}
	}

	static public bool ContainsSP(string _key){
		return PlotFlags.Contains (_key);
	}

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



	protected IEnumerator IE_WaitPlot(string _key){
		WaitForSeconds waitTime = new WaitForSeconds (0.1f);
		while (!PlotFlags.Contains (_key))
			yield return waitTime;
		yield break;
	}
	protected IEnumerator IE_WaitPlot(string[] _keys){
		WaitForSeconds waitTime = new WaitForSeconds (0.1f);
		bool _condition = false;
		do {
			yield return waitTime;
			foreach (string _key in _keys) {
				if (PlotFlags.Contains (_key)) {
					_condition = true;
					break;
				}
			}
		} while (!_condition);

		yield break;
	}

}

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
