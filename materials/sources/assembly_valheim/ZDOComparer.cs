using System;
using System.Collections.Generic;

// Token: 0x02000076 RID: 118
internal class ZDOComparer : IEqualityComparer<ZDO>
{
	// Token: 0x06000756 RID: 1878 RVA: 0x0003AB7D File Offset: 0x00038D7D
	public bool Equals(ZDO a, ZDO b)
	{
		return a == b;
	}

	// Token: 0x06000757 RID: 1879 RVA: 0x0003AB83 File Offset: 0x00038D83
	public int GetHashCode(ZDO a)
	{
		return a.GetHashCode();
	}
}
