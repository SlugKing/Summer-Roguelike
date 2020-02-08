using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippedItemDisplay : MonoBehaviour
{
	public int index;
	private UnityEngine.UI.Image img;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<UnityEngine.UI.Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Inventory.Equipped != null)
		{
			if (Inventory.Equipped.Count > index)
			{
				GetComponent<UnityEngine.UI.Image>().sprite = Inventory.Item_Sprites(Inventory.Equipped[index][0]);
				img.enabled = true;
			}
			else
			{
				img.enabled = false;
			}
		}
    }
}
