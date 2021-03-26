using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000AB RID: 171
public class SceneLoader : MonoBehaviour
{
	// Token: 0x06000BB0 RID: 2992 RVA: 0x00053808 File Offset: 0x00051A08
	private void Start()
	{
		base.StartCoroutine(this.LoadYourAsyncScene());
	}

	// Token: 0x06000BB1 RID: 2993 RVA: 0x00053817 File Offset: 0x00051A17
	private IEnumerator LoadYourAsyncScene()
	{
		ZLog.Log("Starting to load scene:" + this.m_scene);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(this.m_scene, LoadSceneMode.Single);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x04000AE4 RID: 2788
	public string m_scene = "";
}
