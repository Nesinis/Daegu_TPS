using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    // Ÿ������ ������ ��ġ�� ���� �̵���Ų��.
    public Transform target;
    public float rotX;
    public bool dynamicCam = true;
    public float followSpeed = 3.0f;

    Transform player;
    

    void Start()
    {
        player = GameObject.Find("Player").transform;

    }

    void Update()
    {

        if (target != null && player != null)
        {
            // ī�޶��� ��ġ�� Ÿ�� Ʈ�������� ��ġ�� �����Ѵ�.
            //if (!dynamicCam)
            //{
            //    transform.position = target.position;
            //}
            //else
            //{
            //    transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSpeed);
            //}

            // ī�޶��� ���� ������ �÷��̾��� ���� �������� �����Ѵ�.
            //Vector3 dir = (player.position - transform.position).normalized;
            transform.forward = player.forward;

            // ������� ���콺 ���� ȸ�� ���� x�� ȸ������ �ִ´�.
            transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
