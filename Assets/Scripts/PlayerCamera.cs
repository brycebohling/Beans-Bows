using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Range(0.1f, 9f)][SerializeField] float sensitivity = 2f;
	[Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;

	Vector2 rotation = Vector2.zero;
	const string xAxis = "Mouse X";
	const string yAxis = "Mouse Y";

	private void Update()
    {
		rotation.x += Input.GetAxis(xAxis) * sensitivity;
		rotation.y += Input.GetAxis(yAxis) * sensitivity;
		rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
		Quaternion xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		Quaternion yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

		transform.localRotation = yQuat;
        PlayerC.Instance.transform.localRotation = xQuat;
	}
}
