using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxDetails : MonoBehaviour
{
	public int damage;
	public bool persistent = true;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	private void OnTriggerEnter(Collider other)
	{
		Entity e;
		if (other.gameObject.transform.root.name.Contains("Player")) { e = other.gameObject.transform.root.GetComponent<Entity>(); }
		else { e = other.gameObject.GetComponent<Entity>(); }
		Entity owner;
		if (transform.root.name.Contains("Player")) { owner = transform.root.GetComponent<Entity>(); }
		else { owner = transform.parent.GetComponent<Entity>(); }
		if (e.faction != owner.faction && !e.soakedHitboxes.Contains(gameObject.GetInstanceID())) // apply damage
		{
			e.takeDamage(damage, owner.gameObject.GetComponent<CharacterController>());
			owner.scoreDamage(damage);
			if (!persistent)
			{
				e.soakedHitboxes.Add(gameObject.GetInstanceID());
			}
		}
	}
	
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Vector3 size = gameObject.GetComponent<BoxCollider>().size;
		Gizmos.DrawWireCube(transform.position, size);
	}
}
