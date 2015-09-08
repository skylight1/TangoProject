using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimothyTest : MonoBehaviour {

	public class DistanceAndPoint {
		public Vector3 point;
		public double distance;

		public DistanceAndPoint(double aDistance, Vector3 aPoint) {
			this.point = aPoint;
			this.distance = aDistance;
		}
	}

	public TangoPointCloud m_tangoPointCloud;
	public Camera m_camera;
	public GameObject m_bounceSurfacePrefab;
	public GameObject m_reticle;

	private bool m_onTarget;
	private Vector3 targetPosition;
	private Quaternion targetRotation;

	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void Update () {
		updateTargetPositionAndRotation ();
		if (Input.GetKeyDown (KeyCode.Space)) {
			createBounceSurface ();
		}
		
		for (var i = 0; i < Input.touchCount; ++i) {
			if (Input.GetTouch(i).phase == TouchPhase.Began) {
				createBounceSurface ();
			}
		}

		m_reticle.GetComponent<Renderer> ().material.color = m_onTarget ? Color.green : Color.red;
	}
	
	private void updateTargetPositionAndRotation() {
		
		Vector3 p1 = m_camera.transform.position;
		Vector3 p2 = m_camera.transform.position + m_camera.transform.forward;
		
		List<DistanceAndPoint> distancesAndPoints = new List<DistanceAndPoint>();
		
		for (int index = 0; index < m_tangoPointCloud.m_pointsCount; index++) {
			Vector3 p0 = m_tangoPointCloud.m_points[index];
			double distance = Vector3.Cross(p0 - p1, p0 - p2).magnitude;
			distancesAndPoints.Add(new DistanceAndPoint (distance, p0) );
		}
		
		distancesAndPoints.Sort (delegate(DistanceAndPoint o1, DistanceAndPoint o2) { 
			if (o2.distance > o1.distance)
				return -1;
			else if (o2.distance < o1.distance)
				return 1;
			else 
				return 0;
		});
		
		if (distancesAndPoints.Count == 0) {
			return;
		}

		m_onTarget = distancesAndPoints [2].distance <= 0.3;

		// if more than 30 cm away from center line, then no target
		if (!m_onTarget) {
			return;
		}

		// calculate the target geometry in case the user clicks on it
		Vector3 c1 = distancesAndPoints[0].point;
		Vector3 c2 = distancesAndPoints[1].point;
		Vector3 c3 = distancesAndPoints[2].point;
		
		Vector3 cNormal = Vector3.Cross (c1 - c3, c2 - c3);
		
		targetRotation = Quaternion.FromToRotation (Vector3.up, -cNormal);
		
		targetPosition = (c1 + c2 + c3) / 3;
	}
	
	private void createBounceSurface() {
		GameObject m_bounceSurface = (GameObject)Instantiate (m_bounceSurfacePrefab);
		m_bounceSurface.SetActive (true);
		
		m_bounceSurface.transform.rotation = targetRotation;
		
		m_bounceSurface.transform.position = targetPosition;
	}
}
