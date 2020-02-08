using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
	
	[Header ("Parameters")]
	public float runSpeed;
	public float fallSpeed;
	public float fallMultiplier;
	public float height;
	public float width;
	public float jumpForce;
	
	[Header ("Physics")]
	public float gravity;
	
	[Header ("Slopes")]
	public Vector3 bottomSensor, topSensor, leftSensor, rightSensor;
	public float slopeSpeedUp;
	public float slideSpeed;
	public float slopeSpeedDown;
	
	[Header ("Misc")]
	public LayerMask excludeSource;
	
	private float jumpHeight;
	private bool grounded;
	private float vertAccel;
	private float vertDelta;
	private float maxFallSpeed;
    // Start is called before the first frame update
    void Start()
    {
        //vertDelta = 0;
		//jumpHeight = 0;
		//vertAccel = gravity;
    }

    // Update is called once per frame
    void Update()
    {
		Gravity();
		Jump();
        Move();
		FinalMovement();
    }
	
	private Vector3 moveDelta;
	private void Move()
	{
		Debug.Log(jumpHeight);
		Debug.DrawRay(transform.position, new Vector3(Input.GetAxis("Horizontal")*runSpeed, 0, 0), Color.blue, 0.2f); // initial movement vector
		moveDelta = collisionSlopeCheck(new Vector3(Input.GetAxis("Horizontal")*runSpeed, 0, 0));
		//Debug.Log(moveDelta.magnitude);
	}
	
	private void FinalMovement()
	{
		Vector3 movement = new Vector3(moveDelta.x, moveDelta.y+vertAccel, 0); // fix for jumping, +vertAccel+jumpHeight
		movement = transform.TransformDirection(movement);
		transform.position += movement*Time.deltaTime;
	}
	
	private Vector3 collisionSlopeCheck(Vector3 dir) // input dir is the intended direction to move to start
	{
		// goal: add multiple sensors
		Vector3 d = transform.TransformDirection(dir);
		Vector3 l_b = transform.TransformPoint(bottomSensor);
		/*Vector3 l_t = transform.TransformPoint(topSensor);
		Vector3 l_l = transform.TransformPoint(leftSensor);
		Vector3 l_r = transform.TransformPoint(rightSensor); */
		Ray ray_b = new Ray(l_b, d); // ray from the sensor in the intended direction
		Vector3 boxSize = new Vector3(0.95f*width/2, 0.95f*height/2, 1.0f);
		/*Ray ray_t = new Ray(l_t, d);
		Ray ray_l = new Ray(l_l, d);
		Ray ray_r = new Ray(l_r, d); */
		RaycastHit hit;
		
		//if (Physics.Raycast(ray_b, out hit, height, excludeSource)) // raycasting to detect incoming collision
		if (Physics.BoxCast(transform.position, boxSize, d, out hit, Quaternion.identity, height, excludeSource))
		{
			// box factor
			Ray rayDir = new Ray(transform.position, hit.point);
			float cos = Vector3.Dot(new Vector3(1.0f, 0, 0), rayDir.direction) / rayDir.direction.magnitude; // cosine of the angle
			float sin = Mathf.Sin(Mathf.Acos(cos)); // sine of the angle
			
			// the following is because it's a rectangle
			if (cos > sin) { sin = 1.0f; }
			else { cos = 1.0f; }
			
			float factor = Mathf.Sqrt( Mathf.Pow(cos*(0.95f*width/2), 2) + Mathf.Pow(sin*(0.95f*height/2), 2) );
			Debug.Log(hit.distance-factor);
			// factor is now the distance between the center and the edge of the box where relevant
			if (hit.distance <= 0.05f)
			{
				Debug.DrawLine(transform.position-new Vector3(cos*(width/2), sin*(height/2), 0), hit.point, Color.yellow, 0.2f); // draw line to the predicted point of impact
				Vector3 temp = Vector3.Cross(hit.normal, d);
				Debug.DrawRay(hit.point, temp*200, Color.green, 0.2f);
				Vector3 newDir = Vector3.Cross(temp, hit.normal);
				Debug.DrawRay(hit.point, newDir * 20, Color.red, 0.2f);
				Vector3 dir2 = newDir*slideSpeed;
				RaycastHit wallCheck = wallCheckDetails(dir2);
				if (wallCheck.transform != null)
				{
					dir2 *= wallCheck.distance*0.5f;
				}
				//transform.position += dir2;
				return dir2;
			}
			else { return dir; }
		}
		
		return dir;
	}
	
	private void Gravity() // functioning correctly
	{
		Vector3 boxPos = new Vector3(transform.position.x, transform.position.y-(height/2), transform.position.z);
		Vector3 boxSize = new Vector3(width*0.95f, 0.1f, 1.0f);
		grounded = Physics.CheckBox(boxPos, boxSize/2, Quaternion.identity, excludeSource, QueryTriggerInteraction.Ignore);
		if (grounded) { vertAccel = 0; }
		else { vertAccel -= gravity*Time.deltaTime; }
		if (grounded)
		{
			Ray ray = new Ray(transform.position, Vector3.down*(height)); // raycast from center downwards
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, excludeSource))
			{
				if (hit.distance <= (height/2))
				{
					Debug.DrawRay(ray.origin, ray.direction*20, Color.green, 0.2f);
					Vector3 needed = new Vector3(transform.position.x, hit.point.y+(height/2), transform.position.z);
					transform.position = needed;
				}
				else if (hit.distance > (height))
				{
					grounded = true;
					//vertAccel -= gravity*Time.deltaTime;
				}
			}
		}
	}
	
	private void Jump()
	{
		if (grounded)
		{
			if (jumpHeight > 0)
			{
				vertAccel = 0;
			}
		}
		/*
		if (!grounded)
		{
			if (jumpHeight > 0)
			{
				jumpHeight -= (jumpHeight*Time.deltaTime);
			}
			else { jumpHeight = 0; }
		}
		*/
		if (Input.GetKeyDown(KeyCode.W))
		{
			vertAccel = jumpForce;
		}
		/*
		Vector3 boxSize = new Vector3(0.95f*width/2, 0.05f, 1.0f);
		RaycastHit hit;
		if (vertAccel > 0 && Physics.BoxCast(transform.position+new Vector3(0, (height/2)+vertAccel, 0), boxSize, new Vector3(0, height, 0), out hit, Quaternion.identity, vertAccel, excludeSource)) // hit something
		{
			if (hit.distance <= 0.05)
			{
				if (vertAccel > 0) { vertAccel = 0; }
			}
		}
		*/
	}
	
	private void OnDrawGizmos()
	{
		Vector3 boxPos = new Vector3(transform.position.x, transform.position.y-(height/2), transform.position.z);
		Vector3 boxSize = new Vector3(width, 0.1f, 1.0f);
		if(!grounded) { Gizmos.color = Color.red; }
		else { Gizmos.color = Color.green; }
		Gizmos.DrawWireCube(boxPos, boxSize);
		
		Gizmos.color = Color.yellow;
		Vector3 l = transform.TransformPoint(bottomSensor);
		Gizmos.DrawWireSphere(l, 0.2f);
	}
	
	private RaycastHit wallCheckDetails(Vector3 dir)
	{
		Vector3 l = transform.TransformPoint(bottomSensor);
		Ray ray = new Ray(l, dir);
		RaycastHit hit;
		
		if (Physics.Raycast(ray, out hit, 0.2f, excludeSource))
		{
			return hit;
		}
		return hit;
	}
}
