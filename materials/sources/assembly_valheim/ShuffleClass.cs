using System;
using System.Collections.Generic;

// Token: 0x0200007E RID: 126
internal static class ShuffleClass
{
	// Token: 0x0600080A RID: 2058 RVA: 0x0003EC50 File Offset: 0x0003CE50
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

	// Token: 0x0400080D RID: 2061
	private static Random rng = new Random();
}
