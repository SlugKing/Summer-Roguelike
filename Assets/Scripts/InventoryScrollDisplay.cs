using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScrollDisplay : MonoBehaviour
{
	public GameObject listitem;
	private string[] itemnames;
	
    // Start is called before the first frame update
    void Start()
    {
		if (Inventory.Convoy == null)
		{
			Inventory.Convoy = new List<int[]>();
			Inventory.AddConvoyItem(new int[] { 0, 5 });
			Inventory.AddConvoyItem(new int[] { 1, 10 });
			Inventory.AddConvoyItem(new int[] { 1, 10 });
			Inventory.AddConvoyItem(new int[] { 1, 10 });
			Inventory.AddConvoyItem(new int[] { 1, 10 });
			Inventory.AddConvoyItem(new int[] { 1, 10 });
			
		}
		if (Inventory.Equipped == null)
		{
			Inventory.Equipped = new List<int[]>();
			//Inventory.AddEquippedItem(new int[] { 0 });
			//Inventory.AddEquippedItem(new int[] { 1 });
		}
		Inventory.UnequipAll();
        /*for (int i = 0; i < Inventory.Equipped.Count; i++)
		{
			//Debug.Log(Inventory.Item_Sprites(Inventory.Equipped[i][0]));
			GameObject item = Instantiate(listitem, new Vector3(0, (0.25f*i), 0), Quaternion.identity, transform);
			item.transform.Find("Item Sprite").GetComponent<UnityEngine.UI.Image>().sprite = Inventory.Item_Sprites(Inventory.Equipped[i][0]);
			item.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = ItemName(Inventory.Equipped[i][0]);
		}
		*/
		for (int i = 0; i < Inventory.Convoy.Count; i++)
		{
			//Debug.Log(Inventory.Item_Sprites(Inventory.Convoy[i][0]));
			GameObject item = Instantiate(listitem, new Vector3(0, (0.25f*i), 0), Quaternion.identity, transform);
			item.transform.Find("Item Sprite").GetComponent<UnityEngine.UI.Image>().sprite = Inventory.Item_Sprites(Inventory.Convoy[i][0]);
			item.GetComponent<SelectableItem>().index = i;
			item.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = ItemName(Inventory.Convoy[i][0]);
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	private string ItemName(int index)
	{
		if (itemnames == null)
		{
			itemnames = new string[] {
				"Rusty Dagger",
				"Sickle"
			};
		}
		return itemnames[index];
	}
}
