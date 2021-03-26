using System;
using System.Collections.Generic;

// Token: 0x0200007E RID: 126
internal static class ShuffleClass
{
	// Token: 0x06000809 RID: 2057 RVA: 0x0003EB9C File Offset: 0x0003CD9C
	public static void Shuffle<T>(this IList<T> list)
	{
		int i = list.Count;
		while (i > 1)
		{
			i--;
			int index = ShuffleClass.rng.Next(i + 1);
			T value = list[index];
			list[index] = list[i];
			list[i] = value;
		}
	}

	// Token: 0x04000809 RID: 2057
	private static Random rng = new Random();
}
