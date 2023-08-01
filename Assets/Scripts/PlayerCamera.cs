using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerCamera : NetworkBehaviour
{
    public static PlayerCamera Instance { get; private set; }

    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] AudioListener audioListener;

    Transform bowArrowHolder;

    [Range(0.1f, 9f)][SerializeField] float sensitivity = 2f;
	[Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;

	Vector2 rotation = Vector2.zero;
	const string xAxis = "Mouse X";
	const string yAxis = "Mouse Y";


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Instance = this;
            
            vCam.Priority = 1;
            audioListener.enabled = true;

        } else
        {
            vCam.Priority = 0;
        }
    }

	private void Update()
    {
        if (!IsOwner) return;

		rotation.x += Input.GetAxis(xAxis) * sensitivity;
		rotation.y += Input.GetAxis(yAxis) * sensitivity;
		rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
		Quaternion xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		Quaternion yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

		transform.localRotation = yQuat;
        
        if (PlayerC.Instance.BowArrowHolder != null)
        {
            PlayerC.Instance.BowArrowHolder.localRotation = yQuat;
        }
        
        PlayerC.Instance.transform.rotation = xQuat;
	}
}
