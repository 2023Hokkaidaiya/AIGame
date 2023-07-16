using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Academy
public class MyAcademy : Academy {
	//タイマー
	Text myTimer;
	float time = 60f;

	//環境リセット時に呼ばれる
	public override void AcademyReset() {
		//タイマーのリセット
		this.time = 60f;
		this.myTimer = GameObject.Find ("MyTimer").GetComponent<Text> ();
		this.myTimer.text = ""+time;
	}

	//フレーム毎に呼ばれる
	void Update () {
		//タイマーのカウントダウン
		this.time -= Time.deltaTime;
		if (this.time < 0f) {
			this.time = 0f;
		}
		int second = (int)time;
		string text = second.ToString ("00");
		if (this.myTimer.text != text) {
			this.myTimer.text = text;
		}
	}

	//時間の取得
	public float GetTime() {
		return time;
	}
}