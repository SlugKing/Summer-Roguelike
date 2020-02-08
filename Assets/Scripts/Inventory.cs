using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Inventory
{
	private static IList<int[]> convoy;
	// items stored as int[]s with properties when necessary
	private static IList<int[]> equipped;
	private static Sprite[] item_sprite_array;
	//private static IList<int> equippedInventoryIndices;
	
	public static IList<int[]> Convoy
	{
		get { return convoy; }
		set { convoy = value; }
	}
	
	public static IList<int[]> Equipped
	{
		get { return equipped; }
		set { equipped = value; }
	}
	
	public static int MaxEquipSize
	{
		get { return 2; }
	}
	
	public static Sprite Item_Sprites(int index)
	{
		if (item_sprite_array == null) { item_sprite_array = Resources.LoadAll<Sprite>("item_sheet"); }
		return item_sprite_array[index];
	}
	
	public static int[] GetEquippedItem(int index)
	{
		return equipped[index];
	}
	
	public static void EquipItem(int index)
	{
		//if (equippedInventoryIndices == null) { equippedInventoryIndices = new List<int>(); }
		//equippedInventoryIndices.Add(index);
		equipped.Add(convoy[index]);
	}
	
	public static int[] GetConvoyItem(int index)
	{
		return convoy[index];
	}
	
	public static void AddConvoyItem(int[] item)
	{
		convoy.Add(item);
	}
	
	public static void UnequipItem(int index)
	{
		equipped.Remove(convoy[index]);
	}
	
	public static void UnequipAll()
	{
		equipped.Clear();
	}
}
