using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float sensitivity;
	// [SerializeField] Transform playerTransform;
	[Range(0f, 90f)][SerializeField] float yRotationLimit = 90f;

	float xRot;
	float mouseX;
	float mouseY;

	private void Update()
	{
		mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
		mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

		xRot -= mouseY;
		xRot = Mathf.Clamp(xRot, -yRotationLimit, yRotationLimit);

        transform.localRotation = Quaternion.Euler(new Vector3(xRot, 0, 0));
		PlayerC.Instance.transform.Rotate(new Vector3(0, mouseX, 0));
	}
}
