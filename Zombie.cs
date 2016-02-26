using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour {

	const float BASE_SPEED = 400f;//75f;

	public int direction = 1;
	public int lane;
	public Vector3[,] corners = new Vector3[,]
	{
		{new Vector3(-100,0,-300), new Vector3(-100,0,-3300), new Vector3(-1900,0,-3300), new Vector3(-1900,0,-300)},
		{new Vector3(-200,0,-400), new Vector3(-200,0,-3200), new Vector3(-1800,0,-3200), new Vector3(-1800,0,-400)},
		{new Vector3(-300,0,-500), new Vector3(-300,0,-3100), new Vector3(-1700,0,-3100), new Vector3(-1700,0,-500)}
	};

	public float speedModifier = 1f;
	public float regularSpeed = 1f;
	public float slowSpeed = 0.5f;

	public float maxSpeed = 2f;
	public float minSpeed = 0.5f;

	public delegate void StateHandlerDelegate();
	public Dictionary<State, StateHandlerDelegate> stateHandlers;

	public enum State { Walk, Stop, ChangeLanes, ChangeDirection };
	public State currentState;

	private List<string> animationList;
	private CharacterController characterController;

	public float laneCooldown = 1; // don't change lanes more than once per second
	private float lastLaneChange;
	private bool laneTimerStarted = false;

	public float directionCooldown = 1;
	private float lastDirectionChange;
	private bool directionTimerStarted = false;

	public void Start() {
		characterController = GetComponent<CharacterController> ();
		animationList = new List<string> ();
		foreach(AnimationState state in GetComponent<Animation>()) {
			animationList.Add(state.name);
		}

		direction = 1;
	}
	
	public void Walk() {
		GetComponent<Animation>().CrossFade(animationList[0],0.01f);
		characterController.Move (transform.forward * BASE_SPEED * speedModifier * Time.deltaTime);
		turnIfShould ();
		detectSurvivor ();
	}

	public void Stop() {
		GetComponent<Animation>().CrossFade (animationList [1], 0.01f);
		detectSurvivor ();
	}

	public void ChangeDirection() {
		if (!canChangeDirection ()) return;
		transform.Rotate (0, 180, 0);
		direction *= -1;
		lastDirectionChange = Time.time;
		directionTimerStarted = true;
	}

	public bool canChangeDirection() {
		if (directionTimerStarted && Time.time - lastDirectionChange < directionCooldown)
			return false;
		return true;
	}

	public void ChangeLanes() {

		// don't do anything if you're too close to a corner
		if (!inChangeBounds()) {
			return;
		}
		// don't do anything if you're not past the cooldown period
		if (laneTimerStarted && Time.time - lastLaneChange < laneCooldown) {
			return;
		}

		bool moved = false;

		// if in lane 0, go to lane 1 by transform.right*direction*100
		if (lane == 0 && canRight ()) {
			transform.position = transform.position + transform.right*direction*100;
			lane = 1;
			moved = true;
		}
		// if in lane 2, go to lane 1 by transform.left*direction*100
		else if (lane == 2 && canLeft ()) {
			transform.position = transform.position - transform.right*direction*100;
			lane = 1;
			moved = true;
		}
		// if in lane 1, 50-50 which direction you try first (but do try both!)
		else if (lane == 1) {
			if (Random.value < 0.5) {
				if (canLeft ()) {
					transform.position = transform.position - transform.right*direction*100;
					lane = 0;
					moved = true;
				}
				else if (canRight ()) {
					transform.position = transform.position + transform.right*direction*100;
					lane = 2;
					moved = true;
				}
			}
			else {
				if (canRight ()) {
					transform.position = transform.position + transform.right*direction*100;
					lane = 2;
					moved = true;
				}
				else if (canLeft ()) {
					transform.position = transform.position - transform.right*direction*100;
					lane = 0;
					moved = true;
				}
			}
		}
		if (moved) {
			lastLaneChange = Time.time;
			laneTimerStarted = true;
		}
	}

	public bool canChangeLanes() {
		if (!inChangeBounds()) {
			return false;
		}
		if (laneTimerStarted && Time.time - lastLaneChange < laneCooldown) {
			return false;
		}

		if (lane == 0 && canRight ())
						return true;
		if (lane == 2 && canLeft ())
						return true;
		if (lane == 1 && (canLeft () || canRight ()))
						return true;

		return false;

	}

	private bool inChangeBounds() {
		if ((transform.position.x == corners[lane,0].x 
		     		&& transform.position.z <= corners[lane,0].z-200 
		     		&& transform.position.z >= corners[lane,1].z+200)
		    || (transform.position.z == corners[lane,1].z
		 	         && transform.position.x <= corners[lane,1].x-200
		    	     && transform.position.x >= corners[lane,2].x+200)
		    || (transform.position.x == corners[lane,2].x
		             && transform.position.z >= corners[lane,2].z+200
		             && transform.position.z <= corners[lane,3].z-200)
		    || (transform.position.z == corners[lane,3].z
		             && transform.position.x >= corners[lane,3].x+200 
		             && transform.position.x <= corners[lane,0].x-200)) 
			return true;
		else 
			return false;
	}

	public bool canRight() {
		Vector3 rayStart = transform.position + new Vector3 (0, 35, 0);
		Vector3 dir1 = 100 * transform.right * direction;
		Vector3 dir2 = dir1 + 100*transform.forward;
		Vector3 dir3 = dir1 - 100*transform.forward;
		Vector3 dir4 = dir1 + 50 * transform.forward;
		Vector3 dir5 = dir1 - 50 * transform.forward;
		Vector3 dir6 = dir1 + 150 * transform.forward;
		Vector3 dir7 = dir1 - 150 * transform.forward;

		if (!Physics.Raycast (rayStart, dir1, 200) && !Physics.Raycast (rayStart, dir2, 200) && !Physics.Raycast (rayStart, dir3, 200)
		    && !Physics.Raycast (rayStart, dir4, 200) && !Physics.Raycast(rayStart, dir5, 200) && !Physics.Raycast(rayStart, dir6, 250)
		    && !Physics.Raycast(rayStart, dir7, 250))  {
			return true;
		}
		return false;
	}

	public bool canLeft() {
		Vector3 rayStart = transform.position + new Vector3 (0, 35, 0);
		Vector3 dir1 = 100 * -transform.right * direction;
		Vector3 dir2 = dir1 + 80 * transform.forward;
		Vector3 dir3 = dir1 - 80 * transform.forward;
		Vector3 dir4 = dir1 + 40 * transform.forward;
		Vector3 dir5 = dir1 - 40 * transform.forward;
		Vector3 dir6 = dir1 + 120 * transform.forward;
		Vector3 dir7 = dir1 - 120 * transform.forward;

		if (!Physics.Raycast (rayStart, dir1, 150) && !Physics.Raycast (rayStart, dir2, 150) && !Physics.Raycast (rayStart, dir3, 150)
		    && !Physics.Raycast (rayStart, dir4, 150) && !Physics.Raycast(rayStart, dir5, 150) && !Physics.Raycast(rayStart, dir6, 215)
		    && !Physics.Raycast(rayStart, dir7, 215))  {
			return true;
		}
		return false;
	}


	private void turnIfShould() {
		Vector3 pos = transform.position;
		bool hitCorner = false;
		int corner = 0;
		// if you've just hit corner upper-left
		if ((pos.x > corners[lane,0].x && (Mathf.Abs(pos.z - corners[lane,0].z) < 2) && direction == 1) || 
		    (pos.z > corners[lane,0].z && (Mathf.Abs (pos.x - corners[lane,0].x)< 2) && direction == -1)) {
			transform.Rotate (0,90*direction,0);
			pos.x = corners[lane,0].x;
			pos.z = corners[lane,0].z;
			hitCorner = true;
			corner = 0;
		}
		// if you've just hit corner upper-right
		else if ((pos.z < corners[lane,1].z && (Mathf.Abs(pos.x - corners[lane,1].x) < 2) && direction == 1) || 
		    (pos.x > corners[lane,1].x && (Mathf.Abs (pos.z - corners[lane,1].z) < 2) && direction == -1)) {
			transform.Rotate (0,90*direction,0);
			pos.x = corners[lane,1].x;
			pos.z = corners[lane,1].z;
			hitCorner = true;
			corner = 1;
		}
		// if you've just hit corner lower-right
		else if ((pos.x < corners[lane,2].x && (Mathf.Abs(pos.z - corners[lane,2].z) < 2) && direction == 1) || 
		         (pos.z < corners[lane,2].z && (Mathf.Abs (pos.x - corners[lane,2].x) < 2) && direction == -1)) {
			transform.Rotate (0,90*direction,0);
			pos.x = corners[lane,2].x;
			pos.z = corners[lane,2].z;
			hitCorner = true;
			corner = 2;
		}
		else if ((pos.z > corners[lane,3].z && (Mathf.Abs(pos.x - corners[lane,3].x) < 2) && direction == 1) || 
	         (pos.x < corners[lane,3].x && (Mathf.Abs (pos.z - corners[lane,3].z)< 2) && direction == -1)) {
			transform.Rotate (0,90*direction,0);
			pos.x = corners[lane,3].x;
			pos.z = corners[lane,3].z;
			hitCorner = true;
			corner = 3;
		}

		// given some probability p, zombie will despawn and respawn elsewhere
		GameObject t = GameObject.Find ("Terrain");
		Spawner s = t.GetComponent<Spawner> ();
		if (hitCorner && Random.value < s.p) {
			if (s.respawn(this.gameObject, corner)) {
				Destroy (this.gameObject);
			}
		}
		transform.position = pos;
	}

	public void detectSurvivor() {
		// find the object with survivor tag
		GameObject survivor = GameObject.Find ("Survivor");
		Vector3 survivor_pos = survivor.transform.position;

		if (inDangerZone (survivor_pos,0)) {
			GameObject.Find ("Objective").GetComponent<GameManager>().GameOver();
		}
	}

	public bool inDangerZone(Vector3 pos, float cushion) {
		// if within 1.5 to left/right/back, or 7.5 forward, game over (add 35, the survivor's collider's radius!)
		if (transform.forward == new Vector3(1,0,0)) {
			// i.e. if you're facing up
			if ((pos.x < transform.position.x + transform.forward.x*(7.5f*100f + cushion)) &&
			    (pos.x > transform.position.x + transform.forward.x*(-1.5f*100f - cushion)) &&
			    (pos.z < transform.position.z + transform.right.z*(-1.5f*100f - cushion)) && 
			    (pos.z > transform.position.z + transform.right.z*(1.5f*100f + cushion))) {
				return true;
			}
		}
		else if (transform.forward == new Vector3(-1,0,0)) {
			if ((pos.x > transform.position.x + transform.forward.x*(7.5f*100f + cushion)) &&
			    (pos.x < transform.position.x + transform.forward.x*(-1.5*100f - cushion)) &&
			    (pos.z > transform.position.z + transform.right.z*(-1.5f*100f - cushion)) && 
			    (pos.z < transform.position.z + transform.right.z*(1.5f*100f + cushion))) {
				return true;
			}
		}
		else if (transform.forward == new Vector3(0,0,1)) {
			// i.e. if you're facing left
			if ((pos.z < transform.position.z + transform.forward.z*(7.5f*100f + cushion)) &&
			    (pos.z > transform.position.z + transform.forward.z*(-1.5*100f - cushion)) &&
			    (pos.x > transform.position.x + transform.right.x*(-1.5f*100f - cushion)) && 
			    (pos.x < transform.position.x + transform.right.x*(1.5f*100f + cushion))) {
				return true;
			}
		}
		else {
			if ((pos.z > transform.position.z + transform.forward.z*(7.5f*100f + cushion)) &&
			    (pos.z < transform.position.z + transform.forward.z*(-1.5*100f - cushion)) &&
			    (pos.x < transform.position.x + transform.right.x*(-1.5f*100f - cushion)) && 
			    (pos.x > transform.position.x + transform.right.x*(1.5f*100f + cushion))) {
				return true;
			}
		}
		return false;
	}
}

