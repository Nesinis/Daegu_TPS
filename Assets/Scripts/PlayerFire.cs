using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    public GameObject bulletFXObject;
    public GameObject grenadePrefab;
    public Vector3 direction;
    public float throwPower = 5;
    public Transform firePosition;

    // ����ź ���� �׸���� ����
    public float simulationTime = 5.0f;
    public float interval = 0.1f;

    List<Vector3> trajectory = new List<Vector3>();
    ParticleSystem bulletEffect;

    void Start()
    {
        // Ŀ���� ���Ӻ� �ȿ� ���д�.
        Cursor.lockState = CursorLockMode.Locked;

        bulletEffect = bulletFXObject.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        FireType1();
        FireType2();
    }

    
    void FireType1()
    {
        // ����, ���콺 ���� ��ư�� �����ٸ�, ���� ���� �������� �Ѿ��� �߻��ϰ� �ʹ�.
        // 1. ���콺 ���� ��ư �Է� üũ
        if (Input.GetMouseButtonDown(0))
        {
            // 2. ����, ���� ����, üũ �Ÿ�
            // 2-1. ���̸� �����.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 2-2. ���̰� �浹�� ����� ������ ��� ���� ����ü�� �����Ѵ�.
            RaycastHit hitInfo;

            // 2-3. ������� ���̸� ������ ����� �Ÿ���ŭ �߻��Ѵ�.
            bool isHit = Physics.Raycast(ray, out hitInfo, 1000);

            // 2-4. ����, ���̰� �浹�� �ߴٸ� ���̰� ���� ��ġ�� �Ѿ� ����Ʈ�� ǥ���Ѵ�.
            if (isHit)
            {
                //print(hitInfo.transform.name);
                //GameObject go = Instantiate(bulletFXObject, hitInfo.point, Quaternion.identity);

                bulletFXObject.transform.position = hitInfo.point;
                bulletEffect.Play();
            }
        }
    }

    void FireType2()
    {
        // ����, ���콺 ���� ��ư�� ������ �ִٸ�...
        if(Input.GetMouseButton(1))
        {
            // ����ź�� ���ư��� ������ �׸���.
            Vector3 startPos = firePosition.position;
            Vector3 dir = transform.TransformDirection(direction);
            dir.Normalize();
            Vector3 gravity = Physics.gravity;
            int simulCount = (int)(simulationTime / interval);

            trajectory.Clear();
            for(int i = 0; i < simulCount; i++)
            {
                float currentTime = interval * i;

                // p = p0 + vt - 0.5 * g * t * t;
                Vector3 result = startPos + dir * throwPower * currentTime + 0.5f * gravity * currentTime * currentTime;

                trajectory.Add(result);
            }

        }
        // ����, ���콺�� ���� ��ư�� �����ٰ� ����...
        else if (Input.GetMouseButtonUp(1))
        {
            // ����ź �������� �����ϰ�, ���������� �߻��Ѵ�.
            GameObject bomb = Instantiate(grenadePrefab, firePosition.position, firePosition.rotation);

            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            if(rb != null)
            {
                Vector3 dir = transform.TransformDirection(direction);
                dir.Normalize();

                // ���������� �߻��ϱ�
                rb.AddForce(dir * throwPower, ForceMode.Impulse);
            }
        }
    }


    // Scene View�� ����� �׸��� �̺�Ʈ �Լ�
    private void OnDrawGizmos()
    {
        // trajectory ����Ʈ�� ���� ������ �Լ� ����
        if(trajectory.Count < 1)
        {
            return;
        }

        // ������ ������ ������� �����Ѵ�.
        Gizmos.color = Color.green;

        // trajectory ����Ʈ�� ��� ��ȣ�� �����Ͽ� ������ �׸���.
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            Gizmos.DrawLine(trajectory[i], trajectory[i + 1]);
        }

    }

}
