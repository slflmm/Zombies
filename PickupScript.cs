using UnityEngine;
using System.Collections;

public class PickupScript : MonoBehaviour {

	// When the pickup object is touched, it will vanish
	void OnTriggerEnter(Collider other) {
		Destroy (gameObject);
	}
}
