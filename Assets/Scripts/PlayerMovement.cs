using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	private float MoveMult = 5.0f;
	private float JumpMult = 10.0f;
	private float maxFallSpd = -10.0f;
	private float FallSpd = -10.0f;
	private float FastFallSpd = -16.0f;
	private float FastFallThreshold = 1.0f;
	private bool CanJump;
	//private Rigidbody rb;
	private int JumpTotal = 5;
	private int JumpMidair;
	private int JumpReserve;
	private bool JumpPress = false;
	private bool PressedDown = false;
	private float prevVertAxis;
	//private Vector3 halfExtents = new Vector3(0.5f, 1.0f, 0.5f);
	private CharacterController cc;
	private float vertSpd = 0;
	public int dir = 1;
	public float height;
	public LayerMask excludeEntities;
	private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
		JumpMidair = JumpTotal-1;
		JumpReserve = 0;
		//rb = GetComponent<Rigidbody>();
		cc = GetComponent<CharacterController>();
		//cc.detectCollisions = false;
        CanJump = false;
		anim = transform.Find("Visuals").GetComponent<Animator>();
		prevVertAxis = Input.GetAxis("Vertical");
    }

    // Update is called once per frame
    void Update()
    {
		if (prevVertAxis < -0.5f) { PressedDown = true; }
		if (Input.GetAxis("Vertical") >= 0) { PressedDown = false; }
		if (!transform.Find("Visuals").GetComponent<PlayerMoveset>().attacking && !transform.Find("Visuals").GetComponent<PlayerMoveset>().moveDisabled)
		{
			if (Input.GetAxis("Horizontal") > 0) { dir = 1; }
			else if (Input.GetAxis("Horizontal") < 0) { dir = -1; }
			//transform.Find("Visuals").localScale = new Vector3(Mathf.Abs(transform.localScale.x)*dir, transform.localScale.y, transform.localScale.z);
		}
		transform.Find("Visuals").localScale = new Vector3(Mathf.Abs(transform.localScale.x)*dir, transform.localScale.y, transform.localScale.z);
		Vector3 moveDelta;
		if (!transform.Find("Visuals").GetComponent<PlayerMoveset>().moveDisabled)
		{
			moveDelta = new Vector3(Input.GetAxis("Horizontal")*MoveMult, 0, 0);
		}
		else
		{
			moveDelta = new Vector3(0, 0, 0);
		}
		if (!transform.Find("Visuals").GetComponent<PlayerMoveset>().moveDisabled && CanJump && !JumpPress && Input.GetAxisRaw("Vertical") > 0 && !transform.Find("Visuals").GetComponent<PlayerMoveset>().attacking)
		{
			JumpPress = true;
			JumpReserve--;
			anim.SetTrigger("jump");
			vertSpd = JumpMult;
		}
		if (transform.Find("Visuals").GetComponent<PlayerMoveset>().moveDisabled && vertSpd > JumpMult/2.0f) { vertSpd /= 2.0f; }
		if (JumpReserve <= 0) { CanJump = false; }
		if (!cc.isGrounded) { vertSpd -= 19.6f * Time.deltaTime; }
		if ((cc.collisionFlags & CollisionFlags.Above) != 0) { vertSpd = 0; }
		if (!JumpPress && !PressedDown && Input.GetAxisRaw("Vertical") < 0 && vertSpd <= FastFallThreshold) // fastfall
		{
			vertSpd += (FastFallSpd-FallSpd); // instant adjustment
			maxFallSpd = FastFallSpd;
		}
		if (vertSpd < maxFallSpd) { vertSpd = maxFallSpd; } // terminal velocity - linked to fall speed
		Ray platformRay = new Ray(transform.position+new Vector3(0, -height/2.0f, 0), Vector3.down);
		RaycastHit hit;
		if (vertSpd > 0) { gameObject.layer = 10; }
		else if (Physics.Raycast(platformRay, out hit, Mathf.Infinity, excludeEntities))
		{
			Debug.DrawRay(platformRay.origin, platformRay.direction, Color.blue, 0.2f);
			if (hit.distance < 0.15f && !PressedDown) { gameObject.layer = 0; }
		}
		Ray hangingRay = new Ray(transform.position, Vector3.down);
		if (cc.isGrounded && Physics.Raycast(hangingRay, out hit, Mathf.Infinity, excludeEntities))
		{
			if (hit.distance >= height/2.0f + 0.15f) { gameObject.layer = 10; }
		}
		moveDelta.y = vertSpd;
		cc.Move(moveDelta * Time.deltaTime);
		if (cc.isGrounded)
		{
			JumpReserve = JumpTotal;
			CanJump = true;
			vertSpd = 0;
			maxFallSpd = FallSpd;
			gameObject.layer = 0;
			FastFallThreshold = 1.0f;
		}
		if (Input.GetAxis("Vertical") < 0) { gameObject.layer = 10; }
		if (cc.isGrounded && !PressedDown && Input.GetAxis("Vertical") < -0.2f) // platform drop
		{
			PressedDown = true;
			gameObject.layer = 10;
			vertSpd -= 1.0f;
			FastFallThreshold = -1.0f;
		}
		else if (!cc.isGrounded && JumpReserve > JumpMidair) { JumpReserve = JumpMidair; }
		if (Input.GetAxisRaw("Vertical") == 0) { JumpPress = false; }
		transform.position = new Vector3(transform.position.x, transform.position.y, 0);
		anim.SetFloat("verticalSpeed", vertSpd);
		prevVertAxis = Input.GetAxis("Vertical");
    }
	
	/* OLD RAYCAST CODE THIS SUCKS ASS
		RaycastHit[] leftwalldist = Physics.BoxCastAll(center: transform.position, halfExtents: halfExtents - new Vector3(0, 0.1f, 0.1f), orientation: Quaternion.identity, maxDistance: Mathf.Infinity, direction: new Vector3(-1.0f, 0, 0), layerMask: 1 << 9);
		RaycastHit[] rightwalldist = Physics.BoxCastAll(center: transform.position, halfExtents: halfExtents - new Vector3(0, 0.1f, 0.1f), orientation: Quaternion.identity, maxDistance: Mathf.Infinity, direction: this.transform.right, layerMask: 1 << 9);
		RaycastHit closestleft = leftwalldist[0];
		RaycastHit closestright = rightwalldist[0];
		for (int i = 0; i < leftwalldist.Length; i++)
		{
			if (leftwalldist[i].distance < closestleft.distance && leftwalldist[i].transform.name != this.transform.name) { closestleft = leftwalldist[i]; }
		}
		for (int i = 0; i < rightwalldist.Length; i++)
		{
			if (rightwalldist[i].distance < closestright.distance && rightwalldist[i].transform.name != this.transform.name) { closestright = rightwalldist[i]; }
		}
		Debug.Log(closestright.distance);
		
		if ( (closestleft.distance > 0.001 && Input.GetAxis("Horizontal") < 0) || (closestright.distance > 0.001 && Input.GetAxis("Horizontal") > 0) )
		{
			rb.MovePosition(transform.position + new Vector3(Input.GetAxis("Horizontal")*MoveMult, 0, 0));
		}
		if (CanJump && JumpReserve > 0 && Input.GetAxisRaw("Vertical") != 0)
		{
			if (JumpPress == false)
			{
				JumpPress = true;
				JumpReserve -= 1;
				// contains the jump height factor
				CanJump = false;
				rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				rb.AddForce(new Vector3(0, 7.0f, 0), ForceMode.Impulse);
			}
			
		}
		// if the collider changes then CHANGE THIS - second argument
		RaycastHit[] grounddist = Physics.BoxCastAll(center: transform.position, halfExtents: halfExtents - new Vector3(0.1f, 0, 0), direction: new Vector3(0, -1.0f, 0), orientation: Quaternion.identity, maxDistance: Mathf.Infinity, layerMask: 1 << 9);
		RaycastHit closestground = grounddist[0];
		string name = "";
		for (int i = 0; i < grounddist.Length; i++)
		{
			if (grounddist[i].distance < closestground.distance && grounddist[i].transform.name != this.transform.name) { closestground = grounddist[i]; }
		}
		if (closestground.distance <= 0.001 || JumpReserve > 0) { CanJump = true; }
		if (JumpReserve <= 0 && closestground.distance <= 0.001) { JumpReserve = JumpTotal; }
		if (Input.GetAxisRaw("Vertical") == 0) { JumpPress = false; }
		*/
}
