using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Lotto : MonoBehaviour
{
    // ������ ���� ���� ���� �߿��� �����ϰ� ���ڸ� �̾Ƽ� �����ϰ� �ʹ�.
    public int MaxNumber = 12;
    public int numCount = 12;

    public List<int> luckyNumbers = new List<int>();
    public List<string> teamName = new List<string>();
    public List<Text> uiNames = new List<Text>();

    void Start()
    {
        //luckyNumbers.Capacity = numCount;

    }
    

    public void DrawLottoNumbers()
    {
        // 1. �ִ� ���ڸ�ŭ ������ ���ڸ� �̴´�.
        for(int i = 0; i < numCount; i++)
        {
            int num = Random.Range(0, MaxNumber);
            luckyNumbers[i] = num;

            // 2. Ȥ�� �ߺ��� ���ڰ� �ִ��� Ȯ���Ѵ�.
            for (int j = 0; j < i; j++)
            {
                if(num == luckyNumbers[j])
                {
                    // 3. �ߺ��� ���ڰ� �־��ٸ� ����÷�ϱ�� �ϰ� �ߺ� �˻縦 �����Ѵ�.
                    i--;
                    break;
                }
            }
        }

        Invoke("ShowResults", 3.0f);
    }

    void ShowResults()
    {
        // UI Text�� ����ϱ�
        for (int i = 0; i < luckyNumbers.Count; i++)
        {
            //result += luckyNumbers[i].ToString() + ", ";
            //print(teamName[luckyNumbers[i]]);
            uiNames[i].text = teamName[luckyNumbers[i]];
        }
    }

}
