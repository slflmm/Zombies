﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShamblerZombie : Zombie {

	new void Start() {
		
		base.Start ();
		
		speedModifier = 0.5f;
		regularSpeed = 0.5f;
		
		stateHandlers = new Dictionary<State, StateHandlerDelegate>();
		stateHandlers [State.Walk] = Walk;
		stateHandlers [State.Stop] = Stop;
		stateHandlers [State.ChangeLanes] = ChangeLanes;
		
		currentState = State.Walk;
	}
	
	void Update() {
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
			// otherwise handle as classic zombie would
			else if (d <= 150) {
				currentState = State.Stop;
			}
			else {
				speedModifier = Mathf.Min(zSpeedModifier, regularSpeed);
				currentState = State.Walk;
			}
		}
		else {
			speedModifier = regularSpeed;
			currentState = State.Walk;

			// randomly change lanes
			if (Random.value < 0.005 && canChangeLanes()) {
				currentState = State.ChangeLanes;
			}
		}

		
		// do the state
		stateHandlers [currentState] ();
		
	}
}
