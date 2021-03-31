using System;
using UnityEngine;

// Token: 0x02000104 RID: 260
public class TestSceneSetup : MonoBehaviour
{
	// Token: 0x06000F9B RID: 3995 RVA: 0x0006E78C File Offset: 0x0006C98C
	private void Awake()
	{
		WorldGenerator.Initialize(World.GetMenuWorld());
	}

	// Token: 0x06000F9C RID: 3996 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Update()
	{
	}
}
