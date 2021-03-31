using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AE RID: 174
public class SlowUpdater : MonoBehaviour
{
	// Token: 0x06000BC0 RID: 3008 RVA: 0x00053B07 File Offset: 0x00051D07
	private void Awake()
	{
		base.StartCoroutine("UpdateLoop");
	}

	// Token: 0x06000BC1 RID: 3009 RVA: 0x00053B15 File Offset: 0x00051D15
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

	// Token: 0x04000AEF RID: 2799
	private const int m_updatesPerFrame = 100;
}
