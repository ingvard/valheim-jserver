using System;
using System.Threading;
using UnityEngine;

// Token: 0x02000103 RID: 259
public class TestSceneCharacter : MonoBehaviour
{
	// Token: 0x06000F96 RID: 3990 RVA: 0x0006E386 File Offset: 0x0006C586
	private void Start()
	{
		this.m_body = base.GetComponent<Rigidbody>();
	}

	// Token: 0x06000F97 RID: 3991 RVA: 0x0006E394 File Offset: 0x0006C594
	private void Update()
	{
		Thread.Sleep(30);
		this.HandleInput(Time.deltaTime);
	}

	// Token: 0x06000F98 RID: 3992 RVA: 0x0006E3A8 File Offset: 0x0006C5A8
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

	// Token: 0x04000E5F RID: 3679
	public float m_speed = 5f;

	// Token: 0x04000E60 RID: 3680
	public float m_cameraDistance = 10f;

	// Token: 0x04000E61 RID: 3681
	private Rigidbody m_body;

	// Token: 0x04000E62 RID: 3682
	private Quaternion m_lookYaw = Quaternion.identity;

	// Token: 0x04000E63 RID: 3683
	private float m_lookPitch;
}
