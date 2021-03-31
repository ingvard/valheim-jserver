using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200004F RID: 79
public class DamageText : MonoBehaviour
{
	// Token: 0x17000007 RID: 7
	// (get) Token: 0x060004D6 RID: 1238 RVA: 0x00028065 File Offset: 0x00026265
	public static DamageText instance
	{
		get
		{
			return DamageText.m_instance;
		}
	}

	// Token: 0x060004D7 RID: 1239 RVA: 0x0002806C File Offset: 0x0002626C
	private void Awake()
	{
		DamageText.m_instance = this;
		ZRoutedRpc.instance.Register<ZPackage>("DamageText", new Action<long, ZPackage>(this.RPC_DamageText));
	}

	// Token: 0x060004D8 RID: 1240 RVA: 0x0002808F File Offset: 0x0002628F
	private void LateUpdate()
	{
		this.UpdateWorldTexts(Time.deltaTime);
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x0002809C File Offset: 0x0002629C
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

	// Token: 0x060004DA RID: 1242 RVA: 0x00028218 File Offset: 0x00026418
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

	// Token: 0x060004DB RID: 1243 RVA: 0x00028464 File Offset: 0x00026664
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

	// Token: 0x060004DC RID: 1244 RVA: 0x000284B8 File Offset: 0x000266B8
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

	// Token: 0x060004DD RID: 1245 RVA: 0x00028508 File Offset: 0x00026708
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

	// Token: 0x04000521 RID: 1313
	private static DamageText m_instance;

	// Token: 0x04000522 RID: 1314
	public float m_textDuration = 1.5f;

	// Token: 0x04000523 RID: 1315
	public float m_maxTextDistance = 30f;

	// Token: 0x04000524 RID: 1316
	public int m_largeFontSize = 16;

	// Token: 0x04000525 RID: 1317
	public int m_smallFontSize = 8;

	// Token: 0x04000526 RID: 1318
	public float m_smallFontDistance = 10f;

	// Token: 0x04000527 RID: 1319
	public GameObject m_worldTextBase;

	// Token: 0x04000528 RID: 1320
	private List<DamageText.WorldTextInstance> m_worldTexts = new List<DamageText.WorldTextInstance>();

	// Token: 0x02000146 RID: 326
	public enum TextType
	{
		// Token: 0x040010C9 RID: 4297
		Normal,
		// Token: 0x040010CA RID: 4298
		Resistant,
		// Token: 0x040010CB RID: 4299
		Weak,
		// Token: 0x040010CC RID: 4300
		Immune,
		// Token: 0x040010CD RID: 4301
		Heal,
		// Token: 0x040010CE RID: 4302
		TooHard,
		// Token: 0x040010CF RID: 4303
		Blocked
	}

	// Token: 0x02000147 RID: 327
	private class WorldTextInstance
	{
		// Token: 0x040010D0 RID: 4304
		public Vector3 m_worldPos;

		// Token: 0x040010D1 RID: 4305
		public GameObject m_gui;

		// Token: 0x040010D2 RID: 4306
		public float m_timer;

		// Token: 0x040010D3 RID: 4307
		public Text m_textField;
	}
}
