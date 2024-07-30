using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = UnityEngine.Random;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState
    {
        Idle = 0,
        Patrol = 1,
        Trace = 2,
        Attack = 4,
        AttackDelay = 8,
        Return = 16,
        Damaged = 32,
        Dead = 64
    }

    public EnemyState myState = EnemyState.Idle;
    public bool drawGizmos = true;

    [Header("�⺻ �Ӽ�")]
    public EnemyInitPreferences initPreferences;

    public float maxHP = 100;


    float currentTime = 0;
    float idleTime = 3.0f;
    Vector3 patrolCenter;
    CharacterController cc;
    Vector3 patrolNext;
    float currentHP = 0;
    Vector3 hitDirection;
    
    [SerializeField]
    Transform target;

    void Start()
    {
        patrolCenter = transform.position;
        patrolNext = patrolCenter;
        cc = GetComponent<CharacterController>();
        currentHP = maxHP;
    }

    void Update()
    {
        // ���� ���� ���¿� ���� ������ �Լ��� �����Ѵ�.
        switch (myState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Trace:
                TraceTarget();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.AttackDelay:
                AttackDelay();
                break;
            case EnemyState.Return:
                ReturnHome();
                break;
            case EnemyState.Damaged:
                OnDamaged();
                break;
            case EnemyState.Dead:
                Dead();
                break;
        }
    }

    private void Idle()
    {
        CheckSight(initPreferences.sightRange, initPreferences.sightDistance);

        currentTime += Time.deltaTime;
        if(currentTime > idleTime)
        {
            currentTime = 0;
            myState = EnemyState.Patrol;
            print("My State: Idle -> Patrol");
        }

    }

    private void Patrol()
    {
        CheckSight(initPreferences.sightRange, initPreferences.sightDistance);

        // ���õ� �������� �̵��Ѵ�.
        Vector3 dir = patrolNext - transform.position;
        if (dir.magnitude > 0.1f)
        {
            cc.Move(dir.normalized * initPreferences.patrolSpeed * Time.deltaTime);
        }
        // �������� �����ϰ�, 2��~3�� ���̸�ŭ ����� ���� �ٸ� ������ ��÷�Ѵ�.
        else
        {
            // patrolRadius�� �ݰ����� �ϴ� ���� ������ ������ �����Ѵ�.
            #region 1. ���� �⺻ ���� ���
            //float h = Random.Range(-1.0f, 1.0f);
            //float v = Random.Range(-1.0f, 1.0f);
            //Vector3 newPos = new Vector3(h, 0, v).normalized * patrolRadius;
            //patrolNext = patrolCenter + newPos;
            #endregion

            #region 2. Random Ŭ������ �ִ� inside �Լ��� �̿��ؼ� �����ϴ� ���
            Vector2 newPos = Random.insideUnitCircle * initPreferences.patrolRadius;
            patrolNext = patrolCenter + new Vector3(newPos.x, 0, newPos.y);
            #endregion

            #region 3. �ﰢ �Լ��� �̿��� ����
            //float degree = Random.Range(-180.0f, 180.0f);
            //Vector3 newPos = new Vector3(Mathf.Cos(Mathf.Deg2Rad * degree), 0, Mathf.Sin(Mathf.Deg2Rad * degree));
            //float distance = Random.Range(0, patrolRadius);
            //patrolNext = patrolCenter + newPos * distance;
            #endregion

            myState = EnemyState.Idle;
            print("My State: Patrol -> Idle");
            idleTime = Random.Range(2.0f, 3.0f);
        }
    }

    void CheckSight(float degree, float maxDistance)
    {
        // �þ� ���� �ȿ� ���� ����� �ִٸ� �� ����� Ÿ������ �����ϰ� �ʹ�.
        // �þ� ����(�þ߰�: �¿� 30��, ����, �ִ� �þ� �Ÿ�: 15����)
        // ��� ������ ���� �±�(Player) ����
        target = null;

        // 1. ���� �ȿ� ��ġ�� ������Ʈ �߿� Tag�� "Player"�� ������Ʈ�� ��� ã�´�.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // 2. ã�� ������Ʈ�� �߿��� �Ÿ��� maxDistance �̳��� ������Ʈ�� ã�´�.
        for(int i = 0; i < players.Length; i++)
        {
            float distance = Vector3.Distance(players[i].transform.position, transform.position);

            if(distance <= maxDistance)
            {
                // 3. ã�� ������Ʈ�� �ٶ󺸴� ���Ϳ� ���� ���� ���͸� �����Ѵ�.
                Vector3 lookVector = players[i].transform.position - transform.position;
                lookVector.Normalize();

                float cosTheta = Vector3.Dot(transform.forward, lookVector);
                float theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;

                // 4-1. ����, ������ ��� ���� 0���� ũ��(������ ���ʿ� �ִ�)...
                // 4-2. ����, ���հ��� ���� 30���� ������(���� �¿� 30�� �̳�)...
                if(cosTheta > 0 && theta < degree)
                {
                    target = players[i].transform;

                    // ���¸� trace ���·� ��ȯ�Ѵ�.
                    myState = EnemyState.Trace;
                    print("My State: Idle/Patrol -> Trace");
                }
            }
        }
    }


    private void TraceTarget()
    {
        // ����, �ִ� �߰� �Ÿ��� ����ٸ�...
        if (Vector3.Distance(transform.position, patrolCenter) > initPreferences.maxTraceDistance)
        {
            // ���¸� Return ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Return;
            print("My State: Trace -> Return");
            return;
        }

        Vector3 dir = target.position - transform.position;

        if (dir.magnitude > initPreferences.attackRange)
        {
            // Ÿ���� ���� �߰� �̵��Ѵ�.
            cc.Move(dir.normalized * initPreferences.traceSpeed * Time.deltaTime);
        }
        else
        {
            // ���� ���� �̳��� ���� ���¸� Attack ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Attack;
            currentTime = 0;
            print("My State: Trace -> Attack");
        }
    }

    private void Attack()
    {
        // ������ �Ѵ�
        print("Attack Target!");

        // ���� �ִϸ��̼��� ������ ���� ��� ���·� ��ȯ�Ѵ�.
        myState = EnemyState.AttackDelay;
        print("My State: Attack -> AttackDelay");
    }

    private void AttackDelay()
    {
        // ����, Ÿ�ٰ� �Ÿ��� ���� ������ ������ ����ٸ�...
        if (Vector3.Distance(transform.position, target.position) > initPreferences.attackRange)
        {
            // �ٽ� �߰� ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Trace;
            print("My State: AttackDelay -> Trace");
            currentTime = 0;
            return;
        }

        // �����ð� ����Ѵ�.
        currentTime += Time.deltaTime;
        // ���� �ð��� �����ٸ� ���¸� �ٽ� ���� ���·� ��ȯ�Ѵ�.
        if(currentTime > 1.5f)
        {
            currentTime = 0;
            myState = EnemyState.Attack;
            print("My State: AttackDelay -> Attack");
        }

    }

    private void ReturnHome()
    {
        // patrolCenter ��ġ�� �ٽ� ���ư���.
        Vector3 dir = patrolCenter - transform.position;

        // �������� �����ߴٸ�...
        if (dir.magnitude < 0.1f)
        {
            transform.position = patrolCenter;

            // ���¸� Idle ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Idle;
            print("My State: Return -> Idle");
        }
        // �׷��� �ʾҴٸ�...
        else
        {
            cc.Move(dir.normalized * initPreferences.traceSpeed * Time.deltaTime);
        }

    }

    private void OnDamaged()
    {
        // ���� �ð� �ڷ� �������ٰ�(knock-back) ���¸� Trace ���·� ��ȯ�Ѵ�.
        transform.position = Vector3.Lerp(transform.position, hitDirection, 0.25f);

        if(Vector3.Distance(transform.position, hitDirection) < 0.1f)
        {
            myState = EnemyState.Trace;
            print("My State: Damaged -> Trace");
        }
    }

    // ������ ������ �������� �ο��ϴ� �Լ�
    public void TakeDamage(float atkPower, Vector3 hitDir, Transform attacker)
    {
        if(myState == EnemyState.Dead || myState == EnemyState.Return || myState == EnemyState.Damaged)
        {
            return;
        }

        // 1. ���� ü�¿� ����� ���ݷ¸�ŭ�� ���ҽ�Ų��(min 0 ~ max 100).
        currentHP = Mathf.Clamp(currentHP - atkPower, 0, maxHP);

        // 2. ����, �� ��� ���� ü���� 0 ���϶��...
        if (currentHP <= 0)
        {
            // 2-1. ���� ���¸� ���� ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Dead;
            print("My State: Any -> Dead");
            currentTime = 0;

            // 2-2. �ݶ��̴� ������Ʈ�� ��Ȱ��ȭ ó���Ѵ�.
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<CharacterController>().enabled = false;
        }
        // 3. �׷��� �ʴٸ�..
        else
        {
            // 3-1. ���� ���¸� ������ ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Damaged;
            print("My State: Any -> Damaged");
            
            // 3-2. Ÿ�� �������� ���� �Ÿ���ŭ�� �˹� ��ġ�� �����Ѵ�.
            hitDirection = transform.position + hitDir * 1.5f;
            // 3-3. �����ڸ� Ÿ������ �����Ѵ�.
            target = attacker;
        }
    }


    private void Dead()
    {
        // Ŭ���� ������ �Ͻÿ� �ʱ�ȭ�ϱ�
        //initPreferences = new EnemyInitPreferences(10, 5, 10, 1.5f, 30, 10, 30);

        // 3�� �ڿ� ���ŵȴ�.
        currentTime += Time.deltaTime;
        if(currentTime > 3.0f)
        {
            Destroy(gameObject);
        }
    }

    // �� �׸���
    private void OnDrawGizmos()
    {
        if(!drawGizmos)
        {
            return;
        }

        Gizmos.color = new Color32(154, 14, 235, 255);

        #region �� �׸���
        //List<Vector3> points = new List<Vector3>();
        //for (int i = 0; i < 360; i += 5)
        //{
        //    Vector3 point = new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad)) * 5;
        //    points.Add(transform.position + point);
        //}

        //for (int i = 0; i < points.Count - 1; i++)
        //{
        //    Gizmos.DrawLine(points[i], points[i + 1]);
        //}
        #endregion

        // �þ߰� �׸���
        float rightDegree = 90 - initPreferences.sightRange;
        float leftDegree = 90 + initPreferences.sightRange;

        Vector3 rightPos = new Vector3(Mathf.Cos(rightDegree * Mathf.Deg2Rad),
                                       0,
                                       Mathf.Sin(rightDegree * Mathf.Deg2Rad)) * initPreferences.sightDistance
                                + transform.position;

        Vector3 leftPos = new Vector3(Mathf.Cos(leftDegree * Mathf.Deg2Rad),
                                      0,
                                      Mathf.Sin(leftDegree * Mathf.Deg2Rad)) * initPreferences.sightDistance
                                + transform.position;

        Gizmos.DrawLine(transform.position, rightPos);
        Gizmos.DrawLine(transform.position, leftPos);

        // �ִ� �߰� �Ÿ� �׸���
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolCenter, initPreferences.maxTraceDistance);
    }
}

// ����ȭ Ŭ����
[System.Serializable]
public class EnemyInitPreferences
{
    public float patrolRadius = 4.0f;
    public float patrolSpeed = 5.0f;
    public float traceSpeed = 9.0f;
    public float attackRange = 2.0f;
    [Range(0.0f, 90.0f)]
    public float sightRange = 30.0f;
    public float sightDistance = 15.0f;
    public float maxTraceDistance = 25.0f;

    // ������ �Լ�
    public EnemyInitPreferences(float patrolRadius, float patrolSpeed, float traceSpeed, float attackRange, float sightRange, float sightDistance, float maxTraceDistance)
    {
        this.patrolRadius = patrolRadius;
        this.patrolSpeed = patrolSpeed;
        this.traceSpeed = traceSpeed;
        this.attackRange = attackRange;
        this.sightRange = Mathf.Clamp(sightRange, 0, 90.0f);
        this.sightDistance = sightDistance;
        this.maxTraceDistance = maxTraceDistance;
    }
}