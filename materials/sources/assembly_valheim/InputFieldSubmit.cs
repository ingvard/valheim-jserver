using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000054 RID: 84
public class InputFieldSubmit : MonoBehaviour
{
	// Token: 0x06000524 RID: 1316 RVA: 0x0002B3C4 File Offset: 0x000295C4
	private void Awake()
	{
		this.m_field = base.GetComponent<InputField>();
	}

	// Token: 0x06000525 RID: 1317 RVA: 0x0002B3D4 File Offset: 0x000295D4
	private void Update()
	{
		if (this.m_field.text != "" && Input.GetKey(KeyCode.Return))
		{
			this.m_onSubmit(this.m_field.text);
			this.m_field.text = "";
		}
	}

	// Token: 0x040005A4 RID: 1444
	public Action<string> m_onSubmit;

	// Token: 0x040005A5 RID: 1445
	private InputField m_field;
}
