using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class TitleEvents : MonoBehaviour
{
    [SerializeField]
    private GameLauncher gameLauncher;
    //バージョン設定
    [SerializeField]
    private TextMeshProUGUI version_text;

    //UIWindow
    [SerializeField] private GameObject base_window; //window_back
    [SerializeField] private GameObject setup_window;
    [SerializeField] private GameObject RoomLeft_window;
    [SerializeField] private GameObject Error_window;
    [SerializeField] private GameObject end_window;
    [SerializeField] private TMP_InputField setup_player_name;
    [SerializeField] private TMP_InputField start_session_input;
    [SerializeField] private Button start_session_button;
    [SerializeField] private Button start_button;
    [SerializeField] private Button setting_button;
    [SerializeField] private Button rule_button;
    [SerializeField] private Button end_button;
    [SerializeField] private Button ok_button;
    [SerializeField] private Button save_button;
    [SerializeField] private GameObject[] setting_content;

    //AUdioSource(SE)
    [SerializeField] private AudioSource SE_audio;

    //UI操作系統
    private InputSystemUIInputModule ui_input;
    private InputAction cancel;
    void Start()
    {
        //バージョン番号を表示
        if (version_text != null)
        {
            version_text.text = $"version: {Application.version}";
        }
        //UI操作キーのセット
        ui_input = GetComponent<InputSystemUIInputModule>();
        cancel = ui_input.actionsAsset.FindAction("Cancel");

        //設定の初期値(レジストリ)
        var player_name = PlayerPrefs.GetString("player_name", null);

        //ウィンドウ設定
        setting_content[1].GetComponent<TMP_Dropdown>().value = PlayerPrefs.GetInt("screen", 0);

        if (string.IsNullOrEmpty(player_name))
        {
            //初期設定ウィンドウ処理
            setup_window.SetActive(true);
            start_button.interactable = false;
            setting_button.interactable = false;
            rule_button.interactable = false;
            end_button.interactable = false;
            setup_player_name.text = $"Player{Random.Range(0, 10000)}";
        }

        setting_content[0].GetComponent<TMP_InputField>().text = player_name;

        //スクリーン設定ロード(初期化)
        Screen_Dropdown();
    }

    void Update()
    {
        //戻るボタン
        if (gameLauncher.ingame == false)
        {
            if (cancel.WasPressedThisFrame())
            {
                if (base_window.activeSelf == true)
                {
                    base_window.SetActive(false);
                    SE_audio.Play();
                }

                if (end_window.activeSelf == true)
                {
                    start_button.interactable = true;
                    setting_button.interactable = true;
                    rule_button.interactable = true;
                    end_window.SetActive(false);
                    SE_audio.Play();
                }
            }
        }
        else
        {
            //ゲーム退出ウィンドウ
            if (cancel.WasPressedThisFrame())
            {
                if (!Error_window.activeSelf)
                {
                    if (!RoomLeft_window.activeSelf)
                    {
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                        RoomLeft_window.SetActive(true);
                    }
                    else
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                        RoomLeft_window.SetActive(false);
                    }
                }
            }
        }

        //プレイヤー名のセットボタンの有効化
        if (string.IsNullOrEmpty(setup_player_name.text))
        {
            ok_button.interactable = false;
        }
        else
        {
            ok_button.interactable = true;
        }

        //設定のセーブボタンの有効化
        if (string.IsNullOrEmpty(setting_content[0].GetComponent<TMP_InputField>().text))
        {
            save_button.interactable = false;
        }
        else
        {
            save_button.interactable = true;
        }

        //部屋作成ボタンの有効化
        if (string.IsNullOrEmpty(start_session_input.text))
        {
            start_session_button.interactable = false;
        }
        else
        {
            start_session_button.interactable = true;
        }

    }

    //SetUpWindowの決定ボタンの処理
    public void OnSetupButton()
    {
        setup_window.SetActive(false);
        SE_audio.Play();

        start_button.interactable = true;
        setting_button.interactable = true;
        rule_button.interactable = true;
        end_button.interactable = true;

        //レジストリ書き込み
        PlayerPrefs.SetString("player_name", setup_player_name.text);

        PlayerPrefs.Save();
    }

    //設定のロード
    public void SettingLoad()
    {
        setting_content[0].GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("player_name", null);
    }

    //設定のセーブ
    public void PlayerNameInputSave()
    {
        //レジストリ書き込み
        PlayerPrefs.SetString("player_name", setting_content[0].GetComponent<TMP_InputField>().text);
        PlayerPrefs.Save();
    }

    //スクリーンドロップダウン
    public void Screen_Dropdown()
    {
        //ドロップダウンの値に応じてウィンドウと解像度をセット
        if (setting_content[1].GetComponent<TMP_Dropdown>().value == 0)
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(1024, 576, FullScreenMode.Windowed);
        }
        PlayerPrefs.SetInt("screen", setting_content[1].GetComponent<TMP_Dropdown>().value);
    }

    //ゲーム終了命令
    public void End_App()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
            Application.Quit();//ゲームプレイ終了
#endif
    }

    public void cursorOn()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    //セッションの開始
    public void Play()
    {
        gameLauncher.GameSession(start_session_input.text);
    }
}
