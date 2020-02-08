using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	public int maxHP;
	public int currHP;
	public int faction;
	public IList<int> soakedHitboxes;
	public int enemIndex = -1;
	public RoomCreator rc;
	private bool invincible = false;
	private float invuln_timer = 0;
	
    // Start is called before the first frame update
    void Start()
    {
        currHP = maxHP;
		soakedHitboxes = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
		if (invuln_timer <= 0) { invincible = false; }
		if (invincible) { invuln_timer -= Time.deltaTime; }
        if (currHP > maxHP) { currHP = maxHP; }
		if (currHP <= 0)
		{
			// destruction stuff
			EndLife(); // remove when animations implemented
		}
    }
	
	public void takeDamage(int damage, CharacterController enemCC)
	{
		if (!invincible)
		{
			currHP -= damage;
			if (enemIndex != -1)
			{
				if (enemIndex > -1) { EnemyDamageTracker.UpdateDamage(enemIndex, 0, damage); }
				CharacterController cc = GetComponent<CharacterController>();
				if (GetComponent<EnemyBehavior>() != null && GetComponent<KnockbackReceptor>() != null)
				{
					int dir = GetComponent<EnemyBehavior>().dir;
					float kb;
					kb = -5.0f * Mathf.Sign(cc.velocity.x);
					if (cc.velocity.x == 0) { kb = 5.0f * Mathf.Sign(enemCC.velocity.x); }
					if (Mathf.Sign(dir) == Mathf.Sign(kb)) { GetComponent<EnemyBehavior>().dir = -1*(int)Mathf.Sign(kb); }
					GetComponent<KnockbackReceptor>().addKB(new Vector3(kb, 0, 0));
				}
			}
			else
			{
				Animator anim = transform.Find("Visuals").GetComponent<Animator>();
				CharacterController cc = GetComponent<CharacterController>();
				int dir = transform.root.GetComponent<PlayerMovement>().dir;
				float kb;
				kb = -5.0f * Mathf.Sign(cc.velocity.x);
				if (cc.velocity.x == 0) { kb = 5.0f * Mathf.Sign(enemCC.velocity.x); }
				if (Mathf.Sign(dir) == Mathf.Sign(kb)) { transform.root.GetComponent<PlayerMovement>().dir = -1*(int)Mathf.Sign(kb); }
				GetComponent<KnockbackReceptor>().addKB(new Vector3(kb, 0, 0));
				anim.SetBool("damaged", true);
				invincible = true;
				invuln_timer = 2.0f;
			}
		}
	}
	
	public void scoreDamage(int damage)
	{
		if (enemIndex > 0)
		{
			EnemyDamageTracker.UpdateDamage(enemIndex, damage, 0);
		}
	}
	
	public void EndLife() // written as a script method for use in Animations
	{
		if (enemIndex >= 0)
		{
			Destroy(gameObject);
		}
		else
		{
			if (rc != null) { rc.reinit(); }
		}
	}
}
