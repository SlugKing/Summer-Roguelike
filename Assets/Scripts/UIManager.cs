using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public GameObject spritePrefab;
    // Start is called before the first frame update
    void Start()
    {
		/*
		if (Inventory.Convoy == null)
		{
			Inventory.Convoy = new List<int[]>();
		}
		if (Inventory.Equipped == null)
		{
			Inventory.Equipped = new List<int[]>();
			Inventory.AddEquippedItem(new int[] { 0 });
			Inventory.AddEquippedItem(new int[] { 1 });
		}
        for (int i = 0; i < Inventory.Equipped.Count; i++)
		{
			Debug.Log(Inventory.Item_Sprites(Inventory.Equipped[i][0]));
			GameObject sprite = Instantiate(spritePrefab, new Vector3(-2.0f, 1.0f-(0.5f*i), 0), Quaternion.identity);
			sprite.GetComponent<SpriteRenderer>().sprite = Inventory.Item_Sprites(Inventory.Equipped[i][0]);
		}
		*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
