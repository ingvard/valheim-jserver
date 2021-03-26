using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

// Token: 0x02000096 RID: 150
public class DLCMan : MonoBehaviour
{
	// Token: 0x17000023 RID: 35
	// (get) Token: 0x06000A08 RID: 2568 RVA: 0x00048B1E File Offset: 0x00046D1E
	public static DLCMan instance
	{
		get
		{
			return DLCMan.m_instance;
		}
	}

	// Token: 0x06000A09 RID: 2569 RVA: 0x00048B25 File Offset: 0x00046D25
	private void Awake()
	{
		DLCMan.m_instance = this;
		this.CheckDLCsSTEAM();
	}

	// Token: 0x06000A0A RID: 2570 RVA: 0x00048B33 File Offset: 0x00046D33
	private void OnDestroy()
	{
		if (DLCMan.m_instance == this)
		{
			DLCMan.m_instance = null;
		}
	}

	// Token: 0x06000A0B RID: 2571 RVA: 0x00048B48 File Offset: 0x00046D48
	public bool IsDLCInstalled(string name)
	{
		if (name.Length == 0)
		{
			return true;
		}
		foreach (DLCMan.DLCInfo dlcinfo in this.m_dlcs)
		{
			if (dlcinfo.m_name == name)
			{
				return dlcinfo.m_installed;
			}
		}
		ZLog.LogWarning("DLC " + name + " not registered in DLCMan");
		return false;
	}

	// Token: 0x06000A0C RID: 2572 RVA: 0x00048BD0 File Offset: 0x00046DD0
	private void CheckDLCsSTEAM()
	{
		if (!SteamManager.Initialized)
		{
			ZLog.Log("Steam not initialized");
			return;
		}
		ZLog.Log("Checking for installed DLCs");
		foreach (DLCMan.DLCInfo dlcinfo in this.m_dlcs)
		{
			dlcinfo.m_installed = this.IsDLCInstalled(dlcinfo);
			ZLog.Log("DLC:" + dlcinfo.m_name + " installed:" + dlcinfo.m_installed.ToString());
		}
	}

	// Token: 0x06000A0D RID: 2573 RVA: 0x00048C6C File Offset: 0x00046E6C
	private bool IsDLCInstalled(DLCMan.DLCInfo dlc)
	{
		foreach (uint id in dlc.m_steamAPPID)
		{
			if (this.IsDLCInstalled(id))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000A0E RID: 2574 RVA: 0x00048CA0 File Offset: 0x00046EA0
	private bool IsDLCInstalled(uint id)
	{
		AppId_t x = new AppId_t(id);
		int dlccount = SteamApps.GetDLCCount();
		for (int i = 0; i < dlccount; i++)
		{
			AppId_t appId_t;
			bool flag;
			string text;
			if (SteamApps.BGetDLCDataByIndex(i, out appId_t, out flag, out text, 200) && x == appId_t)
			{
				ZLog.Log("DLC installed:" + id);
				return SteamApps.BIsDlcInstalled(appId_t);
			}
		}
		return false;
	}

	// Token: 0x0400092D RID: 2349
	private static DLCMan m_instance;

	// Token: 0x0400092E RID: 2350
	public List<DLCMan.DLCInfo> m_dlcs = new List<DLCMan.DLCInfo>();

	// Token: 0x02000179 RID: 377
	[Serializable]
	public class DLCInfo
	{
		// Token: 0x04001191 RID: 4497
		public string m_name = "DLC";

		// Token: 0x04001192 RID: 4498
		public uint[] m_steamAPPID = new uint[0];

		// Token: 0x04001193 RID: 4499
		[NonSerialized]
		public bool m_installed;
	}
}
