using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
	public GameObject player;
	private Vector3 newCamPos;
	private Vector3 roomPos;
	private RoomCreator rc;
	private float height, length;
    // Start is called before the first frame update
    void Start()
    {
        rc = GameObject.Find("DungeonGenerator").GetComponent<RoomCreator>();
		height = rc.roomHeight;
		length = rc.roomLength;
		
    }

    // Update is called once per frame
    void Update()
    {
		float factor_x, factor_y;
		if (player.transform.position.x != 0) { factor_x = player.transform.position.x / Mathf.Abs(player.transform.position.x); }
		else { factor_x = 1.0f; }
		if (player.transform.position.y != 0) { factor_y = player.transform.position.y / Mathf.Abs(player.transform.position.y); }
		else { factor_y = 1.0f; }
		roomPos = new Vector3((int)((player.transform.position.x + (length*factor_x/2.0f)) / length)*length, (int)((player.transform.position.y + (height*factor_y/2.0f)) / height)*height, 0);
		//Debug.Log((player.transform.position.x - (length/2)) / length);
        newCamPos = transform.position + new Vector3((roomPos.x-transform.position.x), (roomPos.y-transform.position.y), 0);
		transform.position += (newCamPos-transform.position)*Time.deltaTime*3.5f;
    }
}
