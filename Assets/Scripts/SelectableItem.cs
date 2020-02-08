using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableItem : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
	private UnityEngine.UI.Selectable sc;
	public int index;
	private bool equipped;
    // Start is called before the first frame update
    void Start()
    {
        sc = GetComponent<UnityEngine.UI.Selectable>();
		equipped = false;
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	public void OnPointerDown(PointerEventData eventData) {}
	
	public void OnPointerUp(PointerEventData data)
	{
		if (!equipped)
		{
			if (Inventory.Equipped != null && Inventory.Equipped.Count < Inventory.MaxEquipSize)
			{
				Inventory.EquipItem(index);
				equipped = true;
				sc.interactable = false;
			}
		}
		else
		{
			equipped = false;
			sc.interactable = true;
			Inventory.UnequipItem(index);
		}
	}
}
