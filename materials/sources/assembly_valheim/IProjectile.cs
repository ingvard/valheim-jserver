using System;
using UnityEngine;

// Token: 0x0200001C RID: 28
public interface IProjectile
{
	// Token: 0x060002FE RID: 766
	void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item);

	// Token: 0x060002FF RID: 767
	string GetTooltipString(int itemQuality);
}
