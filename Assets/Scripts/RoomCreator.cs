using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class RoomCreator : MonoBehaviour
{
	private IList<int[]> roomCache = new List<int[]>();
	private IList<int[]> enemyCache = new List<int[]>();
	private IList<GameObject> colliderList = new List<GameObject>();
	private float esize = 1.0f;
	private int[] roomsize; // x, y
	public GameObject[] elements; // list of elements - DO NOT TOUCH THIS
	public static int elementCount = 5; // number of elements possible, including empty
	public float[] elementWeight; // weights of elements - DO NOT TOUCH THIS
	public GameObject[] enemies; // list of enemies - DO NOT TOUCH THIS
	public static int enemyTypeCount = 1; // number of enemies
	public static int enemyAttributeCount = 5; // number of enemy attributes
	// below are masks specific to the element list, read right to left for indices
	public static int solid_top_mask = 0b00010;
	public static int solid_bottom_mask = 0b11010;
	public static int solid_left_mask = 0b01010;
	public static int solid_right_mask = 0b10010;
	public static int requires_right_mask = 0b10000;
	public static int requires_left_mask = 0b01000;
	public static int requires_bottom_mask = 0b00000;
	public static int requires_top_mask = 0b00000;
	public static int requires_empty_right_mask = 0b01000;
	public static int requires_empty_left_mask = 0b10000;
	public static int requires_empty_top_mask = 0b11100;
	public static int requires_empty_bottom_mask = 0b00000;
	private int maxdepth = 5;
	public int roomLength = 17;
	public int roomHeight = 13;
	public GameObject kmeansSample;
	public GameObject roomcollider;
	public GameObject backdrop;
	public GameObject player;
	public GameObject clusterTester;
	
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
		*/
		EnemyDamageTracker.EnemyDamageCache = new List<int[]>();
		roomsize = new int[2];
		if (ClusterManager.RoomHistory != null && ClusterManager.EnemyHistory != null)
		{
			IList<int[]> roomHistory = ClusterManager.RoomHistory;
			IList<float[]> enemyHistory = ClusterManager.EnemyHistory;
			roomHistory.OrderBy(room => room[elementCount]);
			
			float[][] roomClusterCache = new float[roomHistory.Count][];
			for (int i = 0; i < roomHistory.Count; i++)
			{
				roomClusterCache[i] = new float[elementCount];
				for (int j = 0; j < elementCount; j++)
				{
					roomClusterCache[i][j] = (float)roomHistory[i][j];
				}
			}
			// enemy sorting here.
			enemyHistory.OrderBy(enemy => enemy[enemyAttributeCount]);
			float[][] enemyClusterCache = new float[enemyHistory.Count][];
			for (int i = 0; i < enemyHistory.Count; i++)
			{
				enemyClusterCache[i] = new float[enemyAttributeCount];
				for (int j = 0; j < enemyAttributeCount; j++)
				{
					enemyClusterCache[i][j] = enemyHistory[i][j];
				}
			}
			
			// debug history display
			/*
			for (int i = 0; i < enemyHistory.Count; i++)
			{
				GameObject test = Instantiate(clusterTester, new Vector3(3.0f, -3.0f + 0.25f*i, -15.6f), Quaternion.identity);
				//Debug.Log(enemyHistory[i][1]);
				test.transform.Find("Plane").GetComponent<Renderer>().material.SetColor("_Color", new Color((float)enemyHistory[i][1]/255.0f, (float)enemyHistory[i][2]/255.0f, (float)enemyHistory[i][3]/255.0f));
			}
			*/
			
			float[][][] roomClusterPackage = kMeans(roomClusterCache, 3, elementCount); // currently set to three clusters
			float[][][] enemyClusterPackage = kMeans(enemyClusterCache, 3, enemyAttributeCount); // currently set to three clusters
			int clusterIndChosen = (int)roomClusterPackage[1][ClusterManager.RoomHistory.Count-1][0]; // choosing the hardest cluster
			int enemClusterIndChosen = (int)enemyClusterPackage[1][ClusterManager.EnemyHistory.Count-1][0]; // hardest cluster
			
			// debug cluster display
			for (int i = 0; i < enemyClusterPackage[0].Length; i++)
			{
				GameObject test = Instantiate(clusterTester, new Vector3(3.0f, -3.0f + 0.25f*i, -15.6f), Quaternion.identity);
				test.transform.Find("Plane").GetComponent<Renderer>().material.SetColor("_Color", new Color((float)enemyClusterPackage[0][i][1]/255.0f, (float)enemyClusterPackage[0][i][2]/255.0f, (float)enemyClusterPackage[0][i][3]/255.0f));
			}
			
			int[] clusterTilePool = new int[elementCount];
			for (int i = 0; i < elementCount; i++)
			{
				clusterTilePool[i] = (int)roomClusterPackage[0][clusterIndChosen][i];
			}
			
			int[] enemClusterModel = new int[enemyAttributeCount];
			for (int i = 0; i < enemyAttributeCount; i++)
			{
				enemClusterModel[i] = (int)enemyClusterPackage[0][enemClusterIndChosen][i];
			}
			
			string str = "";
			foreach (int i in enemClusterModel) { str += i.ToString(); str += " ";}
			Debug.Log(str);
			
			GenerateDungeon(clusterTilePool, enemClusterModel);
			
			//string str = "";
			//foreach (float f in roomClusters[0]) { str += f.ToString(); str += " ";}
			//Debug.Log(str);
		}
		else { GenerateDungeon(); }
		
		//int[,] sampleRoom = new int[,] {{1, 1, 1, 1}, {1, 0, 0, 1}, {1, 0, 0, 1}, {1, 1, 1, 1}};
		//int[,] sampleRoom = new int[,] {{1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 3, 3, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 3, 3, 0, 0, 3, 3, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}};
		//InstantiateRoom(sampleRoom);
		//DungeonRoom sampleRoom = new DungeonRoom(0, 0, 10, 7, 0);
		//GenerateDungeon();
		//InstantiateRoom(sampleRoom);
		
		/*float[][] enemyInfo = new float[6][];
		enemyInfo[0] = new float[6]{ 0, 0, 0, 0, 0, 0 };
		enemyInfo[1] = new float[6]{ 0, 0, 0, 0, 0, 0 };
		enemyInfo[2] = new float[6]{ 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
		enemyInfo[3] = new float[6]{ 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
		enemyInfo[4] = new float[6]{ 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f };
		enemyInfo[5] = new float[6]{ 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f };
		
		enemyInfo[0] = new float[4]{1.0f, 1.0f, 1.0f, 1.0f};
		enemyInfo[1] = new float[4]{0.5f, 0.5f, 0.5f, 0.5f};
		enemyInfo[2] = new float[4]{0.8f, 0.5f, 1.0f, 1.0f};
		enemyInfo[3] = new float[4]{0.4f, 0.25f, 0.5f, 0.2f};
		enemyInfo[4] = new float[4]{0.7f, 0.25f, 1.0f, 0.1f};
		enemyInfo[5] = new float[4]{0.6f, 0.3f, 0.5f, 0.2f};
		
		
		for (int i = 0; i < 6; i++)
		{
			GameObject center = Instantiate(kmeansSample, new Vector3(-6.0f + (2.0f*i), 2.0f, -2.0f), Quaternion.identity, gameObject.transform);
			center.GetComponent<Renderer>().material.SetColor("_Color", new Color(enemyInfo[i][1], enemyInfo[i][2], enemyInfo[i][3]));
			center.transform.localScale = new Vector3(enemyInfo[i][0], enemyInfo[i][0], enemyInfo[i][0]);
		}
		
		
		float[][] clusters = kMeans(enemyInfo, 3, 4);
		
		//Debug.Log("cluster 0 = " +string.Join("",
        //     clusters[0]
        //     .ConvertAll(i => i.ToString())
        //     .ToArray()));
		/*string str = "";
		foreach (float f in clusters[0]) { str += f.ToString(); str += " ";}
		Debug.Log(str);
		str = "";
		foreach (float f in clusters[1]) { str += f.ToString(); str += " ";}
		Debug.Log(str); */
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown("g"))
		{
			reinit();
		}
    }
	
	void GenerateDungeon(int[] cluster = null, int[] enemcluster = null)
	{
		IList<Vector2> coords = new List<Vector2>();
		Stack<DungeonRoom> roomstack = new Stack<DungeonRoom>();
		DungeonRoom startRoom = new DungeonRoom(roomLength, roomHeight);
		coords.Add(new Vector2(0.0f, 0.0f));
		roomstack.Push(startRoom);
		
		while (roomstack.Count > 0)
		{
			DungeonRoom found = roomstack.Pop();
			int prob = 10-(roomCache.Count()/2);
			if (Random.Range(0, 10) < prob && found.depth < maxdepth && !coords.Contains(new Vector2(found.x-(esize*roomLength), found.y)))
			{
				DungeonRoom leftroom = new DungeonRoom(found.x-(esize*roomLength), found.y, roomLength, roomHeight, found.depth+1, cluster, enemcluster);
				found.left = leftroom;
				leftroom.right = found;
				roomstack.Push(leftroom);
				coords.Add(new Vector2(found.x-(esize*roomLength), found.y));
			}
			if (Random.Range(0, 10) < prob && found.depth < maxdepth && !coords.Contains(new Vector2(found.x+(esize*roomLength), found.y)))
			{
				DungeonRoom rightroom = new DungeonRoom(found.x+(esize*roomLength), found.y, roomLength, roomHeight, found.depth+1, cluster, enemcluster);
				found.right = rightroom;
				rightroom.left = found;
				roomstack.Push(rightroom);
				coords.Add(new Vector2(found.x+(esize*roomLength), found.y));
			}
			if (Random.Range(0, 10) < prob && found.depth < maxdepth && !coords.Contains(new Vector2(found.x, found.y+(esize*roomHeight))))
			{
				DungeonRoom uproom = new DungeonRoom(found.x, found.y+(esize*roomHeight), roomLength, roomHeight, found.depth+1, cluster, enemcluster);
				found.up = uproom;
				uproom.down = found;
				roomstack.Push(uproom);
				coords.Add(new Vector2(found.x, found.y+(esize*roomHeight)));
			}
			if (found.depth > 0 && Random.Range(0, 10) < prob && found.depth < maxdepth && !coords.Contains(new Vector2(found.x, found.y-(esize*roomHeight))))
			{
				DungeonRoom downroom = new DungeonRoom(found.x, found.y-(esize*roomHeight), roomLength, roomHeight, found.depth+1, cluster, enemcluster);
				found.down = downroom;
				downroom.up = found;
				roomstack.Push(downroom);
				coords.Add(new Vector2(found.x, found.y-(esize*roomHeight)));
			}
			found.FixEntrances();
			InstantiateRoom(found);
			GameObject collider = Instantiate(roomcollider, new Vector3(found.x, found.y, 0), Quaternion.identity, gameObject.transform); // room collider for found room
			collider.GetComponent<RoomCollider>().cover = collider.transform.Find("Void");
			collider.GetComponent<RoomCollider>().surroundings = collider.transform.Find("Surroundings");
			collider.transform.localScale = new Vector3(1.0f*roomLength, 1.0f*roomHeight, 1.0f);
			collider.GetComponent<RoomCollider>().surroundings.localScale = new Vector3(1.0f + (1.0f/(float)roomLength), 1.0f + (1.0f/(float)roomHeight), 1.0f);
			colliderList.Add(collider);
			int[] roomByElem = new int[elementCount+1];
			for (int i = 0; i < elementCount; i++) { roomByElem[i] = 0; }
			for (int i = 1; i < roomLength-1; i++) // tallying for room regeneration and clustering purposes
			{
				for (int j = 1; j < roomHeight-1; j++)
				{
					roomByElem[found.layout[j, i]]++;
				}
			}
			roomByElem[0] = 0; // nulling out air space
			roomByElem[elementCount] = 0; // used for time metrics
			roomCache.Add(roomByElem);
		}
	}
	
	void InstantiateRoom(DungeonRoom room)
	{
		roomsize[0] = room.layout.GetLength(1);
		roomsize[1] = room.layout.GetLength(0);
		
		// element size is currently: 1.0
		// top left coord = -elementsize * roomsize[x]/2
		
		for (int j = 0; j < roomsize[1]; j++)
		{
			for (int i = 0; i < roomsize[0]; i++)
			{
				if (room.layout[j, i] != 0)
				{
					GameObject elem = Instantiate(elements[room.layout[j, i]-1], new Vector3(esize*i - esize*(roomsize[0]-1)*0.5f + room.x, -esize*j + esize*(roomsize[1]-1)*0.5f + room.y, 0), Quaternion.identity, gameObject.transform);
				}
				if (room.enemylayout[j, i] != -1)
				{
					int enemIndex = room.enemylayout[j, i];
					int[] enemDetails = room.enemies[enemIndex];
					GameObject enem = Instantiate(enemies[enemDetails[0]], new Vector3(esize*i - esize*(roomsize[0]-1)*0.5f + room.x, -esize*j + esize*(roomsize[1]-1)*0.5f + room.y, 0), Quaternion.identity, gameObject.transform);
					enem.transform.Find("Colorable").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color((float)enemDetails[1]/255.0f, (float)enemDetails[2]/255.0f, (float)enemDetails[3]/255.0f));
					enem.GetComponent<EnemyBehavior>().MoveMult = (float)enemDetails[4] / 2.0f;
					enem.GetComponent<EnemyBehavior>().activationRange = esize*Mathf.Max(roomsize[0], roomsize[1]);
					Physics.IgnoreCollision(player.GetComponent<CharacterController>(), enem.GetComponent<CharacterController>(), true);
					enem.GetComponent<Entity>().enemIndex = EnemyDamageTracker.AddEnemy();
					int[] enemInstance = new int[enemyAttributeCount+1];
					for (int k = 0; k < enemyAttributeCount; k++)
					{
						enemInstance[k] = enemDetails[k];
					}
					enemInstance[enemyAttributeCount] = enem.GetComponent<Entity>().enemIndex;
					enemyCache.Add(enemInstance);
				}
			}
		}
		
		GameObject bg = Instantiate(backdrop, new Vector3(room.x, room.y, 5.0f), Quaternion.Euler(-90, 0, 0), gameObject.transform);
		bg.transform.localScale = new Vector3(roomsize[0]*0.1f, 1.0f, roomsize[1]*0.1f);
	}
	
	
	public class DungeonRoom
	{
		public int[,] layout;
		public int[,] enemylayout;
		public int[][] enemies;
		public float x, y;
		public DungeonRoom left, right, up, down;
		public int depth;
		
		
		public DungeonRoom(int length, int height) // constructor for just the starting room
		{
			this.x = 0;
			this.y = 0;
			this.depth = 0;
			layout = new int[height, length];
			enemylayout = new int[height, length];
			
			// set up the walls
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (j == 0 || j == height-1) { layout[j, i] = 1; } // [HARD WALL REFERENCE]
					else if (i == 0 || i == length-1) { layout[j, i] = 1; } // [HARD WALL REFERENCE]
					
				}
			}
			
			layout[(height/3), (length/2)] = 2; // top plat
			layout[(height/3), (length/2)-1] = 2;
			layout[(height/3), (length/2)+1] = 2;
			layout[(2*height/3), (length/3)-1] = 2; // left plat
			layout[(2*height/3), (length/3)-2] = 2;
			layout[(2*height/3), (length/3)] = 2;
			layout[(2*height/3), (2*length/3)] = 2; // right plat
			layout[(2*height/3), (2*length/3)-1] = 2;
			layout[(2*height/3), (2*length/3)+1] = 2;
			
			for (int i = 0; i < length; i++) // initialize the enemy layout. -1 corresponds to no enemy
			{
				for (int j = 0; j < height; j++)
				{
					enemylayout[j, i] = -1;
				}
			}
			
		}
		
		public DungeonRoom(float x, float y, int length, int height, int depth, int[] cluster = null, int[] enemcluster = null) // constructor for a new room.
		{
			this.x = x;
			this.y = y;
			this.depth = depth;
			layout = new int[height, length];
			int[] tilepool = new int[elementCount];
			enemylayout = new int[height, length];
			if (cluster == null)
			{
				for (int i = 0; i < elementCount; i++) { tilepool[i] = 0; }
				for (int i = 0; i < 0.1*height*length; i++) // generate the tile pool. currently set to 10% of the room
				{
					tilepool[Random.Range(1, elementCount)]++; // spawn a random element.
				}
			}
			else
			{
				for (int i = 0; i < elementCount; i++)
				{
					tilepool[i] = cluster[i];
				}
			}
			
			// number of enemies
			int numEnem = Random.Range(0, (int)Mathf.Sqrt(Mathf.Sqrt((height-2)*(length-2)))+1);
			if (enemcluster == null)
			{
				
				// initialize enemies
				if (numEnem > 0)
				{
					enemies = new int[numEnem][];
					for (int i = 0; i < numEnem; i++)
					{
						enemies[i] = new int[] {
							Random.Range(0, enemyTypeCount), // enemy type
							Random.Range(0, 256), // red
							Random.Range(0, 256), // green
							Random.Range(0, 256), // blue
							Random.Range(2, 6) // movement speed
						};
					}
				}
			}
			else
			{
				if (numEnem > 0)
				{
					enemies = new int[numEnem][];
					for (int i = 0; i < numEnem; i++)
					{
						int randomfactor = Random.Range(0, 2);
						if (randomfactor == 0)
						{
							enemies[i] = new int[enemyAttributeCount];
							for (int j = 0; j < enemyAttributeCount; j++)
							{
								enemies[i][j] = enemcluster[j];
							}
						}
						else if (randomfactor == 1)
						{
							enemies[i] = new int[] {
								Random.Range(0, enemyTypeCount), // enemy type
								Random.Range(0, 256), // red
								Random.Range(0, 256), // green
								Random.Range(0, 256), // blue
								Random.Range(2, 6) // movement speed
							};
						}
					}
				}
			}
			
			// set up the walls
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (j == 0 || j == height-1) { layout[j, i] = 1; } // [HARD WALL REFERENCE]
					else if (i == 0 || i == length-1) { layout[j, i] = 1; } // [HARD WALL REFERENCE]
					/*else
					{
						int empty = Random.Range(0, 5);
						if (empty == 0) { layout[j, i] = Random.Range(1, elementCount); }
						else { layout[j, i] = 0; }
					}
					*/
				}
			}
			
			INDGeneration(tilepool, height, length);
			
			// ENEMY PLACEMENT
			// dumps the list of enemies onto empty tiles, not including entrances/exits.
			for (int i = 0; i < length; i++) // initialize the enemy layout. -1 corresponds to no enemy
			{
				for (int j = 0; j < height; j++)
				{
					enemylayout[j, i] = -1;
				}
			}
			for (int i = 0; i < numEnem; i++)
			{
				int enX, enY;
				do
				{
					enX = Random.Range(1, length);
					enY = Random.Range(1, height);
				}
				while (layout[enY, enX] != 0);
				enemylayout[enY, enX] = i;
			}
		}
		
		public void FixEntrances()
		{
			int[] tilepool = new int[elementCount];
			for (int i = 0; i < elementCount; i++) { tilepool[i] = 0; }
			int length = layout.GetLength(1);
			int height = layout.GetLength(0);
			// Y, X
			if (left != null)
			{
				//tilepool[layout[height/2, 0]]++;
				layout[height/2, 0] = 0;
				tilepool[layout[height/2, 1]]++;
				layout[height/2, 1] = 0;
				//tilepool[layout[(height/2)-1, 0]]++;
				layout[(height/2)-1, 0] = 0;
				tilepool[layout[(height/2)-1, 1]]++;
				layout[(height/2)-1, 1] = 0;
				//tilepool[layout[(height/2)+1, 0]]++;
				layout[(height/2)+1, 0] = 0;
				tilepool[layout[(height/2)+1, 1]]++;
				layout[(height/2)+1, 1] = 0;
			}
			if (right != null)
			{
				//tilepool[layout[height/2, length-1]]++;
				layout[height/2, length-1] = 0;
				tilepool[layout[height/2, length-2]]++;
				layout[height/2, length-2] = 0;
				//tilepool[layout[(height/2)-1, length-1]]++;
				layout[(height/2)-1, length-1] = 0;
				tilepool[layout[(height/2)-1, length-2]]++;
				layout[(height/2)-1, length-2] = 0;
				//tilepool[layout[(height/2)+1, length-1]]++;
				layout[(height/2)+1, length-1] = 0;
				tilepool[layout[(height/2)+1, length-2]]++;
				layout[(height/2)+1, length-2] = 0;
			}
			if (up != null)
			{
				//tilepool[layout[0, length/2]]++;
				layout[0, length/2] = 0;
				tilepool[layout[1, length/2]]++;
				layout[1, length/2] = 0;
				//tilepool[layout[0, (length/2)-1]]++;
				layout[0, (length/2)-1] = 0;
				tilepool[layout[1, (length/2)-1]]++;
				layout[1, (length/2)-1] = 0;
				//tilepool[layout[0, (length/2)+1]]++;
				layout[0, (length/2)+1] = 0;
				tilepool[layout[1, (length/2)+1]]++;
				layout[1, (length/2)+1] = 0;
			}
			if (down != null)
			{
				//tilepool[layout[height-1, length/2]]++;
				layout[height-1, length/2] = 0;
				tilepool[layout[height-2, length/2]]++;
				layout[height-2, length/2] = 0;
				//tilepool[layout[height-1, (length/2)-1]]++;
				layout[height-1, (length/2)-1] = 0;
				tilepool[layout[height-2, (length/2)-1]]++;
				layout[height-2, (length/2)-1] = 0;
				//tilepool[layout[height-1, (length/2)+1]]++;
				layout[height-1, (length/2)+1] = 0;
				tilepool[layout[height-2, (length/2)+1]]++;
				layout[height-2, (length/2)+1] = 0;
			}
			tilepool[0] = 0;
			if (depth != 0) { INDGeneration(tilepool, height, length); }
		}
		
		public bool TestMask(int test, int mask)
		{
			return ((((1 << test) & mask) >> test) % 2 == 1);
		}
		
		public void INDGeneration(int[] tilepool, int height, int length) // Iterative Non-Destructive Generation
		{
			int[] empty = new int[elementCount];
			for (int i = 0; i < elementCount; i++) { empty[i] = 0; }
			int[] previous = new int[elementCount];
			for (int i = 0; i < elementCount; i++) { previous[i] = 0; }
			while (!tilepool.SequenceEqual(empty) && !tilepool.SequenceEqual(previous)) // iterative element placement, now without total deletion
			{
				// should break if the current tilepool is the same as the previous, meaning the block could not be placed
				for (int i = 0; i < elementCount; i++) { previous[i] = tilepool[i]; } // setting previous
				int[] previous_placement = new int[elementCount];
				for (int i = 0; i < elementCount; i++) { previous_placement[i] = 0; }
				while (!tilepool.SequenceEqual(empty) && !tilepool.SequenceEqual(previous_placement)) // second layer of previous to prevent infinite loop with unplaceables
				{
					for (int i = 0; i < elementCount; i++) { previous_placement[i] = tilepool[i]; } // setting previous
					for (int i = 0; i < elementCount; i++)
					{
						if (tilepool[i] != 0)
						{
							int tx = Random.Range(1, length-1);
							int ty = Random.Range(1, height-1);
							while (layout[ty, tx] != 0) // finding an empty space
							{
								tx = Random.Range(1, length-1);
								ty = Random.Range(1, height-1);
							}
							
							// tests for placing blocks. !requires || has - fails iff it requires and doesn't have
							if ((!TestMask(i, requires_left_mask) || TestMask(layout[ty, tx-1], solid_right_mask)) && // requires solid left: block to left has a solid right edge
								(!TestMask(i, requires_right_mask) || TestMask(layout[ty, tx+1], solid_left_mask)) && // requires solid right: block to right has a solid left edge
								(!TestMask(i, requires_top_mask) || TestMask(layout[ty-1, tx], solid_bottom_mask)) && // requires solid top: block above has a solid bottom edge
								(!TestMask(i, requires_bottom_mask) || TestMask(layout[ty+1, tx], solid_top_mask)) && // requires solid bottom: block below has a solid top edge
								(!TestMask(i, requires_empty_left_mask) || !TestMask(layout[ty, tx-1], solid_right_mask)) && // requires empty left: block to left has empty right edge
								(!TestMask(i, requires_empty_right_mask) || !TestMask(layout[ty, ty+1], solid_left_mask)) && // requires empty right: block to right has empty left edge
								(!TestMask(i, requires_empty_top_mask) || !TestMask(layout[ty-1, tx], solid_bottom_mask)) && // requires empty top: block above has empty bottom edge
								(!TestMask(i, requires_empty_bottom_mask) || !TestMask(layout[ty+1, tx], solid_top_mask)) && // requires empty bottom: block below has empty top edge
								(!TestMask(layout[ty, tx+1], requires_empty_left_mask) || !TestMask(i, solid_right_mask)) && // unobstructing right: does not obstruct a no-left block to right
								(!TestMask(layout[ty, tx-1], requires_empty_right_mask) || !TestMask(i, solid_left_mask)) && // unobstructing left: does not obstruct a no-right block to left
								(!TestMask(layout[ty-1, tx], requires_empty_bottom_mask) || !TestMask(i, solid_top_mask)) && // unobstructing top: does not obstruct a no-bottom block above
								(!TestMask(layout[ty+1, tx], requires_empty_top_mask) || !TestMask(i, solid_bottom_mask)) // unobstructing bottom: does not obstruct a no-top block below
								)
								{
									layout[ty, tx] = i;
									tilepool[i]--;
								} // good to go, place block!
						}
					}
				}
				
				
				// SPECIAL RULES SECTION
				// ignoring walls for changes
				// j for vertical changes, i for horizontal changes
				for (int i = 1; i < length-1; i++)
				{
					for (int j = 1; j < height-1; j++)
					{
						/*
						bool solid_top_test = (((1 << layout[j, i]) & solid_top_mask) >> layout[j, i]) % 2 == 1;
						bool solid_bottom_test = (((1 << layout[j, i]) & solid_bottom_mask) >> layout[j, i]) % 2 == 1;
						bool solid_left_test = (((1 << layout[j, i]) & solid_left_mask) >> layout[j, i]) % 2 == 1;
						bool solid_right_test = (((1 << layout[j, i]) & solid_right_mask) >> layout[j, i]) % 2 == 1;
						*/
						
						/*
						// if a platform is below or immediately on top of a solid block, delete it
						if (layout[j, i] == 2 &&
						((((1 << layout[j+1, i]) & solid_top_mask) >> layout[j+1, i]) % 2 == 1 ||
						(((1 << layout[j-1, i]) & solid_bottom_mask) >> layout[j-1, i]) % 2 == 1)) { layout[j, i] = 0; }
						*/
						
						// if an empty space is surrounded by hard blocks, fill it
						if (layout[j, i] == 0 &&
						(((1 << layout[j-1, i]) & solid_top_mask) >> layout[j-1, i]) % 2 == 1 &&
						(((1 << layout[j+1, i]) & solid_bottom_mask) >> layout[j+1, i]) % 2 == 1 &&
						(((1 << layout[j, i+1]) & solid_left_mask) >> layout[j, i+1]) % 2 == 1 &&
						(((1 << layout[j, i-1]) & solid_right_mask) >> layout[j, i-1]) % 2 == 1)
						{
							tilepool[layout[j, i]] += 1;
							layout[j, i] = 1; // [HARD WALL REFERENCE]
							
						}
					}
				}
			}
		}
	}
	
	public void reinit() // temporary cleanup
	{
		for (int i = 0; i < roomCache.Count; i++)
		{
			roomCache[i][elementCount] = (int)(colliderList[i].GetComponent<RoomCollider>().aggregateTime); // updating rooms with the times collected
		}
		IList<float[]> enemyCacheData = new List<float[]>();
		for (int i = 0; i < enemyCache.Count; i++)
		{
			/*
			Color c = enemyCache[i].transform.Find("Colorable").gameObject.GetComponent<Renderer>().material.color;
			int index = enemyCache[i].GetComponent<Entity>().enemIndex;
			enemyCacheData.Add(new float[] {
				enemyCache[i].GetComponent<EnemyBehavior>().enemType,
				c.r * 255,
				c.g * 255,
				c.b * 255,
				enemyCache[i].GetComponent<EnemyBehavior>().MoveMult*2,
				(float)(EnemyDamageTracker.GetDamageDone(index) - EnemyDamageTracker.GetDamageTaken(index))
			});
			*/
			int index = enemyCache[i][enemyAttributeCount];
			float[] enemyReprocessed = new float[enemyAttributeCount+1];
			for (int j = 0; j < enemyAttributeCount; j++)
			{
				enemyReprocessed[j] = enemyCache[i][j];
			}
			// extra attributes: damage factor (done - taken)
			enemyReprocessed[enemyAttributeCount] = (float)(EnemyDamageTracker.GetDamageDone(index) - EnemyDamageTracker.GetDamageTaken(index)); // overwriting the index, we don't need anymore
			enemyCacheData.Add(enemyReprocessed);
		}
		if (ClusterManager.RoomHistory == null) { ClusterManager.RoomHistory = new List<int[]>(); }
		if (ClusterManager.EnemyHistory == null) { ClusterManager.EnemyHistory = new List<float[]>(); }
		ClusterManager.AppendRooms(roomCache); // this will ignore the first room
		ClusterManager.AppendEnemies(enemyCacheData);
		SceneManager.LoadScene("between-runs");
	}
	
	
	// concept: using bitmasks for "solid" block definition on given sides
	// 1 << [element number] for the test, and a predefined sequence for the definitions
	// if test & sequence >> [element number] % 2 == 0, it's not solid / doesn't fit the definition
	
	// here's how i could use k means to generate rooms.
	// each room is represented by a tally of each kind of element it's composed of.
	// these tallies are weighted based on player stats (multiply higher for having difficulty, etc)
	// dump into k-means and generate the cluster centers, which are represented as weighted room element tallies
	// when generating an actual room, pick one of these centers and use the tallies to bias the generation towards or away from features
	
	// before plugging something into the k-means, make sure it's sorted by difficulty, using the learned metrics
	
	// todo: add custom weighting to the distance formula
	
	// outputs a 3d jagged array with the following:
	// return[0]: cluster centers, a 2d jagged array with first dim. the index and second dim. the data
	// return[1]: indices for input data, a 2d jagged array with first dim. the index per input datum and second dim. the corresponding index for return[0]
	float[][][] kMeans(float[][] enemyInfo, int k, int numDims)
	{
		int enemyCount = enemyInfo.Length; // could be 1 if i fucked up dimensions
		
		// random initial assignment
		// set up evenly divided cluster sizes
		int[] initClustTotals = new int[k];
		for (int i = 0; i < k; i++) { initClustTotals[i] = enemyCount/k; }
		for (int i = 0; i < enemyCount % k; i++) { initClustTotals[i]++; }
		
		int[] enClustInd = new int[enemyCount];
		int[] clustTotals = new int[k];
		for (int i = 0; i < enemyCount; i++) // assign random clusters to each enemy point
		{
			// if this doesnt work well, just assign it to i % k
			enClustInd[i] = Random.Range(0, k);
			while (initClustTotals[enClustInd[i]] <= 0) { enClustInd[i] = Random.Range(0, k); } // making sure we're picking from a cluster we can
			clustTotals[enClustInd[i]]++; // increment total for cluster
			initClustTotals[enClustInd[i]]--; // decrement remaining picks for cluster
		}
		
		// compute cluster centers
		float[][] clustCenters = new float[k][]; // k by number of dimensions to data
		for (int i = 0; i < k; i++)
		{
			clustCenters[i] = new float[numDims];
		}
		for (int i = 0; i < enemyCount; i++)
		{
			//clustCenters[enClustInd[i]] += enemyInfo[i];
			// have to do this stupid rewriting of summing two arrays smh
			float[] summed = new float[numDims];
			for (int j = 0; j < numDims; j++)
			{
				summed[j] = clustCenters[enClustInd[i]][j] + enemyInfo[i][j];
			}
			clustCenters[enClustInd[i]] = summed;
		}
		for (int i = 0; i < k; i++)
		{
			//clustCenters[i] /= clustTotals[i];
			// move stupid rewriting
			float[] divCenter = new float[numDims];
			for (int j = 0; j < numDims; j++)
			{
				divCenter[j] = clustCenters[i][j] / clustTotals[i];
			}
			clustCenters[i] = divCenter;
		}
		
		// reiterate
		for (int i = 0; i < 10; i++) // currently 3, change for iterations
		{
			// resetting cluster totals
			for (int j = 0; j < k; j++)
			{
				clustTotals[j] = 0;
			}
			// compute distance from each enemy to cluster centers
			// reassign each enemy to closest cluster
			for (int j = 0; j < enemyCount; j++)
			{
				float[] distances = new float[k];
				int lowDistInd = 0;
				for (int l = 0; l < k; l++) // compute distance per enemy to each cluster
				{
					//distances[l] = Mathf.Abs(enemyInfo[j]-clustCenters[l]).Sum();
					float[] elemDist = new float[numDims];
					for (int m = 0; m < numDims; m++)
					{
						elemDist[m] = Mathf.Abs(enemyInfo[j][m]-clustCenters[l][m]);
					}
					distances[l] = 0;
					foreach (float f in elemDist)
					{
						distances[l] += f;
					}
					if (distances[l] < distances[lowDistInd]) { lowDistInd = l; }
				}
				enClustInd[j] = lowDistInd;
				clustTotals[lowDistInd]++;
			}
			
			// recompute cluster centers like above
			clustCenters = new float[k][]; // k by number of dimensions to data
			for (int j = 0; j < k; j++) // initialize each cluster
			{
				clustCenters[j] = new float[numDims];
			}
			for (int j = 0; j < enemyCount; j++)
			{
				//clustCenters[enClustInd[i]] += enemyInfo[i];
				// have to do this stupid rewriting of summing two arrays smh
				float[] summed = new float[numDims];
				for (int l = 0; l < numDims; l++)
				{
					summed[l] = clustCenters[enClustInd[j]][l] + enemyInfo[j][l];
				}
				clustCenters[enClustInd[j]] = summed;
			}
			for (int j = 0; j < k; j++)
			{
				//clustCenters[i] /= clustTotals[i];
				// move stupid rewriting
				float[] divCenter = new float[numDims];
				for (int l = 0; l < numDims; l++)
				{
					divCenter[l] = clustCenters[j][l] / clustTotals[j];
				}
				clustCenters[j] = divCenter;
			}
		}
		// by the end, assuming the original list of enemies was sorted from easiest to hardest, enClustInd's last, first, or middle indexes will
		// suggest which ones are easy, medium, or hard
		
		/*
		string str = "";
		foreach (int i in enClustInd) { str += i.ToString(); str += " ";}
		Debug.Log(str);
		*/
		
		
		/*for (int i = 0; i < k; i++)
		{
			GameObject center = Instantiate(kmeansSample, new Vector3(-4.0f + (2.0f*i), 0, -2.0f), Quaternion.identity, gameObject.transform);
			center.GetComponent<Renderer>().material.SetColor("_Color", new Color(clustCenters[i][1], clustCenters[i][2], clustCenters[i][3]));
			center.transform.localScale = new Vector3(clustCenters[i][0], clustCenters[i][0], clustCenters[i][0]);
		}
		*/
		
		
		// return the cluster center
		float[][][] package = new float[2][][];
		package[0] = clustCenters;
		float[][] ind = new float[enClustInd.Length][];
		for (int i = 0; i < enClustInd.Length; i++) { ind[i] = new float[] { (float)enClustInd[i] }; }
		package[1] = ind;
		return package;
	}

	
}


