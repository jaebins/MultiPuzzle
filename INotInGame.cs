using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

interface INotInGame
{
    void ChangeScene(int sceneCnt);
    void CheckEndChangeSceneEvent(int sceneCnt); // Update �ȿ��� �۵�
    void SetSettingPanel();
}
