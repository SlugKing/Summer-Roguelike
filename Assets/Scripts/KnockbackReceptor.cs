using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackReceptor : MonoBehaviour
{
	
	private Vector3 storedKB;
	private CharacterController cc;
	
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (storedKB.magnitude > 0.2f) { cc.Move(storedKB * Time.deltaTime); }
		storedKB = Vector3.Lerp(storedKB, Vector3.zero, 5.0f*Time.deltaTime);
    }
	
	public void addKB(Vector3 newKB)
	{
		storedKB = new Vector3(storedKB.x + newKB.x, storedKB.y + newKB.y, storedKB.z + newKB.z);
	}
	
	
}
