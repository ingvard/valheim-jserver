using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000113 RID: 275
public class WearNTearUpdater : MonoBehaviour
{
	// Token: 0x06001039 RID: 4153 RVA: 0x000727C1 File Offset: 0x000709C1
	private void Awake()
	{
		base.StartCoroutine("UpdateWear");
	}

	// Token: 0x0600103A RID: 4154 RVA: 0x000727CF File Offset: 0x000709CF
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

	// Token: 0x04000F23 RID: 3875
	private const int m_updatesPerFrame = 50;
}
