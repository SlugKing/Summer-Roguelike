using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCollider : MonoBehaviour
{
	public float aggregateTime;
	private bool isColliding;
	private Color invisible, visible, halfvisible, intermediary;
	public Transform cover;
	public Transform surroundings;
	private float fadeTime;
	
    // Start is called before the first frame update
    void Start()
    {
        aggregateTime = 0;
		cover = transform.Find("Void");
		surroundings = transform.Find("Surroundings");
		visible = cover.GetComponent<Renderer>().material.color;
		invisible = cover.GetComponent<Renderer>().material.color;
		halfvisible = cover.GetComponent<Renderer>().material.color;
		intermediary = cover.GetComponent<Renderer>().material.color;
		invisible.a = 0;
		halfvisible.a = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
		if (fadeTime < 0.5f) { fadeTime += Time.deltaTime; }
		if (isColliding)
		{
			aggregateTime += Time.deltaTime;
			if (cover.GetComponent<Renderer>().material.color.a != invisible.a)
			{
				cover.GetComponent<Renderer>().material.color = Color.Lerp(intermediary, invisible, fadeTime / (0.5f*(intermediary.a/1.0f)));
				//surroundings.GetComponent<Renderer>().material.color = Color.Lerp(visible, halfvisible, fadeTime / 0.5f);
			}
		}
		else if (!isColliding && cover.GetComponent<Renderer>().material.color.a != visible.a)
		{
			cover.GetComponent<Renderer>().material.color = Color.Lerp(intermediary, visible, fadeTime / (0.5f*((1.0f-intermediary.a)/1.0f)));
			//surroundings.GetComponent<Renderer>().material.color = Color.Lerp(halfvisible, visible, fadeTime / 0.5f);
		}
    }
	
	void OnTriggerEnter(Collider other)
	{
		if (other.name.Contains("Bait"))
		{
			isColliding = true;
			fadeTime = 0;
			intermediary = cover.GetComponent<Renderer>().material.color;
		}
		
	}
	
	void OnTriggerExit(Collider other)
	{
		if (other.name.Contains("Bait"))
		{
			isColliding = false;
			fadeTime = 0;
			intermediary = cover.GetComponent<Renderer>().material.color;
		}
		
	}
	
}
