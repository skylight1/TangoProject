using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	float speed = 4.0f; //TODO: adjust

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{

// how's it's usually done:
		/*
		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
		if(Camera.current != null)
		{
			Camera.current.transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));
		}
		*/

// use left/roght arrows to move camera left and right, up/down to move in/out z axis

		if(Input.GetKey(KeyCode.RightArrow))
		{
			Camera.current.transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			Camera.current.transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			Camera.current.transform.Translate(new Vector3(0, 0, -speed * Time.deltaTime));
		}
		if(Input.GetKey(KeyCode.UpArrow))
		{
			transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
		}

// TODO: figure out how to look left and right by rotating on Y axis (use mouse?)
/*		var r : float = speed * Input.GetAxis ("Mouse Y");
		transform.Rotate (r, 0, 0);
*/
	}
}
