using System;

// Token: 0x020000C2 RID: 194
public interface IDestructible
{
	// Token: 0x06000CD5 RID: 3285
	void Damage(HitData hit);

	// Token: 0x06000CD6 RID: 3286
	DestructibleType GetDestructibleType();
}
