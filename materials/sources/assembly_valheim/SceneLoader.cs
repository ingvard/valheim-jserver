using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000AB RID: 171
public class SceneLoader : MonoBehaviour
{
	// Token: 0x06000BB1 RID: 2993 RVA: 0x00053990 File Offset: 0x00051B90
	private void Start()
	{
		base.StartCoroutine(this.LoadYourAsyncScene());
	}

	// Token: 0x06000BB2 RID: 2994 RVA: 0x0005399F File Offset: 0x00051B9F
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

	// Token: 0x04000AEA RID: 2794
	public string m_scene = "";
}
