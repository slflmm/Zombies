using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SurvivorAI : MonoBehaviour {

	const float BASE_SPEED = 400f;
	public float speedModifier = 1.5F;
	private Vector3[] Directions = new Vector3[] { new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,0,1), new Vector3(0,0,-1), new Vector3(1,0,1), new Vector3(1,0,-1), new Vector3(-1,0,1), new Vector3(-1,0,-1)};

	public List<string> animationList;
	CharacterController controller;

	private List<Vector3> totalPath;
	private bool havePath;
	private int path_idx;

	private List<Vector3> tokenLocations;
	private Vector3 currentToken;
	private Vector3 lastToken;

	private List<Vector3> safeLocations;

	private List<GameObject> visibleZombies;

	private bool hiding;
	private bool started;

	private float lastPathCalcTime;

	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController> ();
		animationList = new List<string> ();
		foreach(AnimationState state in GetComponent<Animation>()) {
			animationList.Add(state.name);
		}
		havePath = false;
		path_idx = 0;
		totalPath = new List<Vector3> ();
		tokenLocations = new List<Vector3>(GameObject.Find ("Objective").GetComponent<GameManager> ().pickUpCoords);
		safeLocations =  new List<Vector3>(new Vector3[] {
			new Vector3(5,0,10), new Vector3(5,0,14), new Vector3(5,0,18), new Vector3(5,0,22), new Vector3(5,0,26),
			new Vector3(8,0,7), new Vector3(8,0,29), 
			new Vector3(12,0,7), new Vector3(12,0,29), 
			new Vector3(15,0,10), new Vector3(15,0,14), new Vector3(15,0,18), new Vector3(15,0,22), new Vector3(15,0,26),
			new Vector3(10,0,1), new Vector3(10,0,35)
		});
		hiding = false;
		lastPathCalcTime = 0;
		started = false;

	}
	
	// Update is called once per frame
	void Update () {

		visibleZombies = seeZombies (transform.position);


			started = true;
		if (visibleZombies.Count == 0) {
			hiding = false;
		}
		if (totalPath == null || path_idx < 0 && (!started || Time.time - lastPathCalcTime > 0.5f))
						havePath = false;
		if (!havePath && !hiding) {
			// find the nearest token and make that your goal
			Vector3 goal;
			if (tokenLocations.Count > 0) {
				goal = findNearestToken();
			}
			else {
				goal = GameObject.Find ("Objective").transform.position;
			}
			currentToken = goal/-100;
			transform.position = roundPosition ();
			totalPath = AStar (roundPosition(),goal,true);
			if (totalPath == null && tokenLocations.Count > 1) {
				// consider the second closest
				Vector3 newGoal = findSecondNearest(goal,tokenLocations);
				totalPath = AStar (roundPosition(),newGoal, true);
				if (totalPath == null) {
					totalPath = AStar (roundPosition(),goal,false);
				}
				else {
					goal = newGoal;
				}
			}
			if (totalPath != null) {
			path_idx = totalPath.Count -2;
			havePath = true;
				lastPathCalcTime = Time.time;
			}
		}
//		if (!started || ) 
		if (visibleZombies.Count > 0 && minZombieDistance() < 700000 && !hiding) {
				started = true;
			// recalculate path if you see any zombies
			Vector3 goal = findNearestSafe();
			Vector3 start = roundPosition ();
			transform.position = start;
			totalPath = AStar (start,goal, true);
			if (totalPath == null) {
				// consider the second closest
				Vector3 newGoal = findSecondNearest(goal,safeLocations);
				totalPath = AStar (start,newGoal, true);
				if (totalPath == null) {
					totalPath = AStar (start,goal,false);
				}
				else goal = newGoal;
			}
			currentToken = goal/-100;
			if (totalPath != null) {
			path_idx = totalPath.Count - 2;
			havePath = true;
			hiding = true;
				lastPathCalcTime = Time.time;
			}
		}
		if (path_idx >= 0 && totalPath != null){
			Vector3 destination = totalPath [path_idx];
			move (destination);
		}
		if (path_idx < 0 && totalPath != null) {
			if (tokenLocations.Count > 0 && !hiding) {
				tokenLocations.Remove (currentToken);
			}
			lastToken = currentToken;
		}

	}



	float minZombieDistance() {
		if (visibleZombies.Count > 0) {
			float min = euclidean_distance(transform.position, visibleZombies [0].transform.position);
			for (int i = 0; i < visibleZombies.Count; i++) {
				float dist = euclidean_distance(transform.position, visibleZombies[i].transform.position);
				if (dist < min) {
					min =  dist;
				}
			}
			return min;
		}
		else {
			return 0;
		}
	}

	Vector3 findNearestToken() {
		float minDist = euclidean_distance (transform.position, tokenLocations [0]*-100);
		Vector3 nearest = tokenLocations [0]*-100;
		for (int i = 0; i < tokenLocations.Count; i++) {
			float dist = euclidean_distance(transform.position, tokenLocations[i]*-100);
			if (dist < minDist) {
				nearest = tokenLocations[i]*-100;
				minDist = dist;
			}
		}
		return nearest;
	}

	Vector3 findSecondNearest(Vector3 nearest, List<Vector3> toCopy) {
		List<Vector3> copySafe = new List<Vector3> (toCopy);
		copySafe.Remove (nearest/-100);
		Vector3 secondNearest = copySafe [0] * -100;
		float minDist = euclidean_distance (transform.position, secondNearest);
		for (int i = 0; i < copySafe.Count; i++) {
			float dist = euclidean_distance(transform.position, copySafe[i]*-100);
			if (dist < minDist) {
				secondNearest = copySafe[i]*-100;
				minDist = dist;
			}
		}
		return secondNearest;
	}

	Vector3 findNearestSafe() {
		float minDist = euclidean_distance (transform.position, safeLocations [0]*-100);
		Vector3 nearest = safeLocations [0]*-100;
		for (int i = 0; i < safeLocations.Count; i++) {
			float dist = euclidean_distance(transform.position, safeLocations[i]*-100);
			if (dist < minDist) {
				nearest = safeLocations[i]*-100;
				minDist = dist;
			}
		}
		return nearest;
	}

	Vector3 roundPosition() {
		int x = (int)(transform.position.x / -100 + 0.5f);
		int z = (int)(transform.position.z / -100f + 0.5f);
		return new Vector3 (x, 0, z) * -100;
	}


	void move(Vector3 destination) {

		Vector3 moveDirection = destination - transform.position; //a+b=c ---> b = c-a

		// instead of discrete, move toward destination by (normalized) moveDirection * base speed * speed modifier * Time.deltaTime
		// and if your position is now within some threshold of the goal, let path_idx--
		if (euclidean_distance(transform.position, destination) < 30) {
			path_idx--;
		}
		else {
			controller.Move (moveDirection.normalized * BASE_SPEED * speedModifier * Time.deltaTime);
		}
	}


	List<GameObject> seeZombies(Vector3 pos) {
		List<GameObject> seenZombies = new List<GameObject> ();
		// for each zombie on the board
		GameObject[] zombies = GameObject.FindGameObjectsWithTag ("zombie");
		foreach (GameObject zombie in zombies) {
			// cast ray between you and zombie, add it to list if you collide with zombie itself
			RaycastHit hit;
			if (Physics.Raycast (pos + new Vector3(0,25,0) ,zombie.transform.position - pos + new Vector3(0,25,0), out hit)
			    && hit.collider.gameObject == zombie) {
				seenZombies.Add (zombie);
				zombie.transform.GetChild (2).gameObject.SetActive(true);
			}
			else {
				zombie.transform.GetChild (2).gameObject.SetActive(false);
			}
		}
		return seenZombies;
	}




	private List<Vector3> AStar(Vector3 start, Vector3 goal, bool strict) {
		List<Vector3> closedSet = new List<Vector3> ();
		List<Vector3> openSet = new List<Vector3> () {start};
		Dictionary<Vector3,Vector3> cameFrom = new Dictionary<Vector3,Vector3> ();
		Dictionary<Vector3, float> g_score = new Dictionary<Vector3,float> ();
		Dictionary<Vector3,float> f_score = new Dictionary<Vector3,float> ();
		
		g_score [start] = 0;
		f_score [start] = g_score [start] + euclidean_distance (start, goal);
		
		Vector3 current = new Vector3 ();
		
		int countlength = 0;

		int i = 0;
		
		while (openSet.Count() != 0 && i < 500) {
			i++;

			current = getLowestF(openSet,f_score);
			if (current == goal) return reconstructPath(cameFrom, goal); // return true, just modify the dungeon map
			
			openSet.Remove(current);
			closedSet.Add (current);
			
			foreach (Vector3 neighbour in getAStarNeighbours(current, strict)) {
				if (closedSet.Contains(neighbour)) continue;

				if (!g_score.ContainsKey(current)) g_score[current] = 0;
				float tentative_g_score = g_score[current] + 100f; // 100 is the distance between neighbours
				
				if (!openSet.Contains (neighbour) || tentative_g_score < g_score[neighbour]) {
					cameFrom[neighbour] = current;
					g_score[neighbour] = tentative_g_score;
					f_score[neighbour] = tentative_g_score + euclidean_distance (neighbour, goal);
					
					if (!openSet.Contains(neighbour)) {
						openSet.Add (neighbour);
					}
				}
			}
		}
		return null;
		
	}
	
	// fill current path with the correct information
	private List<Vector3> reconstructPath(Dictionary<Vector3,Vector3> cameFrom, Vector3 current) {
		List<Vector3> currentPath = new List<Vector3>();
		currentPath.Add (current);
		while (cameFrom.ContainsKey(current)) {
			current = cameFrom[current];
			currentPath.Add(current);
		}
		return currentPath;
	}
	
	private Vector3 getLowestF(List<Vector3> openSet, Dictionary<Vector3,float> f_score) {
		float min = f_score[openSet[0]];
		Vector3 lowestNode = openSet[0];
		foreach (Vector3 v in openSet) {
			if (f_score[v] < min) {
				min = f_score[v];
				lowestNode = v;
			}
		}
		return lowestNode;
	}

	private float euclidean_distance (Vector3 a, Vector3 b) {
		return Mathf.Pow(a.x - b.x,2) + Mathf.Pow(a.z - b.z,2);
	}
	
	// ignores neighbours with a collider and spots inside a zombie's safe zone
	private List<Vector3> getAStarNeighbours(Vector3 node, bool strict) {
		List<Vector3> neighbours = new List<Vector3> ();

		foreach (Vector3 d in Directions) {
//			}
			// (throw a raycast in direction d, only include as a neighbour if you can move there)
			RaycastHit hit;
			bool isDiag = (d.x != 0 && d.z != 0);
			if (!(Physics.Raycast (node,d*-100, out hit) 
			      && ((!isDiag && hit.distance <= 50 && hit.collider.name!="Objective") || (isDiag && hit.distance <= 100)))) {

				Vector3 v = node + d*-100;
				if (v.x <= 0 && v.z <= 0 ) {
					if (!strict || (strict && !zombieCovered (v))) {
						neighbours.Add(v);
					}

				}
			}
		}
		return neighbours;
	}

	private bool zombieCovered(Vector3 pos) {
		// go through each visible zombie and check if pos is in its danger zone
		for (int i = 0; i < visibleZombies.Count; i++) {
			if (visibleZombies[i].GetComponent<Zombie>().inDangerZone(pos,150)) {
				return true;
			}
		}
		return false;
	}
}
