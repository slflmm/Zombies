using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	private float startTime;
	private static float BASE_TIME = 38f;

	private bool lost;
	private bool win;
	private bool noTime;

	public Transform pickUp;
	public Vector3[] pickUpCoords = new Vector3[] {
		new Vector3(0,0,10), new Vector3(0,0,18), new Vector3(0,0,26), new Vector3(6,0,14), new Vector3(6,0,22), new Vector3(8,0,8), new Vector3(8,0,28), new Vector3(12,0,8), new Vector3(12,0,28), new Vector3(14,0,14), new Vector3(14,0,22), new Vector3(20,0,10), new Vector3(20,0,18), new Vector3(20,0,26)
	};

	// Use this for initialization
	void Start () {
		lost = false;
		win = false;
		noTime = false;
		Time.timeScale = 1.0f;

		// place the pick up objects
		for (int i = 0; i < pickUpCoords.Length; i++) {
			Transform c = Instantiate(pickUp, pickUpCoords[i]*-100, Quaternion.identity) as Transform;
			c.localScale *= 400;
		}
		startTime = Time.time;
	}

	void Update() {
		if (Time.time - startTime > BASE_TIME*3) {
			OutOfTime();
		}
	}

	public void Win() {
		Time.timeScale = 0;
		win = true;
		Debug.Log (Time.time - startTime);
	}

	public void GameOver() {
		lost = true;
		Time.timeScale = 0;
	}

	public void OutOfTime () {
		noTime = true;
		Time.timeScale = 0;
	}

	void OnTriggerEnter(Collider other) {
		// check that all pickup objects have been collected
//		if (GameObject.FindGameObjectsWithTag("pickupable").Length == 0) Win ();
		Win ();
	}

	void OnGUI() {
		if (lost) {
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			// Now you can finally put in your GUI, such as:
			GUILayout.BeginVertical();
			GUILayout.Box("DETECTED!\n (The zombies maul you)");
			if (GUILayout.Button("Restart")){
				Time.timeScale = 1.0f;
				Application.LoadLevel(0);
			} 
			GUILayout.EndVertical();
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndArea();
		}
		if (win) {
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			// Now you can finally put in your GUI, such as:
			GUILayout.BeginVertical();
			GUILayout.Box("Survivor survives another day!");
			if (GUILayout.Button("Restart")){
				Time.timeScale = 1.0f;
				Application.LoadLevel(0);
			} 
			GUILayout.EndVertical();
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndArea();
		}
		if (noTime) {
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			// Now you can finally put in your GUI, such as:
			GUILayout.BeginVertical();
			GUILayout.Box("Out of time!");
			if (GUILayout.Button("Restart")){
				Time.timeScale = 1.0f;
				Application.LoadLevel(0);
			} 
			GUILayout.EndVertical();
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndArea();
		}
	}
}
