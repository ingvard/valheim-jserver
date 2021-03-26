using System;
using UnityEngine;

// Token: 0x02000104 RID: 260
public class TestSceneSetup : MonoBehaviour
{
	// Token: 0x06000F9A RID: 3994 RVA: 0x0006E604 File Offset: 0x0006C804
	private void Awake()
	{
		WorldGenerator.Initialize(World.GetMenuWorld());
	}

	// Token: 0x06000F9B RID: 3995 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Update()
	{
	}
}
