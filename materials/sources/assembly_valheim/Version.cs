using System;
using UnityEngine;

// Token: 0x020000B4 RID: 180
internal class Version
{
	// Token: 0x06000BF2 RID: 3058 RVA: 0x000551F2 File Offset: 0x000533F2
	public static string GetVersionString()
	{
		return global::Version.CombineVersion(global::Version.m_major, global::Version.m_minor, global::Version.m_patch);
	}

	// Token: 0x06000BF3 RID: 3059 RVA: 0x00055208 File Offset: 0x00053408
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

	// Token: 0x06000BF4 RID: 3060 RVA: 0x00055264 File Offset: 0x00053464
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

	// Token: 0x06000BF5 RID: 3061 RVA: 0x0005530C File Offset: 0x0005350C
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

	// Token: 0x06000BF6 RID: 3062 RVA: 0x00055344 File Offset: 0x00053544
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

	// Token: 0x04000B16 RID: 2838
	public static int m_major = 0;

	// Token: 0x04000B17 RID: 2839
	public static int m_minor = 148;

	// Token: 0x04000B18 RID: 2840
	public static int m_patch = 6;

	// Token: 0x04000B19 RID: 2841
	public static int m_playerVersion = 33;

	// Token: 0x04000B1A RID: 2842
	public static int[] m_compatiblePlayerVersions = new int[]
	{
		32,
		31,
		30,
		29,
		28,
		27
	};

	// Token: 0x04000B1B RID: 2843
	public static int m_worldVersion = 26;

	// Token: 0x04000B1C RID: 2844
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

	// Token: 0x04000B1D RID: 2845
	public static int m_worldGenVersion = 1;
}
