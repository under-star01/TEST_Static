using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager_B : MonoBehaviour
{
    // ���� ���� ���� ������

    private float survivalTime = 0f; // �÷��̾� ���� �ð� (�� ����)
    private bool isGameOver = false; // ���ӿ��� �������� Ȯ��
    private GameOverUI gameOverUI; // ���ӿ��� UI ����

    // ���� ���� �� �ʱ�ȭ 

    void Start()
    {
        // ������ GameOverUI ������Ʈ ã��
        gameOverUI = FindAnyObjectByType<GameOverUI>();
    }

    // �� �����Ӹ��� ���� 

    void Update()
    {
        // ���ӿ����� �ƴ� ���� �ð� ����
        if (!isGameOver)
        {
            // Time.deltaTime: ���� �����Ӱ��� �ð� ���� (��)
            // �� �����Ӹ��� deltaTime�� ���ؼ� �� ���� �ð� ���
            survivalTime += Time.deltaTime;
        }

        //// �׽�Ʈ��: GŰ�� ������ ���ӿ���
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    TriggerGameOver(); // ���ӿ��� �Լ� ȣ��
        //}
    }

    // ���ӿ��� ó�� �Լ� 

    private void TriggerGameOver()
    {
        // �̹� ���ӿ��� ���¸� �ߺ� ���� ����
        if (isGameOver)
        {
            return; // �Լ� ����
        }

        // ���ӿ��� ���·� ����
        isGameOver = true;

        // GameOverUI�� ���� �ð� �����ϸ� ���ӿ��� ȭ�� ǥ��
        gameOverUI.ShowGameOver(survivalTime);
    }

    // �ܺο��� ȣ�� ������ ���ӿ��� �Լ� 
    // �ٸ� ��ũ��Ʈ���� GameManager.GameOver()�� ȣ�� ����

    public void GameOver()
    {
        TriggerGameOver(); // ���� ���ӿ��� �Լ� ȣ��
    }
}

