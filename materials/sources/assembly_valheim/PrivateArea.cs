﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x020000E9 RID: 233
public class PrivateArea : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000E63 RID: 3683 RVA: 0x00066CB4 File Offset: 0x00064EB4
	private void Awake()
	{
		if (this.m_areaMarker)
		{
			this.m_areaMarker.m_radius = this.m_radius;
		}
		this.m_nview = base.GetComponent<ZNetView>();
		if (!this.m_nview.IsValid())
		{
			return;
		}
		WearNTear component = base.GetComponent<WearNTear>();
		component.m_onDamaged = (Action)Delegate.Combine(component.m_onDamaged, new Action(this.OnDamaged));
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_areaMarker)
		{
			this.m_areaMarker.gameObject.SetActive(false);
		}
		if (this.m_inRangeEffect)
		{
			this.m_inRangeEffect.SetActive(false);
		}
		PrivateArea.m_allAreas.Add(this);
		base.InvokeRepeating("UpdateStatus", 0f, 1f);
		this.m_nview.Register<long>("ToggleEnabled", new Action<long, long>(this.RPC_ToggleEnabled));
		this.m_nview.Register<long, string>("TogglePermitted", new Action<long, long, string>(this.RPC_TogglePermitted));
		this.m_nview.Register("FlashShield", new Action<long>(this.RPC_FlashShield));
	}

	// Token: 0x06000E64 RID: 3684 RVA: 0x00066DD7 File Offset: 0x00064FD7
	private void OnDestroy()
	{
		PrivateArea.m_allAreas.Remove(this);
	}

	// Token: 0x06000E65 RID: 3685 RVA: 0x00066DE8 File Offset: 0x00064FE8
	private void UpdateStatus()
	{
		bool flag = this.IsEnabled();
		this.m_enabledEffect.SetActive(flag);
		this.m_flashAvailable = true;
		foreach (Material material in this.m_model.materials)
		{
			if (flag)
			{
				material.EnableKeyword("_EMISSION");
			}
			else
			{
				material.DisableKeyword("_EMISSION");
			}
		}
	}

	// Token: 0x06000E66 RID: 3686 RVA: 0x00066E48 File Offset: 0x00065048
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		if (Player.m_localPlayer == null)
		{
			return "";
		}
		this.ShowAreaMarker();
		StringBuilder stringBuilder = new StringBuilder(256);
		if (this.m_piece.IsCreator())
		{
			if (this.IsEnabled())
			{
				stringBuilder.Append(this.m_name + " ( $piece_guardstone_active )");
				stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_deactivate");
			}
			else
			{
				stringBuilder.Append(this.m_name + " ($piece_guardstone_inactive )");
				stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_activate");
			}
		}
		else if (this.IsEnabled())
		{
			stringBuilder.Append(this.m_name + " ( $piece_guardstone_active )");
			stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
		}
		else
		{
			stringBuilder.Append(this.m_name + " ( $piece_guardstone_inactive )");
			stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
			if (this.IsPermitted(Player.m_localPlayer.GetPlayerID()))
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_remove");
			}
			else
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_add");
			}
		}
		this.AddUserList(stringBuilder);
		return Localization.instance.Localize(stringBuilder.ToString());
	}

	// Token: 0x06000E67 RID: 3687 RVA: 0x00066FCC File Offset: 0x000651CC
	private void AddUserList(StringBuilder text)
	{
		List<KeyValuePair<long, string>> permittedPlayers = this.GetPermittedPlayers();
		text.Append("\n$piece_guardstone_additional: ");
		for (int i = 0; i < permittedPlayers.Count; i++)
		{
			text.Append(permittedPlayers[i].Value);
			if (i != permittedPlayers.Count - 1)
			{
				text.Append(", ");
			}
		}
	}

	// Token: 0x06000E68 RID: 3688 RVA: 0x0006702C File Offset: 0x0006522C
	private void RemovePermitted(long playerID)
	{
		List<KeyValuePair<long, string>> permittedPlayers = this.GetPermittedPlayers();
		if (permittedPlayers.RemoveAll((KeyValuePair<long, string> x) => x.Key == playerID) > 0)
		{
			this.SetPermittedPlayers(permittedPlayers);
			this.m_removedPermittedEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
		}
	}

	// Token: 0x06000E69 RID: 3689 RVA: 0x00067094 File Offset: 0x00065294
	private bool IsPermitted(long playerID)
	{
		foreach (KeyValuePair<long, string> keyValuePair in this.GetPermittedPlayers())
		{
			if (keyValuePair.Key == playerID)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000E6A RID: 3690 RVA: 0x000670F4 File Offset: 0x000652F4
	private void AddPermitted(long playerID, string playerName)
	{
		List<KeyValuePair<long, string>> permittedPlayers = this.GetPermittedPlayers();
		foreach (KeyValuePair<long, string> keyValuePair in permittedPlayers)
		{
			if (keyValuePair.Key == playerID)
			{
				return;
			}
		}
		permittedPlayers.Add(new KeyValuePair<long, string>(playerID, playerName));
		this.SetPermittedPlayers(permittedPlayers);
		this.m_addPermittedEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000E6B RID: 3691 RVA: 0x0006718C File Offset: 0x0006538C
	private void SetPermittedPlayers(List<KeyValuePair<long, string>> users)
	{
		this.m_nview.GetZDO().Set("permitted", users.Count);
		for (int i = 0; i < users.Count; i++)
		{
			KeyValuePair<long, string> keyValuePair = users[i];
			this.m_nview.GetZDO().Set("pu_id" + i, keyValuePair.Key);
			this.m_nview.GetZDO().Set("pu_name" + i, keyValuePair.Value);
		}
	}

	// Token: 0x06000E6C RID: 3692 RVA: 0x0006721C File Offset: 0x0006541C
	private List<KeyValuePair<long, string>> GetPermittedPlayers()
	{
		List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>();
		int @int = this.m_nview.GetZDO().GetInt("permitted", 0);
		for (int i = 0; i < @int; i++)
		{
			long @long = this.m_nview.GetZDO().GetLong("pu_id" + i, 0L);
			string @string = this.m_nview.GetZDO().GetString("pu_name" + i, "");
			if (@long != 0L)
			{
				list.Add(new KeyValuePair<long, string>(@long, @string));
			}
		}
		return list;
	}

	// Token: 0x06000E6D RID: 3693 RVA: 0x000672AE File Offset: 0x000654AE
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000E6E RID: 3694 RVA: 0x000672B8 File Offset: 0x000654B8
	public bool Interact(Humanoid human, bool hold)
	{
		if (hold)
		{
			return false;
		}
		Player player = human as Player;
		if (this.m_piece.IsCreator())
		{
			this.m_nview.InvokeRPC("ToggleEnabled", new object[]
			{
				player.GetPlayerID()
			});
			return true;
		}
		if (this.IsEnabled())
		{
			return false;
		}
		this.m_nview.InvokeRPC("TogglePermitted", new object[]
		{
			player.GetPlayerID(),
			player.GetPlayerName()
		});
		return true;
	}

	// Token: 0x06000E6F RID: 3695 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000E70 RID: 3696 RVA: 0x0006733C File Offset: 0x0006553C
	private void RPC_TogglePermitted(long uid, long playerID, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.IsEnabled())
		{
			return;
		}
		if (this.IsPermitted(playerID))
		{
			this.RemovePermitted(playerID);
			return;
		}
		this.AddPermitted(playerID, name);
	}

	// Token: 0x06000E71 RID: 3697 RVA: 0x00067370 File Offset: 0x00065570
	private void RPC_ToggleEnabled(long uid, long playerID)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Toggle enabled from ",
			playerID,
			"  creator is ",
			this.m_piece.GetCreator()
		}));
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_piece.GetCreator() != playerID)
		{
			return;
		}
		this.SetEnabled(!this.IsEnabled());
	}

	// Token: 0x06000E72 RID: 3698 RVA: 0x000673E5 File Offset: 0x000655E5
	public bool IsEnabled()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool("enabled", false);
	}

	// Token: 0x06000E73 RID: 3699 RVA: 0x0006740C File Offset: 0x0006560C
	private void SetEnabled(bool enabled)
	{
		this.m_nview.GetZDO().Set("enabled", enabled);
		this.UpdateStatus();
		if (enabled)
		{
			this.m_activateEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
			return;
		}
		this.m_deactivateEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000E74 RID: 3700 RVA: 0x00067489 File Offset: 0x00065689
	public void Setup(string name)
	{
		this.m_nview.GetZDO().Set("creatorName", name);
	}

	// Token: 0x06000E75 RID: 3701 RVA: 0x000674A4 File Offset: 0x000656A4
	public void PokeAllAreasInRange()
	{
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (!(privateArea == this) && this.IsInside(privateArea.transform.position, 0f))
			{
				privateArea.StartInRangeEffect();
			}
		}
	}

	// Token: 0x06000E76 RID: 3702 RVA: 0x00067518 File Offset: 0x00065718
	private void StartInRangeEffect()
	{
		this.m_inRangeEffect.SetActive(true);
		base.CancelInvoke("StopInRangeEffect");
		base.Invoke("StopInRangeEffect", 0.2f);
	}

	// Token: 0x06000E77 RID: 3703 RVA: 0x00067541 File Offset: 0x00065741
	private void StopInRangeEffect()
	{
		this.m_inRangeEffect.SetActive(false);
	}

	// Token: 0x06000E78 RID: 3704 RVA: 0x00067550 File Offset: 0x00065750
	public void PokeConnectionEffects()
	{
		List<PrivateArea> connectedAreas = this.GetConnectedAreas(false);
		this.StartConnectionEffects();
		foreach (PrivateArea privateArea in connectedAreas)
		{
			privateArea.StartConnectionEffects();
		}
	}

	// Token: 0x06000E79 RID: 3705 RVA: 0x000675A8 File Offset: 0x000657A8
	private void StartConnectionEffects()
	{
		List<PrivateArea> list = new List<PrivateArea>();
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (!(privateArea == this) && this.IsInside(privateArea.transform.position, 0f))
			{
				list.Add(privateArea);
			}
		}
		Vector3 vector = base.transform.position + Vector3.up * 1.4f;
		if (this.m_connectionInstances.Count != list.Count)
		{
			this.StopConnectionEffects();
			for (int i = 0; i < list.Count; i++)
			{
				GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_connectEffect, vector, Quaternion.identity, base.transform);
				this.m_connectionInstances.Add(item);
			}
		}
		if (this.m_connectionInstances.Count == 0)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			Vector3 vector2 = list[j].transform.position + Vector3.up * 1.4f - vector;
			Quaternion rotation = Quaternion.LookRotation(vector2.normalized);
			GameObject gameObject = this.m_connectionInstances[j];
			gameObject.transform.position = vector;
			gameObject.transform.rotation = rotation;
			gameObject.transform.localScale = new Vector3(1f, 1f, vector2.magnitude);
		}
		base.CancelInvoke("StopConnectionEffects");
		base.Invoke("StopConnectionEffects", 0.3f);
	}

	// Token: 0x06000E7A RID: 3706 RVA: 0x0006775C File Offset: 0x0006595C
	private void StopConnectionEffects()
	{
		foreach (GameObject obj in this.m_connectionInstances)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_connectionInstances.Clear();
	}

	// Token: 0x06000E7B RID: 3707 RVA: 0x000677B8 File Offset: 0x000659B8
	private string GetCreatorName()
	{
		return this.m_nview.GetZDO().GetString("creatorName", "");
	}

	// Token: 0x06000E7C RID: 3708 RVA: 0x000677D4 File Offset: 0x000659D4
	public static bool CheckInPrivateArea(Vector3 point, bool flash = false)
	{
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (privateArea.IsEnabled() && privateArea.IsInside(point, 0f))
			{
				if (flash)
				{
					privateArea.FlashShield(false);
				}
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000E7D RID: 3709 RVA: 0x00067848 File Offset: 0x00065A48
	public static bool CheckAccess(Vector3 point, float radius = 0f, bool flash = true, bool wardCheck = false)
	{
		List<PrivateArea> list = new List<PrivateArea>();
		bool flag = true;
		if (wardCheck)
		{
			flag = true;
			using (List<PrivateArea>.Enumerator enumerator = PrivateArea.m_allAreas.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PrivateArea privateArea = enumerator.Current;
					if (privateArea.IsEnabled() && privateArea.IsInside(point, radius) && !privateArea.HaveLocalAccess())
					{
						flag = false;
						list.Add(privateArea);
					}
				}
				goto IL_B8;
			}
		}
		flag = false;
		foreach (PrivateArea privateArea2 in PrivateArea.m_allAreas)
		{
			if (privateArea2.IsEnabled() && privateArea2.IsInside(point, radius))
			{
				if (privateArea2.HaveLocalAccess())
				{
					flag = true;
				}
				else
				{
					list.Add(privateArea2);
				}
			}
		}
		IL_B8:
		if (!flag && list.Count > 0)
		{
			if (flash)
			{
				foreach (PrivateArea privateArea3 in list)
				{
					privateArea3.FlashShield(false);
				}
			}
			return false;
		}
		return true;
	}

	// Token: 0x06000E7E RID: 3710 RVA: 0x00067978 File Offset: 0x00065B78
	private bool HaveLocalAccess()
	{
		return this.m_piece.IsCreator() || this.IsPermitted(Player.m_localPlayer.GetPlayerID());
	}

	// Token: 0x06000E7F RID: 3711 RVA: 0x0006799E File Offset: 0x00065B9E
	private List<PrivateArea> GetConnectedAreas(bool forceUpdate = false)
	{
		if (Time.time - this.m_connectionUpdateTime > this.m_updateConnectionsInterval || forceUpdate)
		{
			this.GetAllConnectedAreas(this.m_connectedAreas);
			this.m_connectionUpdateTime = Time.time;
		}
		return this.m_connectedAreas;
	}

	// Token: 0x06000E80 RID: 3712 RVA: 0x000679D8 File Offset: 0x00065BD8
	private void GetAllConnectedAreas(List<PrivateArea> areas)
	{
		Queue<PrivateArea> queue = new Queue<PrivateArea>();
		queue.Enqueue(this);
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			privateArea.m_tempChecked = false;
		}
		this.m_tempChecked = true;
		while (queue.Count > 0)
		{
			PrivateArea privateArea2 = queue.Dequeue();
			foreach (PrivateArea privateArea3 in PrivateArea.m_allAreas)
			{
				if (!privateArea3.m_tempChecked && privateArea3.IsEnabled() && privateArea3.IsInside(privateArea2.transform.position, 0f))
				{
					privateArea3.m_tempChecked = true;
					queue.Enqueue(privateArea3);
					areas.Add(privateArea3);
				}
			}
		}
	}

	// Token: 0x06000E81 RID: 3713 RVA: 0x00067AC8 File Offset: 0x00065CC8
	private void FlashShield(bool flashConnected)
	{
		if (!this.m_flashAvailable)
		{
			return;
		}
		this.m_flashAvailable = false;
		this.m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield", Array.Empty<object>());
		if (flashConnected)
		{
			foreach (PrivateArea privateArea in this.GetConnectedAreas(false))
			{
				if (privateArea.m_nview.IsValid())
				{
					privateArea.m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield", Array.Empty<object>());
				}
			}
		}
	}

	// Token: 0x06000E82 RID: 3714 RVA: 0x00067B6C File Offset: 0x00065D6C
	private void RPC_FlashShield(long uid)
	{
		this.m_flashEffect.Create(base.transform.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000E83 RID: 3715 RVA: 0x00067B90 File Offset: 0x00065D90
	private bool IsInside(Vector3 point, float radius)
	{
		return Utils.DistanceXZ(base.transform.position, point) < this.m_radius + radius;
	}

	// Token: 0x06000E84 RID: 3716 RVA: 0x00067BAD File Offset: 0x00065DAD
	public void ShowAreaMarker()
	{
		if (this.m_areaMarker)
		{
			this.m_areaMarker.gameObject.SetActive(true);
			base.CancelInvoke("HideMarker");
			base.Invoke("HideMarker", 0.5f);
		}
	}

	// Token: 0x06000E85 RID: 3717 RVA: 0x00067BE8 File Offset: 0x00065DE8
	private void HideMarker()
	{
		this.m_areaMarker.gameObject.SetActive(false);
	}

	// Token: 0x06000E86 RID: 3718 RVA: 0x00067BFB File Offset: 0x00065DFB
	private void OnDamaged()
	{
		if (this.IsEnabled())
		{
			this.FlashShield(false);
		}
	}

	// Token: 0x06000E87 RID: 3719 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmosSelected()
	{
	}

	// Token: 0x04000D44 RID: 3396
	public string m_name = "Guard stone";

	// Token: 0x04000D45 RID: 3397
	public float m_radius = 10f;

	// Token: 0x04000D46 RID: 3398
	public float m_updateConnectionsInterval = 5f;

	// Token: 0x04000D47 RID: 3399
	public GameObject m_enabledEffect;

	// Token: 0x04000D48 RID: 3400
	public CircleProjector m_areaMarker;

	// Token: 0x04000D49 RID: 3401
	public EffectList m_flashEffect = new EffectList();

	// Token: 0x04000D4A RID: 3402
	public EffectList m_activateEffect = new EffectList();

	// Token: 0x04000D4B RID: 3403
	public EffectList m_deactivateEffect = new EffectList();

	// Token: 0x04000D4C RID: 3404
	public EffectList m_addPermittedEffect = new EffectList();

	// Token: 0x04000D4D RID: 3405
	public EffectList m_removedPermittedEffect = new EffectList();

	// Token: 0x04000D4E RID: 3406
	public GameObject m_connectEffect;

	// Token: 0x04000D4F RID: 3407
	public GameObject m_inRangeEffect;

	// Token: 0x04000D50 RID: 3408
	public MeshRenderer m_model;

	// Token: 0x04000D51 RID: 3409
	private ZNetView m_nview;

	// Token: 0x04000D52 RID: 3410
	private Piece m_piece;

	// Token: 0x04000D53 RID: 3411
	private bool m_flashAvailable = true;

	// Token: 0x04000D54 RID: 3412
	private bool m_tempChecked;

	// Token: 0x04000D55 RID: 3413
	private List<GameObject> m_connectionInstances = new List<GameObject>();

	// Token: 0x04000D56 RID: 3414
	private float m_connectionUpdateTime = -1000f;

	// Token: 0x04000D57 RID: 3415
	private List<PrivateArea> m_connectedAreas = new List<PrivateArea>();

	// Token: 0x04000D58 RID: 3416
	private static List<PrivateArea> m_allAreas = new List<PrivateArea>();
}
