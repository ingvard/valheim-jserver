using System;
using System.Threading;
using UnityEngine;

// Token: 0x02000103 RID: 259
public class TestSceneCharacter : MonoBehaviour
{
	// Token: 0x06000F97 RID: 3991 RVA: 0x0006E50E File Offset: 0x0006C70E
	private void Start()
	{
		this.m_body = base.GetComponent<Rigidbody>();
	}

	// Token: 0x06000F98 RID: 3992 RVA: 0x0006E51C File Offset: 0x0006C71C
	private void Update()
	{
		Thread.Sleep(30);
		this.HandleInput(Time.deltaTime);
	}

	// Token: 0x06000F99 RID: 3993 RVA: 0x0006E530 File Offset: 0x0006C730
	private void HandleInput(float dt)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxis("Mouse X");
		zero.y = Input.GetAxis("Mouse Y");
		if (Input.GetKey(KeyCode.Mouse1) || Cursor.lockState != CursorLockMode.None)
		{
			this.m_lookYaw *= Quaternion.Euler(0f, zero.x, 0f);
			this.m_lookPitch = Mathf.Clamp(this.m_lookPitch - zero.y, -89f, 89f);
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			if (Cursor.lockState == CursorLockMode.None)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}
		Vector3 a = Vector3.zero;
		if (Input.GetKey(KeyCode.A))
		{
			a -= base.transform.right * this.m_speed;
		}
		if (Input.GetKey(KeyCode.D))
		{
			a += base.transform.right * this.m_speed;
		}
		if (Input.GetKey(KeyCode.W))
		{
			a += base.transform.forward * this.m_speed;
		}
		if (Input.GetKey(KeyCode.S))
		{
			a -= base.transform.forward * this.m_speed;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.m_body.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
		}
		Vector3 force = a - this.m_body.velocity;
		force.y = 0f;
		this.m_body.AddForce(force, ForceMode.VelocityChange);
		base.transform.rotation = this.m_lookYaw;
		Quaternion rotation = this.m_lookYaw * Quaternion.Euler(this.m_lookPitch, 0f, 0f);
		mainCamera.transform.position = base.transform.position - rotation * Vector3.forward * this.m_cameraDistance;
		mainCamera.transform.LookAt(base.transform.position + Vector3.up);
	}

	// Token: 0x04000E65 RID: 3685
	public float m_speed = 5f;

	// Token: 0x04000E66 RID: 3686
	public float m_cameraDistance = 10f;

	// Token: 0x04000E67 RID: 3687
	private Rigidbody m_body;

	// Token: 0x04000E68 RID: 3688
	private Quaternion m_lookYaw = Quaternion.identity;

	// Token: 0x04000E69 RID: 3689
	private float m_lookPitch;
}
