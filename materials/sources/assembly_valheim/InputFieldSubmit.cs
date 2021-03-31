using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000054 RID: 84
public class InputFieldSubmit : MonoBehaviour
{
	// Token: 0x06000525 RID: 1317 RVA: 0x0002B478 File Offset: 0x00029678
	private void Awake()
	{
		this.m_field = base.GetComponent<InputField>();
	}

	// Token: 0x06000526 RID: 1318 RVA: 0x0002B488 File Offset: 0x00029688
	private void Update()
	{
		if (this.m_field.text != "" && Input.GetKey(KeyCode.Return))
		{
			this.m_onSubmit(this.m_field.text);
			this.m_field.text = "";
		}
	}

	// Token: 0x040005A8 RID: 1448
	public Action<string> m_onSubmit;

	// Token: 0x040005A9 RID: 1449
	private InputField m_field;
}
