using System;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class ItemStyle : MonoBehaviour, IEquipmentVisual
{
	// Token: 0x06000151 RID: 337 RVA: 0x0000A60E File Offset: 0x0000880E
	public void Setup(int style)
	{
		base.GetComponent<Renderer>().material.SetFloat("_Style", (float)style);
	}
}
