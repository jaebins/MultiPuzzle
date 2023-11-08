using System;
using System.IO;
using UnityEngine;

class Setting
{
    // ��� ���� ����
    public static int BLOCK_SOURCE_LENGTH = 4;
    public static Vector2 BLOCK_STARTPOS = new Vector2(-0.375f, 0.35f);
    public static Vector2 BLOCK_MARGIN = new Vector2(0.15f, 0.15f);
    public static Vector2 BLOCK_SIZE = new Vector2(0.1f, 0.1f);
    public static Vector2 BLOCK_LENGTH = new Vector2(6, 6);

    /// ���� ���� ����
    public static Vector2 PANEL_POS_1 = new Vector2(-500, -50);
    public static Vector2 PANEL_POS_2 = new Vector2(500, -50);

    // ���� ���� ����
    public static int MAXPLAYERS = 2;

    // ��Ÿ
    public static string SAVE_GAMEENV_PATH = Path.Combine(Application.persistentDataPath, "GameEnv.json");
}
