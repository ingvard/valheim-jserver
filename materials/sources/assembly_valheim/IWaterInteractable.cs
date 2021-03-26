using System;
using UnityEngine;

// Token: 0x020000CE RID: 206
public interface IWaterInteractable
{
	// Token: 0x06000D57 RID: 3415
	bool IsOwner();

	// Token: 0x06000D58 RID: 3416
	void SetInWater(float waterLevel);

	// Token: 0x06000D59 RID: 3417
	Transform GetTransform();
}
