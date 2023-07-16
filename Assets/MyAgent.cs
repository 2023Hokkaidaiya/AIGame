using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Agent
public class MyAgent : Agent {
	//アクション定数
	public const int ACTION_NONE = 0;
	public const int ACTION_BACKWARD = 1;
	public const int ACTION_ATTACK = 2;
	public const int ACTION_FORWARD = 3;

	//ゲーム定数
	public const float STAGE_SIZE = 3f; //ステージサイズ
	public const float ATTACK_DISTANCE = 1.5f; //攻撃距離
	public const float ATTACK_POWER = 10f; //攻撃力

	//参照
	MyAcademy academy; //Academy
	MyAgent[] agent = new MyAgent[2]; //Agent
	GameObject[] body = new GameObject[2]; //Body
	Animator[] animator = new Animator[2]; //Animator
	GameObject[] hpBar = new GameObject[2]; //HPバー

	//変数
	public int myNo; //自分のナンバー
	int enemyNo; //敵のナンバー
	float hp; //HP
	int action; //Action
	float waitTime; //待機時間
	int waitType; //待機種別

	//BrainへのStateの提供（ステップ毎に呼ばれる）
	public override void CollectObservations() {
		Vector3 myPos = this.body[this.myNo].transform.position;
		Vector3 enemyPos = this.body[this.enemyNo].transform.position;
		float distance = Mathf.Abs (myPos.x - enemyPos.x);
		AddVectorObs(distance / (STAGE_SIZE*2)); //自分と敵の距離
		AddVectorObs(this.waitTime >= 0 ? 1 : 0); //自分の待機
		AddVectorObs(this.waitTime < 0 && this.action == ACTION_ATTACK ? 1 : 0); //自分の攻撃
		AddVectorObs(this.waitTime < 0 && this.action == ACTION_FORWARD ? 1 : 0); //自分の前進
		AddVectorObs(this.waitTime < 0 && this.action == ACTION_BACKWARD ? 1 : 0); //自分の後進
	}

	//BrainからのAction取得（フレーム毎に呼ばれる）
	public override void AgentAction(float[] vectorAction, string textAction) {
		//アクションの取得
		this.action = (int)vectorAction [0];

		//キャラクターの位置と距離の計算
		Vector3 myPos = this.body[this.myNo].transform.position;
		Vector3 enemyPos = this.body[this.enemyNo].transform.position;
		float distance = Mathf.Abs (myPos.x - enemyPos.x);

		//待機時間
		if (this.waitTime >= 0f) {
			this.waitTime -= Time.deltaTime;

			//ダメージ後退
			if (this.animator[this.myNo].GetCurrentAnimatorStateInfo(0).nameHash == 
				    Animator.StringToHash ("Base Layer.Damage")) {
				myPos.x -= (this.myNo == 0) ? 0.05f : -0.05f;
			}
		}
		//通常操作
		else {
			//後進
			if (action == ACTION_BACKWARD) {
				this.animator[this.myNo].SetBool ("Backward", true);
				myPos.x -= (this.myNo == 0) ? 0.05f : -0.05f;
			} else {
				this.animator[this.myNo].SetBool ("Backward", false);
			}

			//攻撃
			if (action == ACTION_ATTACK) {
				this.animator[this.myNo].SetTrigger ("Attack");

				//攻撃がヒット
				if (distance < ATTACK_DISTANCE) {
					this.agent [this.enemyNo].Damage (); //ダメージ
					this.waitTime = 0.5f; //待機時間
					AddReward(0.1f); //報酬
				}
				//攻撃が空振り
				else {
					this.waitTime = 0.5f; //待機時間
					AddReward(-0.1f); //報酬
				}
			}

			//前進
			if (action == ACTION_FORWARD) {
				this.animator[this.myNo].SetBool ("Forward", true);
				myPos.x += (this.myNo == 0) ? 0.05f : -0.05f;
			} else {
				this.animator[this.myNo].SetBool ("Forward", false);
			}
        }

		//移動制限
		if (distance < 1f) {
			if (this.myNo == 0) myPos.x = enemyPos.x - 1f;
			if (this.myNo == 1) myPos.x = enemyPos.x + 1f;
		}
		if (myPos.x < -STAGE_SIZE) myPos.x = -STAGE_SIZE;
		if (myPos.x > STAGE_SIZE) myPos.x = STAGE_SIZE;
		myPos.z = 0;
		this.body[this.myNo].transform.position = myPos;

		//決着
		if (this.agent[0].hp <= 0 || this.agent[1].hp <= 0 || 
			this.academy.GetTime() <= 0) {
			//エピソード完了
			Done();
		}

		//報酬
	    AddReward(-0.001f);
	}

	//リセット時に呼ばれる
	public override void AgentReset() {
		//参照の準備
		academy = GameObject.Find("MyAcademy").GetComponent<MyAcademy> ();
		academy.AcademyReset ();
		for (int i = 0; i < 2; i++) {
			this.agent[i] = GameObject.Find("MyAgent"+i).GetComponent<MyAgent> ();
			this.body [i] = GameObject.Find("MyAgent"+i);
			this.animator[i] = body[i].GetComponent<Animator> ();
			this.hpBar[i] = GameObject.Find("HPBar"+i);
		}

		//キャラクターの初期化
		this.body [this.myNo].SetActive (false);
		this.body [this.myNo].SetActive (true);
		Vector3 position = this.body[this.myNo].transform.position;
		position.x = this.myNo == 0 ? -STAGE_SIZE/2 : STAGE_SIZE/2;
		position.z = 0;
		this.body[this.myNo].transform.position = position;
		this.animator[this.myNo].speed = 2f;

		//変数の初期化
		this.enemyNo = (this.myNo + 1) % 2;
		SetHP(100);
		this.action = -1;
		this.waitTime = -1f;
		this.waitType = -1;
	}

	//ダメージ
	public void Damage () {
		//アクションキャンセル
		this.body [this.myNo].SetActive (false);
		this.body [this.myNo].SetActive (true);

		//ダメージ後退
		this.animator[this.myNo].SetTrigger ("Damage");
		this.waitTime = 0.4f;

		//HP減
		SetHP (this.hp - ATTACK_POWER);
	}

	//HPの指定
	void SetHP(float hp) {
		this.hp = hp;
		this.hpBar[this.myNo].transform.localScale = 
			new Vector3(this.hp/100f, 1, 1);
	}
}