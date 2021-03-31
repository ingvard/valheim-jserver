using System;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class ItemStyle : MonoBehaviour, IEquipmentVisual
{
	// Token: 0x06000152 RID: 338 RVA: 0x0000A64E File Offset: 0x0000884E
	public void Setup(int style)
	{
		base.GetComponent<Renderer>().material.SetFloat("_Style", (float)style);
	}
}
