using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200004F RID: 79
public class DamageText : MonoBehaviour
{
	// Token: 0x17000007 RID: 7
	// (get) Token: 0x060004D5 RID: 1237 RVA: 0x00027FB1 File Offset: 0x000261B1
	public static DamageText instance
	{
		get
		{
			return DamageText.m_instance;
		}
	}

	// Token: 0x060004D6 RID: 1238 RVA: 0x00027FB8 File Offset: 0x000261B8
	private void Awake()
	{
		DamageText.m_instance = this;
		ZRoutedRpc.instance.Register<ZPackage>("DamageText", new Action<long, ZPackage>(this.RPC_DamageText));
	}

	// Token: 0x060004D7 RID: 1239 RVA: 0x00027FDB File Offset: 0x000261DB
	private void LateUpdate()
	{
		this.UpdateWorldTexts(Time.deltaTime);
	}

	// Token: 0x060004D8 RID: 1240 RVA: 0x00027FE8 File Offset: 0x000261E8
	private void UpdateWorldTexts(float dt)
	{
		DamageText.WorldTextInstance worldTextInstance = null;
		Camera mainCamera = Utils.GetMainCamera();
		foreach (DamageText.WorldTextInstance worldTextInstance2 in this.m_worldTexts)
		{
			worldTextInstance2.m_timer += dt;
			if (worldTextInstance2.m_timer > this.m_textDuration && worldTextInstance == null)
			{
				worldTextInstance = worldTextInstance2;
			}
			DamageText.WorldTextInstance worldTextInstance3 = worldTextInstance2;
			worldTextInstance3.m_worldPos.y = worldTextInstance3.m_worldPos.y + dt;
			float f = Mathf.Clamp01(worldTextInstance2.m_timer / this.m_textDuration);
			Color color = worldTextInstance2.m_textField.color;
			color.a = 1f - Mathf.Pow(f, 3f);
			worldTextInstance2.m_textField.color = color;
			Vector3 vector = mainCamera.WorldToScreenPoint(worldTextInstance2.m_worldPos);
			if (vector.x < 0f || vector.x > (float)Screen.width || vector.y < 0f || vector.y > (float)Screen.height || vector.z < 0f)
			{
				worldTextInstance2.m_gui.SetActive(false);
			}
			else
			{
				worldTextInstance2.m_gui.SetActive(true);
				worldTextInstance2.m_gui.transform.position = vector;
			}
		}
		if (worldTextInstance != null)
		{
			UnityEngine.Object.Destroy(worldTextInstance.m_gui);
			this.m_worldTexts.Remove(worldTextInstance);
		}
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x00028164 File Offset: 0x00026364
	private void AddInworldText(DamageText.TextType type, Vector3 pos, float distance, float dmg, bool mySelf)
	{
		DamageText.WorldTextInstance worldTextInstance = new DamageText.WorldTextInstance();
		worldTextInstance.m_worldPos = pos;
		worldTextInstance.m_gui = UnityEngine.Object.Instantiate<GameObject>(this.m_worldTextBase, base.transform);
		worldTextInstance.m_textField = worldTextInstance.m_gui.GetComponent<Text>();
		this.m_worldTexts.Add(worldTextInstance);
		Color white;
		if (type == DamageText.TextType.Heal)
		{
			white = new Color(0.5f, 1f, 0.5f, 0.7f);
		}
		else if (mySelf)
		{
			if (dmg == 0f)
			{
				white = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
			else
			{
				white = new Color(1f, 0f, 0f, 1f);
			}
		}
		else
		{
			switch (type)
			{
			case DamageText.TextType.Normal:
				white = new Color(1f, 1f, 1f, 1f);
				goto IL_16C;
			case DamageText.TextType.Resistant:
				white = new Color(0.6f, 0.6f, 0.6f, 1f);
				goto IL_16C;
			case DamageText.TextType.Weak:
				white = new Color(1f, 1f, 0f, 1f);
				goto IL_16C;
			case DamageText.TextType.Immune:
				white = new Color(0.6f, 0.6f, 0.6f, 1f);
				goto IL_16C;
			case DamageText.TextType.TooHard:
				white = new Color(0.8f, 0.7f, 0.7f, 1f);
				goto IL_16C;
			}
			white = Color.white;
		}
		IL_16C:
		worldTextInstance.m_textField.color = white;
		if (distance > this.m_smallFontDistance)
		{
			worldTextInstance.m_textField.fontSize = this.m_smallFontSize;
		}
		else
		{
			worldTextInstance.m_textField.fontSize = this.m_largeFontSize;
		}
		string text;
		switch (type)
		{
		case DamageText.TextType.Heal:
			text = "+" + dmg.ToString("0.#", CultureInfo.InvariantCulture);
			break;
		case DamageText.TextType.TooHard:
			text = Localization.instance.Localize("$msg_toohard");
			break;
		case DamageText.TextType.Blocked:
			text = Localization.instance.Localize("$msg_blocked: ") + dmg.ToString("0.#", CultureInfo.InvariantCulture);
			break;
		default:
			text = dmg.ToString("0.#", CultureInfo.InvariantCulture);
			break;
		}
		worldTextInstance.m_textField.text = text;
		worldTextInstance.m_timer = 0f;
	}

	// Token: 0x060004DA RID: 1242 RVA: 0x000283B0 File Offset: 0x000265B0
	public void ShowText(HitData.DamageModifier type, Vector3 pos, float dmg, bool player = false)
	{
		DamageText.TextType type2 = DamageText.TextType.Normal;
		switch (type)
		{
		case HitData.DamageModifier.Normal:
			type2 = DamageText.TextType.Normal;
			break;
		case HitData.DamageModifier.Resistant:
			type2 = DamageText.TextType.Resistant;
			break;
		case HitData.DamageModifier.Weak:
			type2 = DamageText.TextType.Weak;
			break;
		case HitData.DamageModifier.Immune:
			type2 = DamageText.TextType.Immune;
			break;
		case HitData.DamageModifier.VeryResistant:
			type2 = DamageText.TextType.Resistant;
			break;
		case HitData.DamageModifier.VeryWeak:
			type2 = DamageText.TextType.Weak;
			break;
		}
		this.ShowText(type2, pos, dmg, player);
	}

	// Token: 0x060004DB RID: 1243 RVA: 0x00028404 File Offset: 0x00026604
	public void ShowText(DamageText.TextType type, Vector3 pos, float dmg, bool player = false)
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write((int)type);
		zpackage.Write(pos);
		zpackage.Write(dmg);
		zpackage.Write(player);
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "DamageText", new object[]
		{
			zpackage
		});
	}

	// Token: 0x060004DC RID: 1244 RVA: 0x00028454 File Offset: 0x00026654
	private void RPC_DamageText(long sender, ZPackage pkg)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (!mainCamera)
		{
			return;
		}
		if (Hud.IsUserHidden())
		{
			return;
		}
		DamageText.TextType type = (DamageText.TextType)pkg.ReadInt();
		Vector3 vector = pkg.ReadVector3();
		float dmg = pkg.ReadSingle();
		bool flag = pkg.ReadBool();
		float num = Vector3.Distance(mainCamera.transform.position, vector);
		if (num > this.m_maxTextDistance)
		{
			return;
		}
		bool mySelf = flag && sender == ZNet.instance.GetUID();
		this.AddInworldText(type, vector, num, dmg, mySelf);
	}

	// Token: 0x0400051D RID: 1309
	private static DamageText m_instance;

	// Token: 0x0400051E RID: 1310
	public float m_textDuration = 1.5f;

	// Token: 0x0400051F RID: 1311
	public float m_maxTextDistance = 30f;

	// Token: 0x04000520 RID: 1312
	public int m_largeFontSize = 16;

	// Token: 0x04000521 RID: 1313
	public int m_smallFontSize = 8;

	// Token: 0x04000522 RID: 1314
	public float m_smallFontDistance = 10f;

	// Token: 0x04000523 RID: 1315
	public GameObject m_worldTextBase;

	// Token: 0x04000524 RID: 1316
	private List<DamageText.WorldTextInstance> m_worldTexts = new List<DamageText.WorldTextInstance>();

	// Token: 0x02000146 RID: 326
	public enum TextType
	{
		// Token: 0x040010C2 RID: 4290
		Normal,
		// Token: 0x040010C3 RID: 4291
		Resistant,
		// Token: 0x040010C4 RID: 4292
		Weak,
		// Token: 0x040010C5 RID: 4293
		Immune,
		// Token: 0x040010C6 RID: 4294
		Heal,
		// Token: 0x040010C7 RID: 4295
		TooHard,
		// Token: 0x040010C8 RID: 4296
		Blocked
	}

	// Token: 0x02000147 RID: 327
	private class WorldTextInstance
	{
		// Token: 0x040010C9 RID: 4297
		public Vector3 m_worldPos;

		// Token: 0x040010CA RID: 4298
		public GameObject m_gui;

		// Token: 0x040010CB RID: 4299
		public float m_timer;

		// Token: 0x040010CC RID: 4300
		public Text m_textField;
	}
}
