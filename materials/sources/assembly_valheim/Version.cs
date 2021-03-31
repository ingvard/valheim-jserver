using System;
using UnityEngine;

// Token: 0x020000B4 RID: 180
internal class Version
{
	// Token: 0x06000BF3 RID: 3059 RVA: 0x0005537A File Offset: 0x0005357A
	public static string GetVersionString()
	{
		return global::Version.CombineVersion(global::Version.m_major, global::Version.m_minor, global::Version.m_patch);
	}

	// Token: 0x06000BF4 RID: 3060 RVA: 0x00055390 File Offset: 0x00053590
	public static bool IsVersionNewer(int major, int minor, int patch)
	{
		if (major > global::Version.m_major)
		{
			return true;
		}
		if (major == global::Version.m_major && minor > global::Version.m_minor)
		{
			return true;
		}
		if (major != global::Version.m_major || minor != global::Version.m_minor)
		{
			return false;
		}
		if (global::Version.m_patch >= 0)
		{
			return patch > global::Version.m_patch;
		}
		return patch >= 0 || patch < global::Version.m_patch;
	}

	// Token: 0x06000BF5 RID: 3061 RVA: 0x000553EC File Offset: 0x000535EC
	public static string CombineVersion(int major, int minor, int patch)
	{
		if (patch == 0)
		{
			return major.ToString() + "." + minor.ToString();
		}
		if (patch < 0)
		{
			return string.Concat(new string[]
			{
				major.ToString(),
				".",
				minor.ToString(),
				".rc",
				Mathf.Abs(patch).ToString()
			});
		}
		return string.Concat(new string[]
		{
			major.ToString(),
			".",
			minor.ToString(),
			".",
			patch.ToString()
		});
	}

	// Token: 0x06000BF6 RID: 3062 RVA: 0x00055494 File Offset: 0x00053694
	public static bool IsWorldVersionCompatible(int version)
	{
		if (version == global::Version.m_worldVersion)
		{
			return true;
		}
		foreach (int num in global::Version.m_compatibleWorldVersions)
		{
			if (version == num)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000BF7 RID: 3063 RVA: 0x000554CC File Offset: 0x000536CC
	public static bool IsPlayerVersionCompatible(int version)
	{
		if (version == global::Version.m_playerVersion)
		{
			return true;
		}
		foreach (int num in global::Version.m_compatiblePlayerVersions)
		{
			if (version == num)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x04000B1C RID: 2844
	public static int m_major = 0;

	// Token: 0x04000B1D RID: 2845
	public static int m_minor = 148;

	// Token: 0x04000B1E RID: 2846
	public static int m_patch = 7;

	// Token: 0x04000B1F RID: 2847
	public static int m_playerVersion = 33;

	// Token: 0x04000B20 RID: 2848
	public static int[] m_compatiblePlayerVersions = new int[]
	{
		32,
		31,
		30,
		29,
		28,
		27
	};

	// Token: 0x04000B21 RID: 2849
	public static int m_worldVersion = 26;

	// Token: 0x04000B22 RID: 2850
	public static int[] m_compatibleWorldVersions = new int[]
	{
		25,
		24,
		23,
		22,
		21,
		20,
		19,
		18,
		17,
		16,
		15,
		14,
		13,
		11,
		10,
		9
	};

	// Token: 0x04000B23 RID: 2851
	public static int m_worldGenVersion = 1;
}
