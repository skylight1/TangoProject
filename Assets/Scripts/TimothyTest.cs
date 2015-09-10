using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimothyTest : MonoBehaviour {

	/////
	bool DEBUG = true;
	/////

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

	/////
	int nupdates = 0;
	/////

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
		/////
		Vector3 p1forward = m_camera.transform.forward;
		/////
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

		///////////
		if (DEBUG) {
			if (++nupdates == 100 | nupdates % 500 == 0) {
				for (int i=0; i < distancesAndPoints.Count; i++) {
					Debug.Log ("TangoUpdate ***** point number ," + i + 
					           "," + distancesAndPoints [i].point.x +
					           "," + distancesAndPoints [i].point.y +
					           "," + distancesAndPoints [i].point.z +
//					           "," + distancesAndPoints [i].angle +
					           "," + distancesAndPoints [i].distance);
				}
			}
			;
			
			/*
			Debug.Log ("TangoUpdate number of points " + distancesAndPoints.Count);
			Debug.Log ("TangoUpdate min distance " + distancesAndPoints [0].distance);
			Debug.Log ("TangoUpdate max distance " + distancesAndPoints [distancesAndPoints.Count - 1].distance);
			
			Debug.Log ("TangoUpdate center " + centerPoint.x + ", " + centerPoint.y + ", " + centerPoint.z);
			Debug.Log ("TangoUpdate target " + targetPoint.x + ", " + targetPoint.y + ", " + targetPoint.z);
			Debug.Log ("TangoUpdate point 1 " + c1.x + ", " + c1.y + ", " + c1.z);
			Debug.Log ("TangoUpdate point 2 " + c2.x + ", " + c2.y + ", " + c2.z);
			Debug.Log ("TangoUpdate point 3 " + c3.x + ", " + c3.y + ", " + c3.z);
			Debug.Log ("TangoUpdate camera " + p1.x + ", " + p1.y + ", " + p1.z);
			Debug.Log ("TangoUpdate view direction " + p1forward.x + ", " + p1forward.y + ", " + p1forward.z);
			Debug.Log ("TangoUpdate cNormal " + cNormal.x + ", " + cNormal.y + ", " + cNormal.z);
			*/
		}
		double[] pcaPlane = fitNearestSurface (distancesAndPoints, 30);
		Vector3 pcaPlanev3 = new Vector3 ((float) pcaPlane [0], (float) pcaPlane [1], (float) pcaPlane [2]);
		
		if (DEBUG) Debug.Log ("TangoUpdate pcaPlane " + pcaPlanev3.x + ", " + pcaPlanev3.y + ", " + pcaPlanev3.z);

		// replace -cNormal plane with the pcaPlane
		
		//		Vector3 cNormal = Vector3.Cross (c1 - c3, c2 - c3);
//		m_bounceSurface.transform.rotation = Quaternion.FromToRotation (Vector3.up, -pcaPlanev3);

		//////////

			
		targetRotation = Quaternion.FromToRotation (Vector3.up, -cNormal);
		
		targetPosition = (c1 + c2 + c3) / 3;
	}
	
	private void createBounceSurface() {
		GameObject m_bounceSurface = (GameObject)Instantiate (m_bounceSurfacePrefab);
		m_bounceSurface.SetActive (true);
		
		m_bounceSurface.transform.rotation = targetRotation;
		
		m_bounceSurface.transform.position = targetPosition;
	}


	/////////////////////////////////////
	private Vector3 V3mult(Vector3 v1, Vector3 v2) {
		Vector3 vmult = new Vector3 (v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		return vmult;
	}
	
	private Vector3 V3sqrt(Vector3 v1) {
		Vector3 vsqrt = new Vector3 (Mathf.Sqrt (v1.x), Mathf.Sqrt (v1.y), Mathf.Sqrt (v1.z));
		return vsqrt;
	}
	
	private double[] fitNearestSurface(List<DistanceAndPoint> distancesAndPoints, int n) {
		if (DEBUG) Debug.Log ("TangoUpdate pca , for " + n + " points");
		MatrixStats mc = getCovar (distancesAndPoints, n);
		
		OnePC pc1 = powerMethod(mc.covar);
		
		if (DEBUG) Debug.Log ("TangoUpdate pca1 " + pc1.pc[0] + ", " + pc1.pc[1] + ", " + pc1.pc[2]);
		
		double[,] covar2 = new double[3,3];
		for (int i=0; i < 3; i++) {
			for (int j=0; j < 3; j++) {
				covar2[i,j] = mc.covar[i,j] - ( pc1.shrink * pc1.pc[i] * pc1.pc[j] );
			}
		}
		
		OnePC pc2 = powerMethod(covar2);
		
		if (DEBUG) Debug.Log ("TangoUpdate pca2 " + pc2.pc[0] + ", " + pc2.pc[1] + ", " + pc2.pc[2]);
		
		// 	c(x=y1 * z2 - z1 * y2, y=z1 * x2 - x1 * z2, z=x1 * y2 - y1 * x2)
		double[] planeCoefficients = {
			pc1.pc [1] * pc2.pc [2] - pc1.pc [2] * pc2.pc [1],
			pc1.pc [2] * pc2.pc [0] - pc1.pc [0] * pc2.pc [2],
			pc1.pc [0] * pc2.pc [1] - pc1.pc [1] * pc2.pc [0],
			0.0f
		};
		
		planeCoefficients[3] = planeCoefficients[0] * mc.mean[0] + planeCoefficients[1] * mc.mean[1] + planeCoefficients[2] * mc.mean[2];
		
		return planeCoefficients;
	}
	
	private MatrixStats getCovar(List<DistanceAndPoint> distancesAndPoints, int n) {
		if (DEBUG) Debug.Log ("TangoUpdate pca start getCovar");
		
		Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);
		for (int i=0; i < n; i++) {
			sum += distancesAndPoints[i].point;
		}
		
		if (DEBUG) Debug.Log ("TangoUpdate pca vector sum " + sum.x + ", " + sum.y + ", " + sum.z);
		
		double[] mean = {sum.x / n, sum.y / n, sum.z / n};
		
		if (DEBUG) Debug.Log ("TangoUpdate pca vector mean " + mean[0] + ", " + mean[1] + ", " + mean[2]);
		
		double[,] covar = {{0,0,0}, {0,0,0}, {0,0,0}};
		
		if (DEBUG) Debug.Log ("TangoUpdate pca initial, " + covar[0,0] + "," + covar[0,1] + "," + covar[1,2]);
		
		for (int i=0; i < n; i++) {
			covar[0,0] += (distancesAndPoints[i].point.x - mean[0]) * (distancesAndPoints[i].point.x - mean[0]);
			covar[0,1] += (distancesAndPoints[i].point.x - mean[0]) * (distancesAndPoints[i].point.y - mean[1]);
			covar[0,2] += (distancesAndPoints[i].point.x - mean[0]) * (distancesAndPoints[i].point.z - mean[2]);
			covar[1,1] += (distancesAndPoints[i].point.y - mean[1]) * (distancesAndPoints[i].point.y - mean[1]);
			covar[1,2] += (distancesAndPoints[i].point.y - mean[1]) * (distancesAndPoints[i].point.z - mean[2]);
			covar[2,2] += (distancesAndPoints[i].point.z - mean[2]) * (distancesAndPoints[i].point.z - mean[2]);
		}
		covar [1, 0] = covar [0, 1];
		covar [2, 0] = covar [0, 2];
		covar [2, 1] = covar [1, 2];
		
		if (DEBUG) Debug.Log ("TangoUpdate pca , " + covar[0,0] + "," + covar[0,1] + "," + covar[1,2]);
		
		for (int i=0; i < 3; i++) {
			for (int j=0; j < 3; j++) {
				covar [i, j] /= (n - 1);
				if (DEBUG) Debug.Log ("TangoUpdate pca , " + i + "," + j + "," + covar[i,j]);
			}
		}
		
		
		MatrixStats mstats = new MatrixStats (mean, covar);
		
		return mstats;
	}
	
	private OnePC powerMethod(double[,] covar) {
		
		double sqr3 = 1.0f / Mathf.Sqrt (3.0f);
		double[] pc1 = {sqr3, sqr3, sqr3};
		double shrink = 0.0;
		for (int k = 0; k < 20; k++) {
			double[] pc1a = {0.0f, 0.0f, 0.0f};
			for (int i=0; i < 3; i++) {
				for (int j=0; j < 3; j++) {
					pc1a[i] += covar[i,j] * pc1[j];
				}
			}
			double shrink2 = pc1a[0] * pc1a[0] + pc1a[1] * pc1a[1] + pc1a[2] * pc1a[2];
			float shrink2f = (float) shrink2;
			shrink = Mathf.Sqrt(shrink2f);
			for (int i=0; i < 3; i++) {
				pc1[i] = pc1a[i] / shrink;
			}
		}
		OnePC pc = new OnePC (pc1, shrink);
		return pc;
	}
	
	public class MatrixStats {
		public double[] mean;
		public double[,] covar;
		
		public MatrixStats(double[] mean, double[,] covar) {
			this.mean = mean;
			this.covar = covar;
		}
	}
	
	public class OnePC {
		public double[] pc;
		public double shrink;
		
		public OnePC(double[] pc, double shrink) {
			this.pc = pc;
			this.shrink = shrink;
		}
	}
	//////////////////////////////////////////
}
