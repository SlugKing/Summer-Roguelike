using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveset : MonoBehaviour
{
	private bool hitboxActive;
	public GameObject hitbox;
	private IList<GameObject> activeHitboxes;
	public bool attacking;
	public bool moveDisabled;
	private Animator anim;
	public int equippedActiveIndex;
	public GameObject[] weaponPrefabs;
	private IList<GameObject> equippedWeapons;
	
    // Start is called before the first frame update
    void Start()
    {
        activeHitboxes = new List<GameObject>();
		anim = GetComponent<Animator>();
		equippedActiveIndex = 0;
		Transform arm = transform.Find("leftarm_dagger");
		equippedWeapons = new List<GameObject>();
		for (int i = 0; i < Inventory.Equipped.Count; i++)
		{
			GameObject weapon = Instantiate(weaponPrefabs[Inventory.Equipped[i][0]], new Vector3(0, 0.08f, 0), Quaternion.identity, arm);
			if (i != equippedActiveIndex) { weapon.SetActive(false); }
			else { weapon.SetActive(true); }
			equippedWeapons.Add(weapon);
		}
		
    }

    // Update is called once per frame
    void Update()
    {
		if (!attacking)
		{
			if (Input.GetKeyDown("t"))
			{
				anim.SetBool("defaultAttack", true);
			}
			if (Input.GetKeyDown("y"))
			{
				if (equippedActiveIndex < equippedWeapons.Count-1) { equippedActiveIndex++; }
				else { equippedActiveIndex = 0; }
			}
		}
		for (int i = 0; i < equippedWeapons.Count; i++)
		{
			if (i != equippedActiveIndex) { equippedWeapons[i].SetActive(false); }
			else { equippedWeapons[i].SetActive(true); }
		}
    }
	
	public void defaultAttack() // hitbox-creation, called as an Animation Event. do not call directly through code except as debug
	{
		//Vector3 boxPos = transform.TransformPoint(new Vector3(1.0f*GetComponent<PlayerMovement>().dir, 0, 0)); // 1 unit in front of player
		//Vector3 halfExtents = new Vector3(0.25f, 0.25f, 0.25f);
		Vector3 localPos = new Vector3(0.75f, 0, 0);
		Vector3 localScl = new Vector3(1.0f*transform.parent.GetComponent<PlayerMovement>().dir, 1.0f, 1.0f);
		Vector3 hbScale = new Vector3(0.5f, 1.0f, 1.0f);
		GameObject hb = Instantiate(hitbox, transform, false);
		hb.transform.localPosition = localPos;
		hb.transform.localScale = localScl;
		hb.GetComponent<BoxCollider>().size = hbScale;
		hb.GetComponent<HitboxDetails>().damage = Inventory.Equipped[equippedActiveIndex][1];
		hb.GetComponent<HitboxDetails>().persistent = false;
		activeHitboxes.Add(hb);
	}
	
	public void endDefaultAttack() { anim.SetBool("defaultAttack", false); }
	
	public void purgeHitboxes()
	{
		foreach (GameObject o in activeHitboxes)
		{
			Destroy(o);
		}
		activeHitboxes.Clear();
	}
	
	public void allowAttacks()
	{
		attacking = false;
		anim.SetBool("attacking", false);
	}
	
	public void disableAttacks()
	{
		attacking = true;
		anim.SetBool("attacking", true);
	}
	
	public void AllowActionDamaged() // stops damaged animation
	{
		anim.SetBool("damaged", false);
	}
	
	public void DisableMovement()
	{
		moveDisabled = true;
	}
	
	public void EnableMovement()
	{
		moveDisabled = false;
	}
}
