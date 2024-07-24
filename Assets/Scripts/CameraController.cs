using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // �迭(Array)
    //Transform[] camPositions = new Transform[2];

    // ����Ʈ(List)
    List<Transform> camList = new List<Transform>();

    FollowCamera followCam;
    float currentRate = 0;


    void Start()
    {
        // FollowCamera ������Ʈ�� ĳ���Ѵ�.
        followCam = Camera.main.gameObject.GetComponent<FollowCamera>();

        // �ڽ� ���ӿ�����Ʈ �߿��� �ι�°(Near)�� ����°(Far) ������Ʈ�� ã�Ƽ� �迭�� �ִ´�.
        //camPositions[0] = transform.GetChild(1);
        //camPositions[1] = transform.GetChild(2);

        camList.Add(transform.GetChild(1));
        camList.Add(transform.GetChild(2));
        
        // �ʱ� ī�޶�� Near ī�޶�(1��Ī)�� �Ѵ�.
        ChangeCamTarget(0, false);
    }

    void Update()
    {
        // ����, ���� Ű 1���� ������ NearPos�� Ÿ������ �����ϰ�,
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeCamTarget(0, false);
        }
        // 2���� ������ FarPos�� Ÿ������ �����Ѵ�.
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCamTarget(1, true);
        }


        currentRate -= Input.GetAxis("Mouse ScrollWheel") * 0.5f;
        //currentRate += Time.deltaTime *0.5f;
        currentRate = Mathf.Clamp(currentRate, 0.0f, 1.0f);

        Camera.main.transform.position = Vector3.Lerp(camList[0].position, camList[1].position, currentRate);

    }

    void ChangeCamTarget(int targetNum, bool isDynamic)
    {
        // ���� ī�޶��� FollowCamera Ŭ������ �ִ� target�� 0�� ��Ҹ� �ִ´�.
        
        if (followCam != null)
        {
            //followCam.target = camPositions[targetNum];
            followCam.target = camList[targetNum];
            followCam.dynamicCam = isDynamic;
        }
    }
}
