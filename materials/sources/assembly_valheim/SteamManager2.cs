using System;
using System.Text;
using Steamworks;
using UnityEngine;

// Token: 0x02000094 RID: 148
[DisallowMultipleComponent]
public class SteamManager2 : MonoBehaviour
{
	// Token: 0x17000020 RID: 32
	// (get) Token: 0x060009E7 RID: 2535 RVA: 0x00047D11 File Offset: 0x00045F11
	protected static SteamManager2 Instance
	{
		get
		{
			if (SteamManager2.s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager2>();
			}
			return SteamManager2.s_instance;
		}
	}

	// Token: 0x17000021 RID: 33
	// (get) Token: 0x060009E8 RID: 2536 RVA: 0x00047D35 File Offset: 0x00045F35
	public static bool Initialized
	{
		get
		{
			return SteamManager2.Instance.m_bInitialized;
		}
	}

	// Token: 0x060009E9 RID: 2537 RVA: 0x00047A95 File Offset: 0x00045C95
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	// Token: 0x060009EA RID: 2538 RVA: 0x00047D44 File Offset: 0x00045F44
	protected virtual void Awake()
	{
		if (SteamManager2.s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		SteamManager2.s_instance = this;
		if (SteamManager2.s_EverInitialized)
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
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
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
		SteamManager2.s_EverInitialized = true;
	}

	// Token: 0x060009EB RID: 2539 RVA: 0x00047E18 File Offset: 0x00046018
	protected virtual void OnEnable()
	{
		if (SteamManager2.s_instance == null)
		{
			SteamManager2.s_instance = this;
		}
		if (!this.m_bInitialized)
		{
			return;
		}
		if (this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager2.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	// Token: 0x060009EC RID: 2540 RVA: 0x00047E66 File Offset: 0x00046066
	protected virtual void OnDestroy()
	{
		if (SteamManager2.s_instance != this)
		{
			return;
		}
		SteamManager2.s_instance = null;
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.Shutdown();
	}

	// Token: 0x060009ED RID: 2541 RVA: 0x00047E8A File Offset: 0x0004608A
	protected virtual void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
	}

	// Token: 0x040008FD RID: 2301
	protected static bool s_EverInitialized;

	// Token: 0x040008FE RID: 2302
	protected static SteamManager2 s_instance;

	// Token: 0x040008FF RID: 2303
	protected bool m_bInitialized;

	// Token: 0x04000900 RID: 2304
	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
