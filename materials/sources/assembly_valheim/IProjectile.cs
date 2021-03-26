using System;
using UnityEngine;

// Token: 0x0200001C RID: 28
public interface IProjectile
{
	// Token: 0x060002FD RID: 765
	void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item);

	// Token: 0x060002FE RID: 766
	string GetTooltipString(int itemQuality);
}
