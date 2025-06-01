using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{

    public void StartGame() // 게임 시작 버튼 누를 경우
    {
        Debug.Log("게임을 시작합니다");
        SceneManager.LoadScene("Stage1");
    }
    public void LoadGame() // 이어 하기를 누를 경우
    {
        Debug.Log("게임을 이어합니다");
    }

    public void ExitGame()
    {
        Debug.Log("게임을 종료합니다");
    }
}
