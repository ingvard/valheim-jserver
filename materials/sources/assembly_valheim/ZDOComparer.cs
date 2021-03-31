using System;
using System.Collections.Generic;

// Token: 0x02000076 RID: 118
internal class ZDOComparer : IEqualityComparer<ZDO>
{
	// Token: 0x06000757 RID: 1879 RVA: 0x0003AC31 File Offset: 0x00038E31
	public bool Equals(ZDO a, ZDO b)
	{
		return a == b;
	}

	// Token: 0x06000758 RID: 1880 RVA: 0x0003AC37 File Offset: 0x00038E37
	public int GetHashCode(ZDO a)
	{
		return a.GetHashCode();
	}
}
