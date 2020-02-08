using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClusterManager
{
    private static IList<int[]> roomlist;
	private static IList<float[]> enemylist;
	
	public static IList<int[]> RoomHistory
	{
		get { return roomlist; }
		set { roomlist = value; }
	}
	public static IList<float[]> EnemyHistory
	{
		get { return enemylist; }
		set { enemylist = value; }
	}
	public static void AppendRooms(IList<int[]> rooms) // ignores the first room
	{
		for (int i = 1; i < rooms.Count; i++)
		{
			roomlist.Add(rooms[i]);
		}
	}
	public static void AppendEnemies(IList<float[]> enemies)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemylist.Add(enemies[i]);
		}
	}
}
