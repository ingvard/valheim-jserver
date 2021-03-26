using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AE RID: 174
public class SlowUpdater : MonoBehaviour
{
	// Token: 0x06000BBF RID: 3007 RVA: 0x0005397F File Offset: 0x00051B7F
	private void Awake()
	{
		base.StartCoroutine("UpdateLoop");
	}

	// Token: 0x06000BC0 RID: 3008 RVA: 0x0005398D File Offset: 0x00051B8D
	private IEnumerator UpdateLoop()
	{
		for (;;)
		{
			List<SlowUpdate> instances = SlowUpdate.GetAllInstaces();
			int index = 0;
			while (index < instances.Count)
			{
				int num = 0;
				while (num < 100 && instances.Count != 0 && index < instances.Count)
				{
					instances[index].SUpdate();
					int num2 = index + 1;
					index = num2;
					num++;
				}
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			instances = null;
		}
		yield break;
	}

	// Token: 0x04000AE9 RID: 2793
	private const int m_updatesPerFrame = 100;
}
