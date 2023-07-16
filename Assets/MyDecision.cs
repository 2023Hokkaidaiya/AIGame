using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Decision
public class MyDecision : MonoBehaviour, Decision {

	//Actionの決定
	public float[] Decide(List<float> vectorObs, List<Texture2D> visualObs,
		float reward, bool done, List<float> memory)	{
		//情報
		float distance = vectorObs[0]*MyAgent.STAGE_SIZE*2;
		int action = -1;
		if (vectorObs [2] == 1)	action = MyAgent.ACTION_ATTACK;
		if (vectorObs [3] == 1)	action = MyAgent.ACTION_FORWARD;
		if (vectorObs [4] == 1)	action = MyAgent.ACTION_BACKWARD;
		int r = Random.Range (0, 100);

		//近く
		if (distance < 2.0f) {
			if (r < 40) {
				action = MyAgent.ACTION_BACKWARD; //40%:後進
			} else if (r < 40 + 10) {
				action = MyAgent.ACTION_ATTACK; //10%:攻撃
			}
		} 
		//遠く
		else {
			if (r < 30) {
				action = MyAgent.ACTION_FORWARD; //30%:前進
			} else if (r < 30 + 10) {
				action = MyAgent.ACTION_ATTACK; //10%:攻撃
			}
		}
		return new float[1]{action};
	}

	//次のAction決定に利用するMemoryの生成
	public List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs,
		float reward, bool done, List<float> memory) {
		return new List<float>();
	}
}