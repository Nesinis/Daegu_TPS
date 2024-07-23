using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7.0f;
    public float rotSpeed = 200.0f;
    public float yVelocity = 2;
    public float jumpPower = 4;
    public int maxJumpCount = 1;

    // ȸ�� ���� �̸� ����ϱ� ���� ȸ����(x, y) ����
    float rotX;
    float rotY;
    float yPos;
    int currentJumpCount = 0;

    CharacterController cc;

    // �߷��� �����ϰ� �ʹ�.
    // �ٴڿ� �浹�� ���� �������� �Ʒ��� ��� �������� �ϰ� �ʹ�.
    // ����: �Ʒ�, ũ��: �߷�
    Vector3 gravityPower;

    void Start()
    {
        // ������ ȸ�� ���´�� ������ �ϰ� �ʹ�(���� ���� �ʱ�ȭ).
        rotX = transform.eulerAngles.x;
        rotY = transform.eulerAngles.y;

        // ĳ���� ��Ʈ�ѷ� ������Ʈ�� ������ ��Ƴ��´�.
        cc = GetComponent<CharacterController>();

        // �߷� ���� �ʱ�ȭ�Ѵ�.
        gravityPower = Physics.gravity;
    }

    void Update()
    {
        Move();
        Rotate();

    }

    // "Horizontal"�� "Vertical" �Է��� �̿��ؼ� ��������� �̵��ϰ� �ϰ� �ʹ�.
    // 1. ������� �Է��� �޴´�.
    // 2. ����, �ӷ��� ����Ѵ�.
    // 3. �� �����Ӹ��� ���� �ӵ��� �ڽ��� ��ġ�� �����Ѵ�.
    void Move()
    {
        // 1. ���� �̵� ���
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // ���� ���� ���⿡ ���� �̵��ϵ��� �����Ѵ�.
        // 1-1. ���� ���� ���͸� �̿��ؼ� ����ϴ� ���
        //Vector3 dir = transform.forward * v + transform.right * h;
        //dir.Normalize();

        // 1-2. ���� ȸ�� ���� ���� ���� ���� ���͸� ���� ������ ���ͷ� ��ȯ�ϴ� �Լ��� �̿��ϴ� ���
        Vector3 dir = new Vector3(h, 0, v); // ���� ���� ����
        dir = transform.TransformDirection(dir);
        dir.Normalize();

        // 2. ���� �̵� ���

        // �߷� ����
        yPos += gravityPower.y * yVelocity * Time.deltaTime;

        // �ٴڿ� ������� ������ yPos�� ���� 0���� �ʱ�ȭ�Ѵ�.
        if(cc.collisionFlags == CollisionFlags.CollidedBelow)
        {
            yPos = 0;
            currentJumpCount = 0;
        }

        // Ű������ �����̽��ٸ� ������ �������� ������ �ϰ� �ϰ� �ʹ�.
        if (Input.GetButtonDown("Jump") && currentJumpCount < maxJumpCount)
        {
            yPos = jumpPower;
            currentJumpCount++;
        }

        dir.y = yPos;

        //transform.position += dir * moveSpeed * Time.deltaTime;
        cc.Move(dir * moveSpeed * Time.deltaTime);
        //cc.SimpleMove(dir * moveSpeed);
    }

    // ������� ���콺 �巡�� ���⿡ ���� ���� �����¿� ȸ���� �ǰ� �ϰ� �ʹ�.
    // 1. ������� ���콺 �巡�� �Է��� �޴´�.
    // 2. ȸ�� �ӷ�, ȸ�� ������ �ʿ��ϴ�.
    // 3. �� �����Ӹ��� ���� �ӵ��� �ڽ��� ȸ������ �����Ѵ�.
    void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // �� �� ���� ȸ�� ���� �̸� ����Ѵ�(R = R0 + vt).
        rotX += mouseY * rotSpeed * Time.deltaTime;
        rotY += mouseX * rotSpeed * Time.deltaTime;

        // ���� ȸ���� -60�� ~ +60�������� �����Ѵ�.
        if (rotX > 60.0f)
        {
            rotX = 60.0f;
        }
        else if (rotX < -60.0f)
        {
            rotX = -60.0f;
        }

        // ���� ȸ�� ���� ���� Ʈ������ ȸ�� ������ �����Ѵ�.
        transform.eulerAngles = new Vector3(-rotX, rotY, 0);
    }

}
