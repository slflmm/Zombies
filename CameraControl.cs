using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public float xSpeed;
	public float ySpeed;
	public float zoomSpeed;


	// Update is called once per frame
	void Update () {

		float zoomAmount = Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
		if (GetComponent<Camera>().orthographicSize - zoomAmount > 0.3 && GetComponent<Camera>().orthographicSize - zoomAmount <= 1150)
			GetComponent<Camera>().orthographicSize -= zoomAmount;

		float height = GetComponent<Camera>().orthographicSize * 2.0f;
		float width = height * Screen.width / Screen.height;

		float minX = -(3750 - width/2.0f);
		float maxX = -(-150 + width / 2.0f);

		float minZ = -(2150 - height / 2.0f);
		float maxZ = -(-150 + height / 2.0f);

		float xAmount = Input.GetAxis ("Horizontal") * xSpeed * Time.deltaTime;
		float zAmount = Input.GetAxis ("Vertical") * ySpeed * Time.deltaTime;

//		transform.Translate (new Vector3(xAmount, zAmount,0));
//		Vector3 pos = transform.position;
//		pos.z = Mathf.Clamp (transform.position.z, minX, maxX);
//		pos.x = Mathf.Clamp (transform.position.x, minZ, maxZ);
//		transform.position = pos;

	}
}
