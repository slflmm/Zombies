using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhoneZombie : Zombie {

	private float stopTime = 2;
	private float startedStop;

	private float speedTime = 2;
	private float startedSpeed;

	new void Start() {
		
		base.Start ();
		
		speedModifier = 1f;
		
		stateHandlers = new Dictionary<State, StateHandlerDelegate>();
		stateHandlers [State.Walk] = Walk;
		stateHandlers [State.Stop] = Stop;
		stateHandlers [State.ChangeLanes] = ChangeLanes;
		stateHandlers [State.ChangeDirection] = ChangeDirection;
		
		currentState = State.Walk;
	}
	
	void Update() {

		if (Time.time - startedStop < stopTime) {
				}
						
		else {
		// avoid collisions with other zombies	
		Vector3 rayStart = transform.position + new Vector3 (0,25,0);
		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		Vector3 fwdright = fwd + transform.right * direction;
		Vector3 fwdleft = fwd + transform.right * direction * -1;
		Vector3 fwdright2 = fwd + 0.5f*transform.right * direction;
		Vector3 fwdleft2 = fwd + 0.5f * transform.right * direction * -1;
		Vector3[] directions = new Vector3[] { fwd, fwdright, fwdleft, fwdright2, fwdleft2};
		
		RaycastHit hit1 = new RaycastHit();
		RaycastHit hit2 = new RaycastHit();
		RaycastHit hit3 = new RaycastHit();
		RaycastHit hit4 = new RaycastHit ();
		RaycastHit hit5 = new RaycastHit ();
		RaycastHit[] hits = new RaycastHit[] { hit1, hit2, hit3, hit4, hit5 };
		
		bool gotHit = false;
		Zombie z;
		float hitDistance = 0;
		float zSpeedModifier = 0;
		
		for (int i = 0; i < 5; i++) {
			if (Physics.Raycast (rayStart, directions[i], out hits[i]) && hits[i].collider.GetComponent<Zombie>() != null && hits[i].distance <= 200) {
				z = hits[i].collider.GetComponent<Zombie>();
				zSpeedModifier = z.speedModifier;
				hitDistance = hits[i].distance;
				if (z.lane == lane) {
					gotHit = true;
					break;
				}
			}
		}

		if (gotHit) {
			float d = hitDistance;
			// if you can change lanes, do it
			if (canChangeLanes ()) {
				currentState = State.ChangeLanes;
			}	
			else if (canChangeDirection()) {
				currentState = State.ChangeDirection;
			}
			// otherwise resort to the usual behaviours
			else if (d <= 150) {
				currentState = State.Stop;
			}
			else if (d <= 200) {
				speedModifier = Mathf.Min(zSpeedModifier, regularSpeed);
				currentState = State.Walk;
			}
		}
		else {

			currentState = State.Walk;

			float v = Random.value;
			// sometimes change lanes
			if (v < 0.01 && canChangeLanes()) {
				currentState = State.ChangeLanes;
			}
			// occasionally change directions
			else if (v < 0.025 && canChangeDirection()) {
				currentState = State.ChangeDirection;
			}
			// frequently stop for several seconds
			else if (v < 0.035 && Time.time - startedStop > stopTime) {
				currentState = State.Stop;
				startedStop = Time.time;
			}
			else if (v < 0.060 && Time.time - startedSpeed > speedTime) {
				speedModifier = Random.Range(minSpeed, maxSpeed);
				startedSpeed = Time.time;
			}
		}
		}
		
		// do the state
		stateHandlers [currentState] ();
	}
}
