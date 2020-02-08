using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScenes : MonoBehaviour
{
	public void StartGameplayScene()
	{
		StartCoroutine(LoadGameplayASync());
	}
	
	IEnumerator LoadGameplayASync()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("gameplay");
		while (!asyncLoad.isDone)
        {
            yield return null;
        }
	}
}
