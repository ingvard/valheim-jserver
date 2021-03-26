using System;
using UnityEngine;

// Token: 0x02000058 RID: 88
public class Menu : MonoBehaviour
{
	// Token: 0x1700000C RID: 12
	// (get) Token: 0x0600057E RID: 1406 RVA: 0x0002F42A File Offset: 0x0002D62A
	public static Menu instance
	{
		get
		{
			return Menu.m_instance;
		}
	}

	// Token: 0x0600057F RID: 1407 RVA: 0x0002F431 File Offset: 0x0002D631
	private void Start()
	{
		Menu.m_instance = this;
		this.m_root.gameObject.SetActive(false);
	}

	// Token: 0x06000580 RID: 1408 RVA: 0x0002F44A File Offset: 0x0002D64A
	public static bool IsVisible()
	{
		return !(Menu.m_instance == null) && Menu.m_instance.m_hiddenFrames <= 2;
	}

	// Token: 0x06000581 RID: 1409 RVA: 0x0002F46C File Offset: 0x0002D66C
	private void Update()
	{
		if (Game.instance.IsShuttingDown())
		{
			this.m_root.gameObject.SetActive(false);
			return;
		}
		if (this.m_root.gameObject.activeSelf)
		{
			this.m_hiddenFrames = 0;
			if ((Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")) && !this.m_settingsInstance && !Feedback.IsVisible())
			{
				if (this.m_quitDialog.gameObject.activeSelf)
				{
					this.OnQuitNo();
					return;
				}
				if (this.m_logoutDialog.gameObject.activeSelf)
				{
					this.OnLogoutNo();
					return;
				}
				this.m_root.gameObject.SetActive(false);
				return;
			}
		}
		else
		{
			this.m_hiddenFrames++;
			bool flag = !InventoryGui.IsVisible() && !Minimap.IsOpen() && !global::Console.IsVisible() && !TextInput.IsVisible() && !ZNet.instance.InPasswordDialog() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible();
			if ((Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")) && flag)
			{
				Gogan.LogEvent("Screen", "Enter", "Menu", 0L);
				this.m_root.gameObject.SetActive(true);
				this.m_menuDialog.gameObject.SetActive(true);
				this.m_logoutDialog.gameObject.SetActive(false);
				this.m_quitDialog.gameObject.SetActive(false);
			}
		}
	}

	// Token: 0x06000582 RID: 1410 RVA: 0x0002F5E3 File Offset: 0x0002D7E3
	public void OnSettings()
	{
		Gogan.LogEvent("Screen", "Enter", "Settings", 0L);
		this.m_settingsInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_settingsPrefab, base.transform);
	}

	// Token: 0x06000583 RID: 1411 RVA: 0x0002F612 File Offset: 0x0002D812
	public void OnQuit()
	{
		this.m_quitDialog.gameObject.SetActive(true);
		this.m_menuDialog.gameObject.SetActive(false);
	}

	// Token: 0x06000584 RID: 1412 RVA: 0x0002F636 File Offset: 0x0002D836
	public void OnQuitYes()
	{
		Gogan.LogEvent("Game", "Quit", "", 0L);
		Application.Quit();
	}

	// Token: 0x06000585 RID: 1413 RVA: 0x0002F653 File Offset: 0x0002D853
	public void OnQuitNo()
	{
		this.m_quitDialog.gameObject.SetActive(false);
		this.m_menuDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000586 RID: 1414 RVA: 0x0002F677 File Offset: 0x0002D877
	public void OnLogout()
	{
		this.m_menuDialog.gameObject.SetActive(false);
		this.m_logoutDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000587 RID: 1415 RVA: 0x0002F69B File Offset: 0x0002D89B
	public void OnLogoutYes()
	{
		Gogan.LogEvent("Game", "LogOut", "", 0L);
		Game.instance.Logout();
	}

	// Token: 0x06000588 RID: 1416 RVA: 0x0002F6BD File Offset: 0x0002D8BD
	public void OnLogoutNo()
	{
		this.m_logoutDialog.gameObject.SetActive(false);
		this.m_menuDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000589 RID: 1417 RVA: 0x0002F6E1 File Offset: 0x0002D8E1
	public void OnClose()
	{
		Gogan.LogEvent("Screen", "Exit", "Menu", 0L);
		this.m_root.gameObject.SetActive(false);
	}

	// Token: 0x0600058A RID: 1418 RVA: 0x0002F70A File Offset: 0x0002D90A
	public void OnButtonFeedback()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.m_feedbackPrefab, base.transform);
	}

	// Token: 0x04000624 RID: 1572
	private GameObject m_settingsInstance;

	// Token: 0x04000625 RID: 1573
	private static Menu m_instance;

	// Token: 0x04000626 RID: 1574
	public Transform m_root;

	// Token: 0x04000627 RID: 1575
	public Transform m_menuDialog;

	// Token: 0x04000628 RID: 1576
	public Transform m_quitDialog;

	// Token: 0x04000629 RID: 1577
	public Transform m_logoutDialog;

	// Token: 0x0400062A RID: 1578
	public GameObject m_settingsPrefab;

	// Token: 0x0400062B RID: 1579
	public GameObject m_feedbackPrefab;

	// Token: 0x0400062C RID: 1580
	private int m_hiddenFrames;
}
