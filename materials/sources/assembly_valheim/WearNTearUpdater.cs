using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000113 RID: 275
public class WearNTearUpdater : MonoBehaviour
{
	// Token: 0x06001038 RID: 4152 RVA: 0x00072639 File Offset: 0x00070839
	private void Awake()
	{
		base.StartCoroutine("UpdateWear");
	}

	// Token: 0x06001039 RID: 4153 RVA: 0x00072647 File Offset: 0x00070847
	private IEnumerator UpdateWear()
	{
		for (;;)
		{
			List<WearNTear> instances = WearNTear.GetAllInstaces();
			int index = 0;
			while (index < instances.Count)
			{
				int num = 0;
				while (num < 50 && instances.Count != 0 && index < instances.Count)
				{
					instances[index].UpdateWear();
					int num2 = index + 1;
					index = num2;
					num++;
				}
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			instances = null;
		}
		yield break;
	}

	// Token: 0x04000F1D RID: 3869
	private const int m_updatesPerFrame = 50;
}
