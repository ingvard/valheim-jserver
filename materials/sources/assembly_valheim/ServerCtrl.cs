using System;
using System.IO;
using UnityEngine;

// Token: 0x020000AC RID: 172
public class ServerCtrl
{
	// Token: 0x1700002D RID: 45
	// (get) Token: 0x06000BB4 RID: 2996 RVA: 0x000539C1 File Offset: 0x00051BC1
	public static ServerCtrl instance
	{
		get
		{
			return ServerCtrl.m_instance;
		}
	}

	// Token: 0x06000BB5 RID: 2997 RVA: 0x000539C8 File Offset: 0x00051BC8
	public static void Initialize()
	{
		if (ServerCtrl.m_instance == null)
		{
			ServerCtrl.m_instance = new ServerCtrl();
		}
	}

	// Token: 0x06000BB6 RID: 2998 RVA: 0x000539DB File Offset: 0x00051BDB
	private ServerCtrl()
	{
		this.ClearExitFile();
	}

	// Token: 0x06000BB7 RID: 2999 RVA: 0x000539E9 File Offset: 0x00051BE9
	public void Update(float dt)
	{
		this.CheckExit(dt);
	}

	// Token: 0x06000BB8 RID: 3000 RVA: 0x000539F2 File Offset: 0x00051BF2
	private void CheckExit(float dt)
	{
		this.m_checkTimer += dt;
		if (this.m_checkTimer > 2f)
		{
			this.m_checkTimer = 0f;
			if (File.Exists("server_exit.drp"))
			{
				Application.Quit();
			}
		}
	}

	// Token: 0x06000BB9 RID: 3001 RVA: 0x00053A2C File Offset: 0x00051C2C
	private void ClearExitFile()
	{
		try
		{
			File.Delete("server_exit.drp");
		}
		catch
		{
		}
	}

	// Token: 0x04000AEB RID: 2795
	private static ServerCtrl m_instance;

	// Token: 0x04000AEC RID: 2796
	private float m_checkTimer;
}
