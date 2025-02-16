using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyFSM : ActorBase
{
    public enum EnemyState
    {
        Idle = 0,
        Patrol = 1,
        Trace = 2,
        MeleeAttack = 4,
        FarAttack = 8,
        AttackDelay = 16,
        Return = 32,
        Damaged = 64,
        Dead = 128
    }

    public EnemyState myState = EnemyState.Idle;
    public bool drawGizmos = true;

    [Header("기본 속성")]
    public EnemyInitPreferences initPreferences;
    public EnemyStateBase myStatus;
    public Slider hpSlider;
    public Animator enemyAnim;
    
    public float attackType = 0;

    float currentTime = 0;
    float idleTime = 3.0f;
    Vector3 patrolCenter;
    CharacterController cc;
    Vector3 patrolNext;
    Vector3 hitDirection;
    float[] idleBlendValue = new float[] { 0, 0.5f, 1.0f };
    int idleNumber = 0;
    NavMeshAgent smith;

    [SerializeField]
    Transform target;

    void Start()
    {
        patrolCenter = transform.position;
        patrolNext = patrolCenter;
        cc = GetComponent<CharacterController>();
        //myStatus.Initialize(100, 9);
        //myStatus.patrolSpeed = 5;
        hpSlider.value = myStatus.currentHP / myStatus.maxHP;
        smith = GetComponent<NavMeshAgent>();
        if(smith != null)
        {
            smith.speed = myStatus.patrolSpeed;
            smith.angularSpeed = 300.0f;
            smith.acceleration = 35.0f;
            smith.autoBraking = true;
            smith.autoTraverseOffMeshLink = false;
            smith.stoppingDistance = 1.0f;
        }
    }

    void Update()
    {
        // 나의 현재 상태에 따라 각각의 함수를 실행한다.
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
            case EnemyState.MeleeAttack:
                //Attack();
                break;
            case EnemyState.FarAttack:
                
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
            enemyAnim.SetBool("PatrolStart", true);

            // Idle 애니메이션이 0,1,2가 순환해서 선택되도록 설정한다.
            idleNumber = (idleNumber + 1) % 3;
            float selectedIdle = idleBlendValue[idleNumber];
            enemyAnim.SetFloat("SelectedIdle", selectedIdle);

            // 네비메시 에이전트의 목적지로 PatrolNext 지점을 설정한다.
            smith.SetDestination(patrolNext);
        }

    }

    private void Patrol()
    {
        CheckSight(initPreferences.sightRange, initPreferences.sightDistance);

        // 선택된 지점으로 이동한다.
        Vector3 dir = patrolNext - transform.position;
        //dir.y = 0;

        if (dir.magnitude > 1.0f)
        {
            //cc.Move(dir.normalized * myStatus.patrolSpeed * Time.deltaTime);
            // 이동하려는 방향으로 회전한다.
            //transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
        // 목적지에 도달하고, 2초~3초 사이만큼 대기한 다음 다른 지점을 추첨한다.
        else
        {
            // patrolRadius를 반경으로 하는 원의 임의의 지점을 선택한다.
            #region 1. 벡터 기본 연산 방식
            //float h = Random.Range(-1.0f, 1.0f);
            //float v = Random.Range(-1.0f, 1.0f);
            //float distance = Random.Range(0, initPreferences.patrolRadius);
            //Vector3 newPos = new Vector3(h, 0, v).normalized * distance;
            //patrolNext = patrolCenter + newPos;
            #endregion

            #region 2. Random 클래스에 있는 inside 함수를 이용해서 연산하는 방식
            Vector2 newPos = Random.insideUnitCircle * initPreferences.patrolRadius;
            patrolNext = patrolCenter + new Vector3(newPos.x, 0, newPos.y);
            #endregion

            #region 3. 삼각 함수를 이용한 계산식
            //float degree = Random.Range(-180.0f, 180.0f);
            //Vector3 newPos = new Vector3(Mathf.Cos(Mathf.Deg2Rad * degree), 0, Mathf.Sin(Mathf.Deg2Rad * degree));
            //float distance = Random.Range(0, initPreferences.patrolRadius);
            //patrolNext = patrolCenter + newPos * distance;
            #endregion

            myState = EnemyState.Idle;
            print("My State: Patrol -> Idle");
            idleTime = Random.Range(2.0f, 3.0f);
            enemyAnim.SetBool("PatrolStart", false);

            // 강제로 네비메시 에이전트를 멈춘다.
            smith.isStopped = true;
            smith.ResetPath();
        }
    }

    void CheckSight(float degree, float maxDistance)
    {
        // 시야 범위 안에 들어온 대상이 있다면 그 대상을 타겟으로 설정하고 싶다.
        // 시야 범위(시야각: 좌우 30도, 전방, 최대 시야 거리: 15미터)
        // 대상 선택을 위한 태그(Player) 설정
        target = null;

        // 1. 월드 안에 배치된 오브젝트 중에 Tag가 "Player"인 오브젝트를 모두 찾는다.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // 2. 찾은 오브젝트들 중에서 거리가 maxDistance 이내인 오브젝트만 찾는다.
        for(int i = 0; i < players.Length; i++)
        {
            float distance = Vector3.Distance(players[i].transform.position, transform.position);

            if(distance <= maxDistance)
            {
                // 3. 찾은 오브젝트를 바라보는 벡터와 나의 전방 벡터를 내적한다.
                Vector3 lookVector = players[i].transform.position - transform.position;
                lookVector.Normalize();

                float cosTheta = Vector3.Dot(transform.forward, lookVector);
                float theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;

                // 4-1. 만일, 내적의 결과 값이 0보다 크면(나보다 앞쪽에 있다)...
                // 4-2. 만일, 사잇각의 값이 30보다 작으면(전방 좌우 30도 이내)...
                if(cosTheta > 0 && theta < degree)
                {
                    target = players[i].transform;

                    // 상태를 trace 상태로 전환한다.
                    myState = EnemyState.Trace;
                    print("My State: Idle/Patrol -> Trace");
                    enemyAnim.SetTrigger("Trace");
                    attackType = Random.Range(0, 1.0f);
                }
            }
        }
    }


    private void TraceTarget()
    {
        // 만일, 최대 추격 거리를 벗어났다면...
        if (Vector3.Distance(transform.position, patrolCenter) > initPreferences.maxTraceDistance)
        {
            // 상태를 Return 상태로 전환한다.
            myState = EnemyState.Return;
            print("My State: Trace -> Return");
            enemyAnim.SetBool("Returning", true);

            smith.speed = myStatus.speed;
            smith.SetDestination(patrolCenter);

            return;
        }

        // 삼항 연산자
        float selectedRange = attackType > 0.5f ? initPreferences.attackRange : initPreferences.farAttackRange;
        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.magnitude > selectedRange)
        {
            // 타겟을 향해 추격 이동한다.
            //cc.Move(dir.normalized * myStatus.speed * Time.deltaTime);
            //transform.rotation = Quaternion.LookRotation(dir.normalized);
            smith.SetDestination(target.position);
            smith.speed = myStatus.speed;
        }
        else
        {
            // 공격 범위 이내로 들어가면 상태를 Attack 상태로 전환한다.
            currentTime = 0;

            if (dir.magnitude > initPreferences.attackRange)
            {
                myState = EnemyState.FarAttack;
                print("My State: Trace -> FarAttack");
                enemyAnim.SetTrigger("FarAttack");
            }
            else
            {
                myState = EnemyState.MeleeAttack;
                print("My State: Trace -> MeleeAttack");
                enemyAnim.SetTrigger("Attack");
            }

            smith.isStopped = true;
            smith.ResetPath();
        }
    }

    public void Attack()
    {
        // 공격을 한다
        target.GetComponent<PlayerMove>().TakeDamage(20, Vector3.zero, transform);

        // 공격 애니메이션이 끝나면...
        #region 애니메이션 상태 정보로부터 진행 상황을 가져오기
        // 현재 진행중인 애니메이션 상태 정보를 가져온다.
        //AnimatorStateInfo stateInfo = enemyAnim.GetCurrentAnimatorStateInfo(0);

        // 근접 공격의 상태를 해시데이터로 변환한다.
        //int attackHash = Animator.StringToHash("Base Layer.AttackMelee");

        // 만일, 현재 진행 중인 상태가 근접 공격이라면...
        //if(stateInfo.fullPathHash == attackHash)
        //{
        //print("Length: " + stateInfo.length);
        //print("Progress: " + stateInfo.normalizedTime);

        //    if(stateInfo.normalizedTime > 1.0f)
        //    {
        //        // 공격 대기 상태로 전환한다.
        //        myState = EnemyState.AttackDelay;
        //        print("My State: Attack -> AttackDelay");   
        //    }
        //}
        #endregion

    }

    private void AttackDelay()
    {
        // 만일, 타겟과 거리가 공격 가능한 범위를 벗어났다면...
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > initPreferences.farAttackRange)
        {
            // 다시 추격 상태로 전환한다.
            myState = EnemyState.Trace;
            print("My State: AttackDelay -> Trace");
            enemyAnim.SetTrigger("Trace");
            currentTime = 0;
            return;
        }
        else if (dist > initPreferences.attackRange)
        {
            // 일정시간 대기한다.
            currentTime += Time.deltaTime;
            // 일정 시간이 지났다면 상태를 원거리 공격 상태로 전환한다.
            if (currentTime > 1.5f)
            {
                currentTime = 0;
                myState = EnemyState.FarAttack;
                print("My State: AttackDelay -> FarAttack");
                enemyAnim.SetTrigger("FarAttack");
            }
        }
        else
        {
            // 일정시간 대기한다.
            currentTime += Time.deltaTime;
            // 일정 시간이 지났다면 상태를 근거리 공격 상태로 전환한다.
            if (currentTime > 1.5f)
            {
                currentTime = 0;
                myState = EnemyState.MeleeAttack;
                print("My State: AttackDelay -> MeleeAttack");
                enemyAnim.SetTrigger("Attack");
            }
        }
    }

    private void ReturnHome()
    {
        // patrolCenter 위치로 다시 돌아간다.
        Vector3 dir = patrolCenter - transform.position;
        dir.y = 0;

        // 목적지에 근접했다면...
        if (dir.magnitude < 1.1f)
        {
            // 네비게이션을 정지한다.
            smith.isStopped = true;
            smith.ResetPath();

            transform.position = patrolCenter;

            // 상태를 Idle 상태로 전환한다.
            myState = EnemyState.Idle;
            print("My State: Return -> Idle");
            enemyAnim.SetBool("Returning", false);

            smith.speed = myStatus.patrolSpeed;
        }
        // 그렇지 않았다면...
        //else
        //{
        //    cc.Move(dir.normalized * myStatus.speed * Time.deltaTime);
        //    transform.rotation = Quaternion.LookRotation(dir.normalized);
        //}

    }

    private void OnDamaged()
    {
        // 일정 시간 뒤로 물러났다가(knock-back) 상태를 Trace 상태로 전환한다.
        transform.position = Vector3.Lerp(transform.position, hitDirection, 0.05f);

        if(Vector3.Distance(transform.position, hitDirection) < 0.1f)
        {
            myState = EnemyState.Trace;
            print("My State: Damaged -> Trace");
            //enemyAnim.SetTrigger("Trace");
            enemyAnim.SetBool("Hit", false);

            smith.speed = myStatus.speed;
        }
    }

    // 상대방이 나에게 데미지를 부여하는 함수
    public override void TakeDamage(float atkPower, Vector3 hitDir, Transform attacker)
    {
        // 부모에 구현된 TakeDamage 함수를 먼저 실행한다.
        base.TakeDamage(atkPower, hitDir, attacker);

        if(myState == EnemyState.Dead || myState == EnemyState.Return || myState == EnemyState.Damaged)
        {
            return;
        }

        // 1. 현재 체력에 상대의 공격력만큼을 감소시킨다(min 0 ~ max 100).
        myStatus.currentHP = Mathf.Clamp(myStatus.currentHP - atkPower, 0, myStatus.maxHP);

        // 1-2. 체력 슬라이더 UI에 현재 체력을 표시한다.
        hpSlider.value = myStatus.currentHP / myStatus.maxHP;

        // 2. 만일, 그 결과 현재 체력이 0 이하라면...
        if (myStatus.currentHP <= 0)
        {
            // 2-1. 나의 상태를 죽음 상태로 전환한다.
            myState = EnemyState.Dead;
            print("My State: Any -> Dead");
            currentTime = 0;
            enemyAnim.SetTrigger("Die");

            // 2-2. 콜라이더 컴포넌트를 비활성화 처리한다.
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<CharacterController>().enabled = false;
        }
        // 3. 그렇지 않다면..
        else
        {
            // 3-1. 나의 상태를 데미지 상태로 전환한다.
            myState = EnemyState.Damaged;
            print("My State: Any -> Damaged");
            enemyAnim.SetBool("Hit", true);

            // 3-2. 타격 방향으로 일정 거리만큼을 넉백 위치로 지정한다.
            hitDir.y = 0;
            hitDirection = transform.position + hitDir * 2.5f;
            smith.isStopped = true;
            smith.ResetPath();

            // 3-3. 공격자를 타겟으로 설정한다.
            target = attacker;
        }
    }


    private void Dead()
    {
        // 클래스 변수를 일시에 초기화하기
        //initPreferences = new EnemyInitPreferences(10, 5, 10, 1.5f, 30, 10, 30);

        // 4초 뒤에 제거된다.
        currentTime += Time.deltaTime;
        if(currentTime > 4.0f)
        {
            Destroy(gameObject);
        }
    }

    // 원 그리기
    private void OnDrawGizmos()
    {
        if(!drawGizmos)
        {
            return;
        }

        Gizmos.color = new Color32(154, 14, 235, 255);

        #region 원 그리기
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

        // 시야각 그리기
        float rightDegree = 90 - initPreferences.sightRange;
        float leftDegree = 90 + initPreferences.sightRange;

        Vector3 rightPos = (transform.right * Mathf.Cos(rightDegree * Mathf.Deg2Rad) +
                           transform.forward * Mathf.Sin(rightDegree * Mathf.Deg2Rad)) * initPreferences.sightDistance
                           + transform.position;

        Vector3 leftPos = (transform.right * Mathf.Cos(leftDegree * Mathf.Deg2Rad) +
                           transform.forward * Mathf.Sin(leftDegree * Mathf.Deg2Rad)) * initPreferences.sightDistance
                          + transform.position;

        Gizmos.DrawLine(transform.position, rightPos);
        Gizmos.DrawLine(transform.position, leftPos);

        // 최대 추격 거리 그리기
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolCenter, initPreferences.maxTraceDistance);
    }
}

// 직렬화 클래스
[System.Serializable]
public class EnemyInitPreferences
{
    public float patrolRadius = 4.0f;
    public float attackRange = 1.5f;
    public float farAttackRange = 4.0f;
    [Range(0.0f, 90.0f)]
    public float sightRange = 30.0f;
    public float sightDistance = 15.0f;
    public float maxTraceDistance = 25.0f;

    // 생성자 함수
    public EnemyInitPreferences(float patrolRadius, float attackRange, float sightRange, float sightDistance, float maxTraceDistance)
    {
        this.patrolRadius = patrolRadius;
        this.attackRange = attackRange;
        this.sightRange = Mathf.Clamp(sightRange, 0, 90.0f);
        this.sightDistance = sightDistance;
        this.maxTraceDistance = maxTraceDistance;
    }
}