using System;
using System.IO;
using System.Linq;
using System.Text;
using Steamworks;
using UnityEngine;

// Token: 0x02000093 RID: 147
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	// Token: 0x1700001E RID: 30
	// (get) Token: 0x060009DC RID: 2524 RVA: 0x00047AFB File Offset: 0x00045CFB
	public static SteamManager instance
	{
		get
		{
			return SteamManager.s_instance;
		}
	}

	// Token: 0x060009DD RID: 2525 RVA: 0x00047B02 File Offset: 0x00045D02
	public static bool Initialize()
	{
		if (SteamManager.s_instance == null)
		{
			new GameObject("SteamManager").AddComponent<SteamManager>();
		}
		return SteamManager.Initialized;
	}

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x060009DE RID: 2526 RVA: 0x00047B26 File Offset: 0x00045D26
	public static bool Initialized
	{
		get
		{
			return SteamManager.s_instance != null && SteamManager.s_instance.m_bInitialized;
		}
	}

	// Token: 0x060009DF RID: 2527 RVA: 0x00047B41 File Offset: 0x00045D41
	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	// Token: 0x060009E0 RID: 2528 RVA: 0x00047B49 File Offset: 0x00045D49
	public static void SetServerPort(int port)
	{
		SteamManager.m_serverPort = port;
	}

	// Token: 0x060009E1 RID: 2529 RVA: 0x00047B54 File Offset: 0x00045D54
	private uint LoadAPPID()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SteamAppId");
		if (environmentVariable != null)
		{
			ZLog.Log("Using environment steamid " + environmentVariable);
			return uint.Parse(environmentVariable);
		}
		try
		{
			string s = File.ReadAllText("steam_appid.txt");
			ZLog.Log("Using steam_appid.txt");
			return uint.Parse(s);
		}
		catch
		{
		}
		ZLog.LogWarning("Failed to find APPID");
		return 0U;
	}

	// Token: 0x060009E2 RID: 2530 RVA: 0x00047BC4 File Offset: 0x00045DC4
	private void Awake()
	{
		if (SteamManager.s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		SteamManager.s_instance = this;
		SteamManager.APP_ID = this.LoadAPPID();
		ZLog.Log("Using steam APPID:" + SteamManager.APP_ID.ToString());
		if (!SteamManager.ACCEPTED_APPIDs.Contains(SteamManager.APP_ID))
		{
			ZLog.Log("Invalid APPID");
			Application.Quit();
			return;
		}
		if (SteamManager.s_EverInialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary((AppId_t)SteamManager.APP_ID))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException arg)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + arg, this);
			Application.Quit();
			return;
		}
		this.m_bInitialized = SteamAPI.Init();
		if (!this.m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		ZLog.Log("Authentication:" + SteamNetworkingSockets.InitAuthentication().ToString());
		SteamManager.s_EverInialized = true;
	}

	// Token: 0x060009E3 RID: 2531 RVA: 0x00047D04 File Offset: 0x00045F04
	private void OnEnable()
	{
		if (SteamManager.s_instance == null)
		{
			SteamManager.s_instance = this;
		}
		if (!this.m_bInitialized)
		{
			return;
		}
		if (this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	// Token: 0x060009E4 RID: 2532 RVA: 0x00047D52 File Offset: 0x00045F52
	private void OnDestroy()
	{
		ZLog.Log("Steam manager on destroy");
		if (SteamManager.s_instance != this)
		{
			return;
		}
		SteamManager.s_instance = null;
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.Shutdown();
	}

	// Token: 0x060009E5 RID: 2533 RVA: 0x00047D80 File Offset: 0x00045F80
	private void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
	}

	// Token: 0x040008FA RID: 2298
	public static uint[] ACCEPTED_APPIDs = new uint[]
	{
		1223920U,
		892970U
	};

	// Token: 0x040008FB RID: 2299
	public static uint APP_ID = 0U;

	// Token: 0x040008FC RID: 2300
	private static int m_serverPort = 2456;

	// Token: 0x040008FD RID: 2301
	private static SteamManager s_instance;

	// Token: 0x040008FE RID: 2302
	private static bool s_EverInialized;

	// Token: 0x040008FF RID: 2303
	private bool m_bInitialized;

	// Token: 0x04000900 RID: 2304
	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
