using System;
using System.Text;
using Steamworks;
using UnityEngine;

// Token: 0x02000094 RID: 148
[DisallowMultipleComponent]
public class SteamManager2 : MonoBehaviour
{
	// Token: 0x17000020 RID: 32
	// (get) Token: 0x060009E8 RID: 2536 RVA: 0x00047DBD File Offset: 0x00045FBD
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
	// (get) Token: 0x060009E9 RID: 2537 RVA: 0x00047DE1 File Offset: 0x00045FE1
	public static bool Initialized
	{
		get
		{
			return SteamManager2.Instance.m_bInitialized;
		}
	}

	// Token: 0x060009EA RID: 2538 RVA: 0x00047B41 File Offset: 0x00045D41
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	// Token: 0x060009EB RID: 2539 RVA: 0x00047DF0 File Offset: 0x00045FF0
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

	// Token: 0x060009EC RID: 2540 RVA: 0x00047EC4 File Offset: 0x000460C4
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

	// Token: 0x060009ED RID: 2541 RVA: 0x00047F12 File Offset: 0x00046112
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

	// Token: 0x060009EE RID: 2542 RVA: 0x00047F36 File Offset: 0x00046136
	protected virtual void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
	}

	// Token: 0x04000901 RID: 2305
	protected static bool s_EverInitialized;

	// Token: 0x04000902 RID: 2306
	protected static SteamManager2 s_instance;

	// Token: 0x04000903 RID: 2307
	protected bool m_bInitialized;

	// Token: 0x04000904 RID: 2308
	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
