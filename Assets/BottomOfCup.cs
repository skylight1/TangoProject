using UnityEngine;
using System.Collections;

public class BottomOfCup : MonoBehaviour {

	public GameObject m_scoreText;

	void Start () {
	
	}
	
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		StartCoroutine(ShowScoreMessage());

		other.gameObject.SetActive (false);
	}

	IEnumerator ShowScoreMessage() {
		m_scoreText.SetActive (true);
		yield return new WaitForSeconds (2);
		m_scoreText.SetActive (false);
	}
}
