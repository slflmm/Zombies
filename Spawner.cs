using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

	public Transform classicZombie;
	public Transform shamblerZombie;
	public Transform modernZombie;
	public Transform phoneZombie;

	public enum Settings { maxN, maxP, maxR, noZombies };
	public Settings setting;

	public int n; // parameter for number of zombies to spawn
	public float p; // parameter for probability of despawn/respawn
	public float r; // ratio of easy/hard zombies

	private int maxNn = 6;
	private float maxNp = 0;
	private float maxNr = 0;

	private int maxPn = 1;
	private float maxPp = 0.7f;
	private float maxPr = 0;

	private int maxRn = 2;
	private float maxRp = 0.2f;
	private float maxRr = 0.7f;


	public enum ZombieType { Classic, Shambler, Modern, Phone };
	private int[] segmentToRotation = new int[] {0, 90, 180, 270}; // numbered from bottom edge clockwise

	public Vector3[,] corners = new Vector3[,]
	{
		{new Vector3(-100,0,-300), new Vector3(-100,0,-3300), new Vector3(-1900,0,-3300), new Vector3(-1900,0,-300)},
		{new Vector3(-200,0,-400), new Vector3(-200,0,-3200), new Vector3(-1800,0,-3200), new Vector3(-1800,0,-400)},
		{new Vector3(-300,0,-500), new Vector3(-300,0,-3100), new Vector3(-1700,0,-3100), new Vector3(-1700,0,-500)}
	};

	// Use this for initialization
	void Start () {

		switch(setting) {
		case Settings.maxN:
			n = maxNn;
			p = maxNp;
			r = maxNr;
			break;
		case Settings.maxP:
			n = maxPn;
			p = maxPp;
			r = maxPr;
			break;
		case Settings.maxR:
			n = maxRn;
			p = maxRp;
			r = maxRr;
			break;
		default:
			n = 0;
			p = 0;
			r = 0;
			break;
		}


		// spawn n zombies
		for (int i = 0; i < n; i++) {
			float x, z;
			int segment, lane;
			do {
				segment = Random.Range (0,3);
				lane = Random.Range (0,3);
				if (segment == 0 || segment == 2) {
					// line is ___, so x is the value and y is in range
					x = corners[lane,(segment+2)%4].x;
					if (lane == 0) {
						z = Random.Range (corners[lane,2].z+100, corners[lane,3].z-100);
					}
					else {
						z = Random.Range (corners[lane,1].z+100, corners[lane,0].z-100);
					}
				}
				else {
					z = corners[lane,0].z;
					x = Random.Range (corners[lane,3].x+100, corners[lane,0].x-100);
				}
			} while (isOccupied(new Vector3(x,0,z), lane)); // keep looking until the spot is not occupied

			spawn (new Vector3(x,0,z), lane, segment, (ZombieType)Random.Range (0,2));
		}
	}

	public bool respawn(GameObject zombie, int corner) {
		// check each corner until it isn't this one and is free (or if you've tried 20 times)
		Vector3 oldPos = zombie.transform.position;
		int tries = 0;
		Vector3 pos;
		int lane;
		int c;
		do {
			int idx = Random.Range (0, 12);
			c = idx%4;
			lane = idx/4;
			pos = corners[lane,c];
			tries++;
		} while (tries < 20 && isOccupied(pos, lane) && pos == oldPos);


		if (tries < 19) {
			int segment = (c+2)%4;
			ZombieType type;
			if (Random.value < r) {
				// do a difficult zombie
				type = (ZombieType)Random.Range (2,4);
			}
			else {
				// do an easy zombie
				type = (ZombieType)Random.Range (0,2);
			}
			spawn (pos,lane,segment,type);
			return true;
		}
		return false;

	}

	// creates a new zombie at position
	void spawn(Vector3 position, int lane, int segment, ZombieType type) {
		Transform z;
		switch (type)
		{
		case ZombieType.Classic:
			z = Instantiate (classicZombie, position, Quaternion.identity) as Transform;
			break;
		case ZombieType.Shambler:
			z = Instantiate (shamblerZombie, position, Quaternion.identity) as Transform;
			break;
		case ZombieType.Modern:
			z = Instantiate (modernZombie, position, Quaternion.identity) as Transform;
			break;
		case ZombieType.Phone:
			z = Instantiate (phoneZombie, position, Quaternion.identity) as Transform;
			break;
		default:
			z = Instantiate (classicZombie, position, Quaternion.identity) as Transform;
			break;
		}
		z.eulerAngles = new Vector3 (0, segmentToRotation [segment], 0);
		z.GetComponent<Zombie> ().lane = lane;
	}

	private bool isOccupied(Vector3 pos, int lane) {
		// draw a + across the 'square' around the position, see if it intersects a zombie on the given lane
		Vector3 upStart = pos + 100*Vector3.forward;
		Vector3 downStart = pos - 100*Vector3.forward;
		Vector3 leftStart = pos + 100*Vector3.left;
		Vector3 rightStart = pos - 100*Vector3.left;
		Vector3[] starts = new Vector3[] {upStart, downStart, leftStart, rightStart };

		Vector3 dir1 = -200 * Vector3.forward;
		Vector3 dir2 = 200 * Vector3.forward;
		Vector3 dir3 = -200 * Vector3.left;
		Vector3 dir4 = 200 * Vector3.right;
		Vector3[] dirs = new Vector3[] {dir1, dir2, dir3, dir4};

		RaycastHit hit;
		for (int i = 0; i < 4; i++) {
			if (Physics.Raycast (starts[i], dirs[i], out hit) && hit.collider.GetComponent<Zombie>() != null && hit.distance < 200) {
				if (hit.collider.GetComponent<Zombie>().lane == lane) return true;
			}
		}
		return false;
	}

}
