using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class sc_BoxOutline : MonoBehaviour {
	const float UI2VectorRate = 0.02857f;
	const float outlineMargin = 0.075f;
	const float line2point = 0.1f;
	const float lineScaleRate = 0.3906f;
	const float jumpMargin = 0.1f;
	Transform[] tr_Dots = new Transform[4];
	Transform[] tr_Lines = new Transform[5];
	SpriteRenderer[] spr_Dots = new SpriteRenderer[4];
	SpriteRenderer[] spr_Lines = new SpriteRenderer[5];
	Color c_bright = new Color(1, 1, 1, 1);
	Color c_dark = new Color(1, 1, 1, 0);
	Vector2 v_lineOrigin = new Vector2(0, 0.5f);
	float[] x_lines = new float[]{ 0.36f, 0.5f, 2.05f, 0.5f, 2.05f };
	Vector2[] tmpDotPos = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
	Vector2[] tmpLinePos = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };

	void Awake (){
		OutlineInit ();
		OutlineReset ();
	}

	void OutlineInit(){
		for (int i = 0; i < 9; i++) {
			if (i < 4) {
				tr_Dots [i] = transform.GetChild (i);
				spr_Dots [i] = tr_Dots [i].GetComponent<SpriteRenderer> ();
			} else {
				tr_Lines [i - 4] = transform.GetChild (i);
				spr_Lines [i - 4] = tr_Lines [i - 4].GetComponent<SpriteRenderer> ();
			}

		}

	}

	void OutlineReset(){
		for (int i = 0; i < 4; i++) {
			spr_Dots [i].color = c_dark;
		}
		for (int i = 0; i < 5; i++) {
			tr_Lines [i].localScale = v_lineOrigin;
			spr_Lines [i].color = c_bright;
		}

	}

	void ComputePos(float _width, float _height){
		x_lines [0] = 0.36f;
		x_lines [1] = x_lines [3] = (_height * UI2VectorRate + 2f * outlineMargin - 2f * line2point) * lineScaleRate;
		x_lines [2] = x_lines [4] = (_width * UI2VectorRate + 2f * outlineMargin - 2f * line2point) * lineScaleRate;

		float dist = line2point + x_lines [0] / lineScaleRate;
		tr_Lines [0].localPosition = new Vector2 (-outlineMargin - Mathf.Abs (dist * Mathf.Cos (tr_Lines [0].localRotation.eulerAngles.z / 180f * Mathf.PI))
			, -outlineMargin - Mathf.Abs (dist * Mathf.Sin (tr_Lines [0].localRotation.eulerAngles.z / 180f * Mathf.PI)));				//斜線

		tmpDotPos [0] = new Vector2 (-0.8f * outlineMargin, -0.9f * outlineMargin);														//左下點
		tmpDotPos [1] = new Vector2 (-0.8f * outlineMargin, _height * UI2VectorRate + 0.9f * outlineMargin);							//左上點
		tmpDotPos [2] = new Vector2 (_width * UI2VectorRate + 0.8f * outlineMargin, _height * UI2VectorRate + 0.9f * outlineMargin);	//右上點
		tmpDotPos [3] = new Vector2 (_width * UI2VectorRate + 0.8f * outlineMargin, -0.9f * outlineMargin);								//右下點

		tmpLinePos [0] = new Vector2 (-outlineMargin, -outlineMargin + line2point);														//左線
		tmpLinePos [1] = new Vector2 (-outlineMargin + line2point, -outlineMargin);														//下線
		tmpLinePos [2] = new Vector2 (_width * UI2VectorRate + outlineMargin, _height * UI2VectorRate + outlineMargin - line2point);	//右線
		tmpLinePos [3] = new Vector2 (_width * UI2VectorRate + outlineMargin - line2point, _height * UI2VectorRate + outlineMargin);	//上線


	}

	void TweenOutlineColor(float _alpha, float _t){
		if (_t <= 0.01f) {
			for (int i = 0; i < 4; i++)
				spr_Dots [i].color = new Color (1, 1, 1, _alpha);
			for (int i = 0; i < 5; i++)
				spr_Lines [i].color = new Color (1, 1, 1, _alpha);
		} else {
			for (int i = 0; i < 4; i++)
				spr_Dots [i].DOFade (_alpha, _t);
			for (int i = 0; i < 5; i++)
				spr_Lines [i].DOFade (_alpha, _t);
		}

	}

	public void OpenOutline(sc_NpcDialog.animType _type, float _width, float _height){
		if (tr_Dots [0] == null)
			OutlineInit ();

		ComputePos (_width, _height);
		switch (_type) {
		case sc_NpcDialog.animType.Start:
			OutlineReset ();
			for (int i = 0; i < 4; i++) {
				tr_Dots [i].localPosition = tmpDotPos [i];
				tr_Lines [i + 1].localPosition = tmpLinePos [i];
			}
			TweenOutlineColor (1f, 0);
			Sequence seq0 = DOTween.Sequence ();
			seq0.Append (tr_Lines [0].DOScaleX (x_lines [0], 0.35f))
				.Append (tr_Lines [1].DOScaleX (x_lines [1], 0.45f)).Join (tr_Lines [2].DOScaleX (x_lines [2], 0.45f))
				.Join (tr_Lines [3].DOScaleX (x_lines [3], 0.45f)).Join (tr_Lines [4].DOScaleX (x_lines [4], 0.45f)).SetEase (Ease.OutCubic);
			for (int i = 0; i < 4; i++)
				seq0.Insert (0.55f, spr_Dots [i].DOColor (c_bright, 0.3f).SetEase(Ease.Linear));
			break;

		case sc_NpcDialog.animType.End:
			Sequence seq1 = DOTween.Sequence ();
			for (int i = 0; i < 4; i++)
				seq1.Join (spr_Dots [i].DOColor (c_dark, 0.3f).SetEase(Ease.Linear));
			seq1.Join (tr_Lines [1].DOScaleX (0f, 0.35f)).Join (tr_Lines [2].DOScaleX (0f, 0.35f))
				.Join (tr_Lines [3].DOScaleX (0f, 0.35f)).Join (tr_Lines [4].DOScaleX (0f, 0.35f))
				.Append (tr_Lines [0].DOScaleX (0f, 0.3f)).SetEase (Ease.InOutCubic);
			break;

		case sc_NpcDialog.animType.Update:
			tr_Dots [0].localPosition = tmpDotPos [0];
			tr_Lines [1].localPosition = tmpLinePos [0];
			tr_Lines [2].localPosition = tmpLinePos [1];
			Sequence seq2 = DOTween.Sequence ();
			seq2.Append (tr_Lines [1].DOScaleX (x_lines [1], 0.2f)).Join (tr_Lines [2].DOScaleX (x_lines [2], 0.2f))
				.Join (tr_Lines [3].DOScaleX (x_lines [3], 0.2f)).Join (tr_Lines [4].DOScaleX (x_lines [4], 0.2f))
				.Join(tr_Dots[1].DOLocalMove(tmpDotPos[1], 0.2f)).Join(tr_Dots[2].DOLocalMove(tmpDotPos[2], 0.2f)).Join(tr_Dots[3].DOLocalMove(tmpDotPos[3], 0.2f))
				.Join(tr_Lines[3].DOLocalMove(tmpLinePos[2], 0.2f)).Join(tr_Lines[4].DOLocalMove(tmpLinePos[3], 0.2f));
			TweenOutlineColor (1f, 0.15f);
			break;

		case sc_NpcDialog.animType.Rest:
			tr_Dots [0].localPosition = tmpDotPos [0];
			tr_Lines [1].localPosition = tmpLinePos [0];
			tr_Lines [2].localPosition = tmpLinePos [1];
			Sequence seq3 = DOTween.Sequence ();
			seq3.Append (tr_Lines [1].DOScaleX (x_lines [1], 0.3f)).Join (tr_Lines [2].DOScaleX (x_lines [2], 0.3f))
				.Join (tr_Lines [3].DOScaleX (x_lines [3], 0.3f)).Join (tr_Lines [4].DOScaleX (x_lines [4], 0.3f))
				.Join (tr_Dots [1].DOLocalMove (tmpDotPos [1], 0.3f)).Join (tr_Dots [2].DOLocalMove (tmpDotPos [2], 0.3f)).Join (tr_Dots [3].DOLocalMove (tmpDotPos [3], 0.3f))
				.Join (tr_Lines [3].DOLocalMove (tmpLinePos [2], 0.3f)).Join (tr_Lines [4].DOLocalMove (tmpLinePos [3], 0.3f));
			TweenOutlineColor (0.5f, 0.3f);
			break;

		}

	}

	//jump
	public void OpenOutline(float pre_width, float pre_height, float now_width, float now_height, float _lerpTime){
		float midX = pre_width > now_width ? pre_width : now_width;
		float midY = pre_height > now_height ? pre_height : now_height;
		ComputePos (midX, midY);
		tmpDotPos [0] += new Vector2 (-jumpMargin, -jumpMargin);
		tmpDotPos [1] += new Vector2 (-jumpMargin, jumpMargin);
		tmpDotPos [2] += new Vector2 (jumpMargin, jumpMargin);
		tmpDotPos [3] += new Vector2 (jumpMargin, -jumpMargin);
		tmpLinePos [0] += new Vector2 (-jumpMargin, 0);
		tmpLinePos [1] += new Vector2 (0, -jumpMargin);
		tmpLinePos [2] += new Vector2 (jumpMargin, 0);
		tmpLinePos [3] += new Vector2 (0, jumpMargin);
		x_lines [0] = 0.3f;
		TweenOutlineColor (1f, 0.15f);
		Sequence seq = DOTween.Sequence ();
		seq.Append (tr_Lines [1].DOScaleX (x_lines [1], _lerpTime)).Join (tr_Lines [1].DOScaleX (x_lines [1], _lerpTime)).Join (tr_Lines [2].DOScaleX (x_lines [2], _lerpTime))
			.Join (tr_Lines [3].DOScaleX (x_lines [3], _lerpTime)).Join (tr_Lines [4].DOScaleX (x_lines [4], _lerpTime));
		seq.Join(tr_Dots [0].DOLocalMove (tmpDotPos [0], _lerpTime)).Join (tr_Dots [1].DOLocalMove (tmpDotPos [1], _lerpTime))
			.Join (tr_Dots [2].DOLocalMove (tmpDotPos [2], _lerpTime)).Join (tr_Dots [3].DOLocalMove (tmpDotPos [3], _lerpTime));
		seq.Join(tr_Lines[1].DOLocalMove(tmpLinePos[0], _lerpTime)).Join(tr_Lines[2].DOLocalMove(tmpLinePos[1], _lerpTime))
			.Join(tr_Lines[3].DOLocalMove(tmpLinePos[2], _lerpTime)).Join(tr_Lines[4].DOLocalMove(tmpLinePos[3], _lerpTime));
		ComputePos (now_width, now_height);
		seq.Append (tr_Lines [1].DOScaleX (x_lines [1], 0.16f)).Join (tr_Lines [1].DOScaleX (x_lines [1], 0.16f)).Join (tr_Lines [2].DOScaleX (x_lines [2], 0.16f))
			.Join (tr_Lines [3].DOScaleX (x_lines [3], 0.16f)).Join (tr_Lines [4].DOScaleX (x_lines [4], 0.16f));
		seq.Join(tr_Dots [0].DOLocalMove (tmpDotPos [0], 0.16f)).Join (tr_Dots [1].DOLocalMove (tmpDotPos [1], 0.16f))
			.Join (tr_Dots [2].DOLocalMove (tmpDotPos [2], 0.16f)).Join (tr_Dots [3].DOLocalMove (tmpDotPos [3], 0.16f));
		seq.Join(tr_Lines[1].DOLocalMove(tmpLinePos[0], 0.16f)).Join(tr_Lines[2].DOLocalMove(tmpLinePos[1], 0.16f))
			.Join(tr_Lines[3].DOLocalMove(tmpLinePos[2], 0.16f)).Join(tr_Lines[4].DOLocalMove(tmpLinePos[3], 0.16f));


	}
}
