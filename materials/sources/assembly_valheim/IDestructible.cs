using System;

// Token: 0x020000C2 RID: 194
public interface IDestructible
{
	// Token: 0x06000CD6 RID: 3286
	void Damage(HitData hit);

	// Token: 0x06000CD7 RID: 3287
	DestructibleType GetDestructibleType();
}
