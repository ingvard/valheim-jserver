using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000EE RID: 238
public class Raven : MonoBehaviour, Hoverable, Interactable, IDestructible
{
	// Token: 0x06000E9D RID: 3741 RVA: 0x0006883E File Offset: 0x00066A3E
	public static bool IsInstantiated()
	{
		return Raven.m_instance != null;
	}

	// Token: 0x06000E9E RID: 3742 RVA: 0x0006884C File Offset: 0x00066A4C
	private void Awake()
	{
		base.transform.position = new Vector3(0f, 100000f, 0f);
		Raven.m_instance = this;
		this.m_animator = this.m_visual.GetComponentInChildren<Animator>();
		this.m_collider = base.GetComponent<Collider>();
		base.InvokeRepeating("IdleEffect", UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax), UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax));
		base.InvokeRepeating("CheckSpawn", 1f, 1f);
	}

	// Token: 0x06000E9F RID: 3743 RVA: 0x000688DD File Offset: 0x00066ADD
	private void OnDestroy()
	{
		if (Raven.m_instance == this)
		{
			Raven.m_instance = null;
		}
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x000688F2 File Offset: 0x00066AF2
	public string GetHoverText()
	{
		if (this.IsSpawned())
		{
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");
		}
		return "";
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x0006891C File Offset: 0x00066B1C
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x0006892E File Offset: 0x00066B2E
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_hasTalked && Chat.instance.IsDialogVisible(base.gameObject))
		{
			Chat.instance.ClearNpcText(base.gameObject);
		}
		else
		{
			this.Talk();
		}
		return false;
	}

	// Token: 0x06000EA3 RID: 3747 RVA: 0x00068968 File Offset: 0x00066B68
	private void Talk()
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (this.m_currentText == null)
		{
			return;
		}
		if (this.m_currentText.m_key.Length > 0)
		{
			Player.m_localPlayer.SetSeenTutorial(this.m_currentText.m_key);
			Gogan.LogEvent("Game", "Raven", this.m_currentText.m_key, 0L);
		}
		else
		{
			Gogan.LogEvent("Game", "Raven", this.m_currentText.m_topic, 0L);
		}
		this.m_hasTalked = true;
		if (this.m_currentText.m_label.Length > 0)
		{
			Player.m_localPlayer.AddKnownText(this.m_currentText.m_label, this.m_currentText.m_text);
		}
		this.Say(this.m_currentText.m_topic, this.m_currentText.m_text, false, true, true);
	}

	// Token: 0x06000EA4 RID: 3748 RVA: 0x00068A48 File Offset: 0x00066C48
	private void Say(string topic, string text, bool showName, bool longTimeout, bool large)
	{
		if (topic.Length > 0)
		{
			text = "<color=orange>" + topic + "</color>\n" + text;
		}
		Chat.instance.SetNpcText(base.gameObject, Vector3.up * this.m_textOffset, this.m_textCullDistance, longTimeout ? this.m_longDialogVisibleTime : this.m_dialogVisibleTime, showName ? this.m_name : "", text, large);
		this.m_animator.SetTrigger("talk");
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000EA6 RID: 3750 RVA: 0x00068ACC File Offset: 0x00066CCC
	private void IdleEffect()
	{
		if (!this.IsSpawned())
		{
			return;
		}
		this.m_idleEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
		base.CancelInvoke("IdleEffect");
		base.InvokeRepeating("IdleEffect", UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax), UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax));
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x00068B42 File Offset: 0x00066D42
	private bool CanHide()
	{
		return Player.m_localPlayer == null || !Chat.instance.IsDialogVisible(base.gameObject);
	}

	// Token: 0x06000EA8 RID: 3752 RVA: 0x00068B68 File Offset: 0x00066D68
	private void Update()
	{
		this.m_timeSinceTeleport += Time.deltaTime;
		if (!this.IsAway() && !this.IsFlying() && Player.m_localPlayer)
		{
			Vector3 vector = Player.m_localPlayer.transform.position - base.transform.position;
			vector.y = 0f;
			vector.Normalize();
			float f = Vector3.SignedAngle(base.transform.forward, vector, Vector3.up);
			if (Mathf.Abs(f) > this.m_minRotationAngle)
			{
				this.m_animator.SetFloat("anglevel", this.m_rotateSpeed * Mathf.Sign(f), 0.4f, Time.deltaTime);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(vector), Time.deltaTime * this.m_rotateSpeed);
			}
			else
			{
				this.m_animator.SetFloat("anglevel", 0f, 0.4f, Time.deltaTime);
			}
		}
		if (this.IsSpawned())
		{
			if (Player.m_localPlayer != null && !Chat.instance.IsDialogVisible(base.gameObject) && Vector3.Distance(Player.m_localPlayer.transform.position, base.transform.position) < this.m_autoTalkDistance)
			{
				this.m_randomTextTimer += Time.deltaTime;
				float num = this.m_hasTalked ? this.m_randomTextInterval : this.m_randomTextIntervalImportant;
				if (this.m_randomTextTimer >= num)
				{
					this.m_randomTextTimer = 0f;
					if (this.m_hasTalked)
					{
						this.Say("", this.m_randomTexts[UnityEngine.Random.Range(0, this.m_randomTexts.Count)], false, false, false);
					}
					else
					{
						this.Say("", this.m_randomTextsImportant[UnityEngine.Random.Range(0, this.m_randomTextsImportant.Count)], false, false, false);
					}
				}
			}
			if ((Player.m_localPlayer == null || Vector3.Distance(Player.m_localPlayer.transform.position, base.transform.position) > this.m_despawnDistance || this.EnemyNearby(base.transform.position) || RandEventSystem.InEvent() || this.m_currentText == null || this.m_groundObject == null || this.m_hasTalked) && this.CanHide())
			{
				bool forceTeleport = this.GetBestText() != null || this.m_groundObject == null;
				this.FlyAway(forceTeleport);
				this.RestartSpawnCheck(3f);
			}
			this.m_exclamation.SetActive(!this.m_hasTalked);
			return;
		}
		this.m_exclamation.SetActive(false);
	}

	// Token: 0x06000EA9 RID: 3753 RVA: 0x00068E30 File Offset: 0x00067030
	private bool FindSpawnPoint(out Vector3 point, out GameObject landOn)
	{
		Vector3 position = Player.m_localPlayer.transform.position;
		Vector3 forward = Utils.GetMainCamera().transform.forward;
		forward.y = 0f;
		forward.Normalize();
		point = new Vector3(0f, -999f, 0f);
		landOn = null;
		bool result = false;
		for (int i = 0; i < 20; i++)
		{
			Vector3 a = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(-30, 30), 0f) * forward;
			Vector3 vector = position + a * UnityEngine.Random.Range(this.m_spawnDistance - 5f, this.m_spawnDistance);
			float num;
			Vector3 vector2;
			GameObject gameObject;
			if (ZoneSystem.instance.GetSolidHeight(vector, out num, out vector2, out gameObject) && num > ZoneSystem.instance.m_waterLevel && num > point.y && num < 2000f && vector2.y > 0.5f && Mathf.Abs(num - position.y) < 2f)
			{
				vector.y = num;
				point = vector;
				landOn = gameObject;
				result = true;
			}
		}
		return result;
	}

	// Token: 0x06000EAA RID: 3754 RVA: 0x00068F59 File Offset: 0x00067159
	private bool EnemyNearby(Vector3 point)
	{
		return LootSpawner.IsMonsterInRange(point, this.m_enemyCheckDistance);
	}

	// Token: 0x06000EAB RID: 3755 RVA: 0x00068F68 File Offset: 0x00067168
	private bool InState(string name)
	{
		return this.m_animator.isInitialized && (this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag(name) || this.m_animator.GetNextAnimatorStateInfo(0).IsTag(name));
	}

	// Token: 0x06000EAC RID: 3756 RVA: 0x00068FB8 File Offset: 0x000671B8
	private Raven.RavenText GetBestText()
	{
		Raven.RavenText ravenText = this.GetTempText();
		Raven.RavenText closestStaticText = this.GetClosestStaticText(this.m_spawnDistance);
		if (closestStaticText != null && (ravenText == null || closestStaticText.m_priority >= ravenText.m_priority))
		{
			ravenText = closestStaticText;
		}
		return ravenText;
	}

	// Token: 0x06000EAD RID: 3757 RVA: 0x00068FF0 File Offset: 0x000671F0
	private Raven.RavenText GetTempText()
	{
		foreach (Raven.RavenText ravenText in Raven.m_tempTexts)
		{
			if (ravenText.m_munin == this.m_isMunin)
			{
				return ravenText;
			}
		}
		return null;
	}

	// Token: 0x06000EAE RID: 3758 RVA: 0x00069050 File Offset: 0x00067250
	private Raven.RavenText GetClosestStaticText(float maxDistance)
	{
		if (Player.m_localPlayer == null)
		{
			return null;
		}
		Raven.RavenText ravenText = null;
		float num = 9999f;
		bool flag = false;
		Vector3 position = Player.m_localPlayer.transform.position;
		foreach (Raven.RavenText ravenText2 in Raven.m_staticTexts)
		{
			if (ravenText2.m_munin == this.m_isMunin && ravenText2.m_guidePoint)
			{
				float num2 = Vector3.Distance(position, ravenText2.m_guidePoint.transform.position);
				if (num2 < maxDistance)
				{
					bool flag2 = ravenText2.m_key.Length > 0 && Player.m_localPlayer.HaveSeenTutorial(ravenText2.m_key);
					if (ravenText2.m_alwaysSpawn || !flag2)
					{
						if (ravenText == null)
						{
							ravenText = ravenText2;
							num = num2;
							flag = flag2;
						}
						else if (flag2 == flag)
						{
							if (ravenText2.m_priority == ravenText.m_priority || flag2)
							{
								if (num2 < num)
								{
									ravenText = ravenText2;
									num = num2;
									flag = flag2;
								}
							}
							else if (ravenText2.m_priority > ravenText.m_priority)
							{
								ravenText = ravenText2;
								num = num2;
								flag = flag2;
							}
						}
						else if (!flag2 && flag)
						{
							ravenText = ravenText2;
							num = num2;
							flag = flag2;
						}
					}
				}
			}
		}
		return ravenText;
	}

	// Token: 0x06000EAF RID: 3759 RVA: 0x000691A8 File Offset: 0x000673A8
	private void RemoveSeendTempTexts()
	{
		for (int i = 0; i < Raven.m_tempTexts.Count; i++)
		{
			if (Player.m_localPlayer.HaveSeenTutorial(Raven.m_tempTexts[i].m_key))
			{
				Raven.m_tempTexts.RemoveAt(i);
				return;
			}
		}
	}

	// Token: 0x06000EB0 RID: 3760 RVA: 0x000691F4 File Offset: 0x000673F4
	private void FlyAway(bool forceTeleport = false)
	{
		Chat.instance.ClearNpcText(base.gameObject);
		if (forceTeleport || this.IsUnderRoof())
		{
			this.m_animator.SetTrigger("poff");
			this.m_timeSinceTeleport = 0f;
			return;
		}
		this.m_animator.SetTrigger("flyaway");
	}

	// Token: 0x06000EB1 RID: 3761 RVA: 0x00069248 File Offset: 0x00067448
	private void CheckSpawn()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		this.RemoveSeendTempTexts();
		Raven.RavenText bestText = this.GetBestText();
		if (this.IsSpawned() && this.CanHide() && bestText != null && bestText != this.m_currentText)
		{
			this.FlyAway(true);
			this.m_currentText = null;
		}
		if (this.IsAway() && bestText != null)
		{
			if (this.EnemyNearby(base.transform.position))
			{
				return;
			}
			if (RandEventSystem.InEvent())
			{
				return;
			}
			bool forceTeleport = this.m_timeSinceTeleport < 6f;
			this.Spawn(bestText, forceTeleport);
		}
	}

	// Token: 0x06000EB2 RID: 3762 RVA: 0x00003EB4 File Offset: 0x000020B4
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Character;
	}

	// Token: 0x06000EB3 RID: 3763 RVA: 0x000692D7 File Offset: 0x000674D7
	public void Damage(HitData hit)
	{
		if (!this.IsSpawned())
		{
			return;
		}
		this.m_animator.SetTrigger("poff");
		this.RestartSpawnCheck(4f);
	}

	// Token: 0x06000EB4 RID: 3764 RVA: 0x000692FD File Offset: 0x000674FD
	private void RestartSpawnCheck(float delay)
	{
		base.CancelInvoke("CheckSpawn");
		base.InvokeRepeating("CheckSpawn", delay, 1f);
	}

	// Token: 0x06000EB5 RID: 3765 RVA: 0x0006931B File Offset: 0x0006751B
	private bool IsSpawned()
	{
		return this.InState("visible");
	}

	// Token: 0x06000EB6 RID: 3766 RVA: 0x00069328 File Offset: 0x00067528
	public bool IsAway()
	{
		return this.InState("away");
	}

	// Token: 0x06000EB7 RID: 3767 RVA: 0x00069335 File Offset: 0x00067535
	public bool IsFlying()
	{
		return this.InState("flying");
	}

	// Token: 0x06000EB8 RID: 3768 RVA: 0x00069344 File Offset: 0x00067544
	private void Spawn(Raven.RavenText text, bool forceTeleport)
	{
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		if (text.m_static)
		{
			this.m_groundObject = text.m_guidePoint.gameObject;
			base.transform.position = text.m_guidePoint.transform.position;
		}
		else
		{
			Vector3 position;
			GameObject groundObject;
			if (!this.FindSpawnPoint(out position, out groundObject))
			{
				return;
			}
			base.transform.position = position;
			this.m_groundObject = groundObject;
		}
		this.m_currentText = text;
		this.m_hasTalked = false;
		this.m_randomTextTimer = 99999f;
		if (this.m_currentText.m_key.Length > 0 && Player.m_localPlayer.HaveSeenTutorial(this.m_currentText.m_key))
		{
			this.m_hasTalked = true;
		}
		Vector3 forward = Player.m_localPlayer.transform.position - base.transform.position;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation(forward);
		if (forceTeleport)
		{
			this.m_animator.SetTrigger("teleportin");
			return;
		}
		if (!text.m_static)
		{
			this.m_animator.SetTrigger("flyin");
			return;
		}
		if (this.IsUnderRoof())
		{
			this.m_animator.SetTrigger("teleportin");
			return;
		}
		this.m_animator.SetTrigger("flyin");
	}

	// Token: 0x06000EB9 RID: 3769 RVA: 0x0006949C File Offset: 0x0006769C
	private bool IsUnderRoof()
	{
		return Physics.Raycast(base.transform.position + Vector3.up * 0.2f, Vector3.up, 20f, LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"piece"
		}));
	}

	// Token: 0x06000EBA RID: 3770 RVA: 0x000694FA File Offset: 0x000676FA
	public static void RegisterStaticText(Raven.RavenText text)
	{
		Raven.m_staticTexts.Add(text);
	}

	// Token: 0x06000EBB RID: 3771 RVA: 0x00069507 File Offset: 0x00067707
	public static void UnregisterStaticText(Raven.RavenText text)
	{
		Raven.m_staticTexts.Remove(text);
	}

	// Token: 0x06000EBC RID: 3772 RVA: 0x00069518 File Offset: 0x00067718
	public static void AddTempText(string key, string topic, string text, string label, bool munin)
	{
		if (key.Length > 0)
		{
			using (List<Raven.RavenText>.Enumerator enumerator = Raven.m_tempTexts.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.m_key == key)
					{
						return;
					}
				}
			}
		}
		Raven.RavenText ravenText = new Raven.RavenText();
		ravenText.m_key = key;
		ravenText.m_topic = topic;
		ravenText.m_label = label;
		ravenText.m_text = text;
		ravenText.m_static = false;
		ravenText.m_munin = munin;
		Raven.m_tempTexts.Add(ravenText);
	}

	// Token: 0x04000D8B RID: 3467
	public GameObject m_visual;

	// Token: 0x04000D8C RID: 3468
	public GameObject m_exclamation;

	// Token: 0x04000D8D RID: 3469
	public string m_name = "Name";

	// Token: 0x04000D8E RID: 3470
	public bool m_isMunin;

	// Token: 0x04000D8F RID: 3471
	public bool m_autoTalk = true;

	// Token: 0x04000D90 RID: 3472
	public float m_idleEffectIntervalMin = 10f;

	// Token: 0x04000D91 RID: 3473
	public float m_idleEffectIntervalMax = 20f;

	// Token: 0x04000D92 RID: 3474
	public float m_spawnDistance = 15f;

	// Token: 0x04000D93 RID: 3475
	public float m_despawnDistance = 20f;

	// Token: 0x04000D94 RID: 3476
	public float m_autoTalkDistance = 3f;

	// Token: 0x04000D95 RID: 3477
	public float m_enemyCheckDistance = 10f;

	// Token: 0x04000D96 RID: 3478
	public float m_rotateSpeed = 10f;

	// Token: 0x04000D97 RID: 3479
	public float m_minRotationAngle = 15f;

	// Token: 0x04000D98 RID: 3480
	public float m_dialogVisibleTime = 10f;

	// Token: 0x04000D99 RID: 3481
	public float m_longDialogVisibleTime = 10f;

	// Token: 0x04000D9A RID: 3482
	public float m_dontFlyDistance = 3f;

	// Token: 0x04000D9B RID: 3483
	public float m_textOffset = 1.5f;

	// Token: 0x04000D9C RID: 3484
	public float m_textCullDistance = 20f;

	// Token: 0x04000D9D RID: 3485
	public float m_randomTextInterval = 30f;

	// Token: 0x04000D9E RID: 3486
	public float m_randomTextIntervalImportant = 10f;

	// Token: 0x04000D9F RID: 3487
	public List<string> m_randomTextsImportant = new List<string>();

	// Token: 0x04000DA0 RID: 3488
	public List<string> m_randomTexts = new List<string>();

	// Token: 0x04000DA1 RID: 3489
	public EffectList m_idleEffect = new EffectList();

	// Token: 0x04000DA2 RID: 3490
	public EffectList m_despawnEffect = new EffectList();

	// Token: 0x04000DA3 RID: 3491
	private Raven.RavenText m_currentText;

	// Token: 0x04000DA4 RID: 3492
	private GameObject m_groundObject;

	// Token: 0x04000DA5 RID: 3493
	private Animator m_animator;

	// Token: 0x04000DA6 RID: 3494
	private Collider m_collider;

	// Token: 0x04000DA7 RID: 3495
	private bool m_hasTalked;

	// Token: 0x04000DA8 RID: 3496
	private float m_randomTextTimer = 9999f;

	// Token: 0x04000DA9 RID: 3497
	private float m_timeSinceTeleport = 9999f;

	// Token: 0x04000DAA RID: 3498
	private static List<Raven.RavenText> m_tempTexts = new List<Raven.RavenText>();

	// Token: 0x04000DAB RID: 3499
	private static List<Raven.RavenText> m_staticTexts = new List<Raven.RavenText>();

	// Token: 0x04000DAC RID: 3500
	private static Raven m_instance = null;

	// Token: 0x020001A8 RID: 424
	[Serializable]
	public class RavenText
	{
		// Token: 0x040012F8 RID: 4856
		public bool m_alwaysSpawn = true;

		// Token: 0x040012F9 RID: 4857
		public bool m_munin;

		// Token: 0x040012FA RID: 4858
		public int m_priority;

		// Token: 0x040012FB RID: 4859
		public string m_key = "";

		// Token: 0x040012FC RID: 4860
		public string m_topic = "";

		// Token: 0x040012FD RID: 4861
		public string m_label = "";

		// Token: 0x040012FE RID: 4862
		[TextArea]
		public string m_text = "";

		// Token: 0x040012FF RID: 4863
		[NonSerialized]
		public bool m_static;

		// Token: 0x04001300 RID: 4864
		[NonSerialized]
		public GuidePoint m_guidePoint;
	}
}
