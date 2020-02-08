using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyDamageTracker
{
	private static IList<int[]> damagecache;
	
    public static IList<int[]> EnemyDamageCache
	{
		get { return damagecache; }
		set { damagecache = value; }
	}
	
	public static int AddEnemy()
	{
		int[] enem = new int[] { 0, 0 };
		damagecache.Add(enem);
		return damagecache.Count-1;
	}
	
	public static void UpdateDamage(int index, int damageDone, int damageTaken)
	{
		damagecache[index][0] += damageDone;
		damagecache[index][1] += damageTaken;
	}
	
	public static int GetDamageDone(int index)
	{
		return damagecache[index][0];
	}
	
	public static int GetDamageTaken(int index)
	{
		return damagecache[index][1];
	}
}
