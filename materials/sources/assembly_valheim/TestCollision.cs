using System;
using UnityEngine;

// Token: 0x02000102 RID: 258
public class TestCollision : MonoBehaviour
{
	// Token: 0x06000F92 RID: 3986 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Start()
	{
	}

	// Token: 0x06000F93 RID: 3987 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Update()
	{
	}

	// Token: 0x06000F94 RID: 3988 RVA: 0x0006E2D8 File Offset: 0x0006C4D8
	public void OnCollisionEnter(Collision info)
	{
		ZLog.Log("Hit by " + info.rigidbody.gameObject.name);
		ZLog.Log(string.Concat(new object[]
		{
			"rel vel ",
			info.relativeVelocity,
			" ",
			info.relativeVelocity
		}));
		ZLog.Log(string.Concat(new object[]
		{
			"Vel ",
			info.rigidbody.velocity,
			"  ",
			info.rigidbody.angularVelocity
		}));
	}
}
