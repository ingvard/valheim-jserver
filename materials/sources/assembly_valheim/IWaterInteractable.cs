using System;
using UnityEngine;

// Token: 0x020000CE RID: 206
public interface IWaterInteractable
{
	// Token: 0x06000D58 RID: 3416
	bool IsOwner();

	// Token: 0x06000D59 RID: 3417
	void SetInWater(float waterLevel);

	// Token: 0x06000D5A RID: 3418
	Transform GetTransform();
}
