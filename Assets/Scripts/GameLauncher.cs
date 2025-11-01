using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] GameObject Title;
    [SerializeField] GameObject Window_back;
    [SerializeField] GameObject loading_window;
    [SerializeField] GameObject Error_window;
    [SerializeField] TMP_Text Error_Text;
    [SerializeField] TMP_Text Room_name;
    [SerializeField] GameObject Battle_Text;
    [SerializeField] GameObject Loading_Text;
    [SerializeField] GameObject TryInRomm_Text;
    [SerializeField] GameObject Search_Text;
    [SerializeField] TMP_Text battleplay;
    [SerializeField] AudioSource TitleBGM;
    [SerializeField] AudioSource GameBGM;
    [SerializeField] AudioSource SE;
    [SerializeField] GameObject Lightning_Movie;
    [SerializeField]
    private NetworkPrefabRef GameMasterPrefab;
    public NetworkObject master;
    [SerializeField]
    private NetworkRunner networkRunnerPrefab;
    private NetworkRunner networkRunner;
    [SerializeField]
    private NetworkPrefabRef playerAvatarPrefab;
    public StartGameResult public_result;
    public NetworkLinkedList<string> player_names;

    //HUDのシリアライズ
    [Header("HUDのオブジェクト")]
    [SerializeField] GameObject HUD;
    [SerializeField] GameObject Player_Screen;
    [SerializeField] GameObject Wait_Screen;
    [SerializeField] TMP_Text Play_Tip;

    //ゲーム中か否か
    public bool ingame = false;

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // セッションへ参加したプレイヤーが自分自身かどうかを判定する
        if (player == runner.LocalPlayer)
        {
            var spawnPosition = new Vector3(0, 1f, 0);
            // 自分自身のアバターをスポーンする
            var playerobj = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, onBeforeSpawned: (_, networkObject) =>
            {
                // プレイヤー名を設定
                networkObject.GetComponent<PlayerAvater>().NickName = PlayerPrefs.GetString("player_name");
                networkObject.GetComponent<PlayerAvater>().owner = player;
            });
            runner.SetPlayerObject(player, playerobj);
            ingame = true;

        }

        //プレイヤーカウント
        int playerCount = runner.ActivePlayers.Count();

        //二人揃ったらゲーム開始
        if (playerCount == 2)
        {
            Loading_Text.SetActive(false);
            Search_Text.SetActive(false);
            Battle_Text.SetActive(true);

            TitleBGM.Pause();
            GameBGM.Play();

            //ゲーム動作用オブジェクトの生成
            if (runner.IsSharedModeMasterClient)
            {
                master = runner.Spawn(GameMasterPrefab);
            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Lightning_Movie.SetActive(true);
            StartCoroutine(HideLoadWindow(3f));
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer != player)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Error_window.SetActive(true);
            Error_Text.text = "相手が退出したためルームを終了します。";
        }
    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //異常停止したらログを出力
        if (shutdownReason != ShutdownReason.Ok)
        {
            ErrorOutput(shutdownReason.ToString());
        }
    }

    //セッションに参加したらローディング画面表示を変更
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        Search_Text.SetActive(true);
        Room_name.text = $"ルーム名:{runner.SessionInfo.Name}";
        TryInRomm_Text.SetActive(false);
    }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    //セッションへのアクセスに失敗したらログを出力
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        ErrorOutput(reason.ToString());
    }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    //セッションアクセス
    public async void GameSession(string session_name)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        // GameLauncherを、NetworkRunnerのコールバック対象に追加する
        networkRunner.AddCallbacks(this);

        //ローディング画面表示のセット
        loading_window.SetActive(true);
        Title.SetActive(false);
        Window_back.SetActive(false);

        TryInRomm_Text.SetActive(true);

        //セッションの開始
        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = session_name,
            PlayerCount = 2,
            IsOpen = true,
            IsVisible = false,
        });

        //異常検出でログを出力
        if (!result.Ok)
        {
            ErrorOutput(result.ShutdownReason.ToString());
        }
    }

    //異常検出ログ
    private void ErrorOutput(string reason)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ingame = false;
        Error_window.SetActive(true);
        Error_Text.text = "エラーが発生しました。\n 理由:" + reason;
    }

    //ログに了承し、セッションを終了(初期化)
    public void OnDebugOKButton()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        networkRunner.Shutdown();
        ingame = false;
        battleplay.text = "";
        SE.Play();
        Error_window.SetActive(false);
        loading_window.SetActive(false);
        TryInRomm_Text.SetActive(false);
        Search_Text.SetActive(false);
        Title.SetActive(true);
        Lightning_Movie.SetActive(false);
        HUD.SetActive(false);

        GameBGM.Pause();
        TitleBGM.Play();
    }

    private IEnumerator HideLoadWindow(float delay)
    {
        yield return new WaitForSeconds(delay);

        loading_window.SetActive(false);
    }
    //電気仕掛け中のLocalPlayerHUDセット
    public void InElectSetHUD()
    {
        var local_player = networkRunner.GetPlayerObject(networkRunner.LocalPlayer);

        //Nullなら返す
        if (local_player.IsUnityNull()) return;
        var local_avater = local_player.GetComponent<PlayerAvater>();

        if (local_avater.IsUnityNull()) return;

        if (local_avater.isValid)
        {
            HUD.SetActive(true);
            Player_Screen.SetActive(true);
            Wait_Screen.SetActive(false);
            Play_Tip.text = "電気を仕掛けよう";
        }
        else
        {
            HUD.SetActive(true);
            Player_Screen.SetActive(false);
            Wait_Screen.SetActive(true);
        }
    }

    void Update()
    {
        //ゲーム中か判定し、HUDを有効
        if (ingame)
        {
            InElectSetHUD();
        }
    }


}