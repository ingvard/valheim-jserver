﻿using System;
using UnityEngine;

// Token: 0x02000057 RID: 87
public class KeyHints : MonoBehaviour
{
	// Token: 0x06000577 RID: 1399 RVA: 0x0002F313 File Offset: 0x0002D513
	private void OnDestroy()
	{
		KeyHints.m_instance = null;
	}

	// Token: 0x1700000B RID: 11
	// (get) Token: 0x06000578 RID: 1400 RVA: 0x0002F31B File Offset: 0x0002D51B
	public static KeyHints instance
	{
		get
		{
			return KeyHints.m_instance;
		}
	}

	// Token: 0x06000579 RID: 1401 RVA: 0x0002F322 File Offset: 0x0002D522
	private void Awake()
	{
		KeyHints.m_instance = this;
		this.ApplySettings();
	}

	// Token: 0x0600057A RID: 1402 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Start()
	{
	}

	// Token: 0x0600057B RID: 1403 RVA: 0x0002F330 File Offset: 0x0002D530
	public void ApplySettings()
	{
		this.m_keyHintsEnabled = (PlayerPrefs.GetInt("KeyHints", 1) == 1);
	}

	// Token: 0x0600057C RID: 1404 RVA: 0x0002F34A File Offset: 0x0002D54A
	private void Update()
	{
		this.UpdateHints();
	}

	// Token: 0x0600057D RID: 1405 RVA: 0x0002F354 File Offset: 0x0002D554
	private void UpdateHints()
	{
		Player localPlayer = Player.m_localPlayer;
		if (!this.m_keyHintsEnabled || localPlayer == null || localPlayer.IsDead() || Chat.instance.IsChatDialogWindowVisible())
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(false);
			return;
		}
		bool activeSelf = this.m_buildHints.activeSelf;
		bool activeSelf2 = this.m_buildHints.activeSelf;
		ItemDrop.ItemData currentWeapon = localPlayer.GetCurrentWeapon();
		if (localPlayer.InPlaceMode())
		{
			this.m_buildHints.SetActive(true);
			this.m_combatHints.SetActive(false);
			return;
		}
		if (localPlayer.GetShipControl())
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(false);
			return;
		}
		if (currentWeapon != null && (currentWeapon != localPlayer.m_unarmedWeapon.m_itemData || localPlayer.IsTargeted()))
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(true);
			bool flag = currentWeapon.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow;
			bool active = !flag && currentWeapon.HavePrimaryAttack();
			bool active2 = !flag && currentWeapon.HaveSecondaryAttack();
			this.m_bowDrawGP.SetActive(flag);
			this.m_bowDrawKB.SetActive(flag);
			this.m_primaryAttackGP.SetActive(active);
			this.m_primaryAttackKB.SetActive(active);
			this.m_secondaryAttackGP.SetActive(active2);
			this.m_secondaryAttackKB.SetActive(active2);
			return;
		}
		this.m_buildHints.SetActive(false);
		this.m_combatHints.SetActive(false);
	}

	// Token: 0x0400061E RID: 1566
	private static KeyHints m_instance;

	// Token: 0x0400061F RID: 1567
	[Header("Key hints")]
	public GameObject m_buildHints;

	// Token: 0x04000620 RID: 1568
	public GameObject m_combatHints;

	// Token: 0x04000621 RID: 1569
	public GameObject m_primaryAttackGP;

	// Token: 0x04000622 RID: 1570
	public GameObject m_primaryAttackKB;

	// Token: 0x04000623 RID: 1571
	public GameObject m_secondaryAttackGP;

	// Token: 0x04000624 RID: 1572
	public GameObject m_secondaryAttackKB;

	// Token: 0x04000625 RID: 1573
	public GameObject m_bowDrawGP;

	// Token: 0x04000626 RID: 1574
	public GameObject m_bowDrawKB;

	// Token: 0x04000627 RID: 1575
	private bool m_keyHintsEnabled = true;
}
