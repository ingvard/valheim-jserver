using System;
using System.IO;
using UnityEngine;

// Token: 0x020000AC RID: 172
public class ServerCtrl
{
	// Token: 0x1700002D RID: 45
	// (get) Token: 0x06000BB3 RID: 2995 RVA: 0x00053839 File Offset: 0x00051A39
	public static ServerCtrl instance
	{
		get
		{
			return ServerCtrl.m_instance;
		}
	}

	// Token: 0x06000BB4 RID: 2996 RVA: 0x00053840 File Offset: 0x00051A40
	public static void Initialize()
	{
		if (ServerCtrl.m_instance == null)
		{
			ServerCtrl.m_instance = new ServerCtrl();
		}
	}

	// Token: 0x06000BB5 RID: 2997 RVA: 0x00053853 File Offset: 0x00051A53
	private ServerCtrl()
	{
		this.ClearExitFile();
	}

	// Token: 0x06000BB6 RID: 2998 RVA: 0x00053861 File Offset: 0x00051A61
	public void Update(float dt)
	{
		this.CheckExit(dt);
	}

	// Token: 0x06000BB7 RID: 2999 RVA: 0x0005386A File Offset: 0x00051A6A
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

	// Token: 0x06000BB8 RID: 3000 RVA: 0x000538A4 File Offset: 0x00051AA4
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

	// Token: 0x04000AE5 RID: 2789
	private static ServerCtrl m_instance;

	// Token: 0x04000AE6 RID: 2790
	private float m_checkTimer;
}
