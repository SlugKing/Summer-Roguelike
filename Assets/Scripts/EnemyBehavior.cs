using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
	
	private CharacterController cc;
	public int dir;
	public float MoveMult = 2.0f;
	private float maxFallSpd = -10.0f;
	private float vertSpd = 0;
	public bool changedDir;
	public LayerMask excludeEntities;
	public float halfWidth;
	public float halfHeight;
	public GameObject player;
	public float activationRange;
	public int enemType;
	
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
		//cc.detectCollisions = false;
		dir = 1;
		player = GameObject.FindWithTag("Player");
		changedDir = false;
    }

    // Update is called once per frame
    void Update()
    {
		if (Mathf.Sqrt(Mathf.Pow(transform.position.x-player.transform.position.x, 2) + Mathf.Pow(transform.position.y-player.transform.position.y, 2)) <= activationRange)
		{
			// changing directions at walls and preventing falling off
			Ray ray = new Ray(transform.position+new Vector3(dir*MoveMult*Time.deltaTime, 0, 0), Vector3.down);
			Ray wallrayRight = new Ray(transform.position+new Vector3(halfWidth, 0, 0), Vector3.right);
			Ray wallrayRightSlope = new Ray(transform.position+new Vector3(halfWidth, halfHeight, 0), new Vector3(1.0f, 1.0f, 0));
			Ray wallrayLeft = new Ray(transform.position+new Vector3(-halfWidth, 0, 0), -Vector3.right);
			Ray wallrayLeftSlope = new Ray(transform.position+new Vector3(-halfWidth, halfHeight, 0), new Vector3(-1.0f, 1.0f, 0));
			RaycastHit hit, slopehit;
			// right
			// Physics.Raycast(wallrayRight, out hit, Mathf.Infinity, excludeEntities) && 
			//hit.distance < 0.1f && 
			if (Physics.Raycast(wallrayRight, out hit, Mathf.Infinity, excludeEntities))
			{
				//Debug.DrawRay(wallrayRight.origin, wallrayRight.direction*0.1f, Color.blue, 0.2f);
				Debug.DrawRay(wallrayRight.origin, wallrayRight.direction*0.1f, Color.blue, 0.2f);
				if (hit.distance < 0.15f)
				{
					if (changedDir == false)
					{
						dir *= -1;
						changedDir = true;
					}
				}
				else if (hit.distance >= 0.15f && changedDir == true) { changedDir = false; }
			}
			// left
			// Physics.Raycast(wallrayLeft, out hit, Mathf.Infinity, excludeEntities) && 
			//hit.distance < 0.1f && 
			if (Physics.Raycast(wallrayLeft, out hit, Mathf.Infinity, excludeEntities))
			{
				//Debug.DrawRay(wallrayLeft.origin, wallrayLeft.direction*0.1f, Color.blue, 0.2f);
				Debug.DrawRay(wallrayLeft.origin, wallrayLeft.direction*0.1f, Color.blue, 0.2f);
				if (hit.distance < 0.15f)
				{
					if (changedDir == false)
					{
						dir *= -1;
						changedDir = true;
					}
				}
				else if (hit.distance >= 0.15f && changedDir == true) { changedDir = false; }
			}
			if (cc.isGrounded && Physics.Raycast(ray, out hit, Mathf.Infinity, excludeEntities))
			{
				
				if (hit.distance >= 1.0f)
				{
					Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.2f);
					if (cc.isGrounded) { dir *= -1; }
				}
				//else { Debug.DrawRay(ray.origin, ray.direction*2.0f, Color.green, 0.2f); }
			}
			
			Vector3 moveDelta = new Vector3(dir*MoveMult, 0, 0);
			vertSpd -= 19.6f * Time.deltaTime;
			if ((cc.collisionFlags & CollisionFlags.Above) != 0 && vertSpd > 0) { vertSpd = 0; }
			//if (!changedDir && (cc.collisionFlags & CollisionFlags.Sides) != 0) { dir *= -1; changedDir = true; }
			//Debug.Log(dir);
			//if (changedDir) { changedDir = false; }
			if (vertSpd < maxFallSpd) { vertSpd = maxFallSpd; }
			moveDelta.y = vertSpd;
			cc.Move(moveDelta * Time.deltaTime);
			if (cc.isGrounded)
			{
				vertSpd = 0;
			}
			transform.position = new Vector3(transform.position.x, transform.position.y, 0);
		}
    }
	
	/*
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Debug.Log(hit.normal.x - 1.0f);
		if (Mathf.Abs(hit.normal.x + 1.0f) < 0.2f) { dir = -1; }
		else if (Mathf.Abs(hit.normal.x - 1.0f) < 0.2f) { dir = 1; }
	}
	*/
}
