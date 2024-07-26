using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Attribute�� �ʿ� ������Ʈ �ڵ� �߰��ϱ�
[RequireComponent(typeof(Rigidbody))]
public class GrenadeExplosion : MonoBehaviour
{
    [Header("���� �ݰ�")]
    [Range(0.0f, 10.0f)]
    public float radius = 5.0f;
    
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
  
    // ����, �ٴڿ� ��Ҵٸ�...
    private void OnCollisionEnter(Collision collision)
    {
        // ���� ���� ��ġ�� �������� �ݰ� radius ���� ���� ��� ���� ������Ʈ�� ã�´�(�迭).
        Collider[] cols = Physics.OverlapSphere(transform.position, radius, 1<<6);

        // ����, ã���� ���� �ִٸ�
        if (cols.Length > 0)
        {
            // ã�� ������Ʈ�� ��� �ı��Ѵ�.
            for(int i = 0; i < cols.Length; i++)
            {
                Destroy(cols[i].gameObject);
            }
        }

        Destroy(gameObject);
    }
}
