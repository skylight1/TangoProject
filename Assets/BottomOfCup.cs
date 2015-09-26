using UnityEngine;
using System.Collections;

public class BottomOfCup : MonoBehaviour {

	void Start () {
	
	}
	
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		Debug.Log ("SCORE!!!!!");
		other.gameObject.SetActive (false);
	}
}
