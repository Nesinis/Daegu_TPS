using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBase : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // summary ǥ�ð� �ȵȴٸ� vs�� ���� �Ѹ� ��
    /// <summary>
    /// �������� ���� �� ����ϴ� �Լ�
    /// </summary>
    /// <param name="atkPower">������ �� ������ ��</param>
    /// <param name="hitDir">�˹��� ��ų ���� ����</param>
    /// <param name="attacker">�������� Ʈ������ ������Ʈ</param>
    public virtual void TakeDamage(float atkPower, Vector3 hitDir, Transform attacker)
    {

    }
}
