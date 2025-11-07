using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using NUnit.Framework;

//ゲームの動作
public class GameMaster : NetworkBehaviour
{
    //ネットワークランナー変数
    private NetworkRunner runner;

    //ゲームランチャーをセット
    private GameLauncher gameLauncher;

    //プレイヤーリスト
    private PlayerRef[] players;

    //プレイヤーネームのリスト
    private List<string> player_names;

    //ゲーム中か否か
    [Networked]
    public NetworkBool IsGaming{ get; set; }

    [Networked]
    public NetworkString<_128> Battle_Player_Text { get; set; }

    //アタックプレイヤーの添字
    [Networked]
    public int AttackPlayer_num { get; set; }

    //現在のラウンド
    [Networked, SerializeField]
    public int round { get; set; }

    //現在のラウンド内の手番
    [Networked, SerializeField]
    public int turn { get; set; }

    //プレイヤーポイントリスト
    [Networked]
    public NetworkLinkedList<int> player1_points { get; }

    //プレイヤーポイントリスト2
    [Networked]
    public NetworkLinkedList<int> player2_points { get; }

    //電気椅子の番号
    [Networked, SerializeField]
    public int elected_chair { get; set; }

    //ファイナルサンダーGUIをオンにする信号
    [Networked, OnChangedRender(nameof(RPCChangeFinalThunderSelect))]
    public NetworkBool isFinalThunderSelect{ get; set; }

    public override void Spawned()
    {
        //ネットワークランナーセット
        runner = FindFirstObjectByType<NetworkRunner>(); ;
        gameLauncher = FindFirstObjectByType<GameLauncher>();

        //プレイヤーRefのArray
        players = runner.ActivePlayers.ToArray();

        //先攻プレイヤーの決定
        if (runner.IsSharedModeMasterClient)
        {
            AttackPlayer_num = Random.Range(0, 2);
        }

        IsGaming = true;

        round = 0;
        turn = 0;

        elected_chair = 0;

        player_names = new List<string>();

    }

    public override void FixedUpdateNetwork()
    {
        //ゲームプレイヤーを取得し、名前を表示
        if (player_names.Count != 2)
        {
            player_names = new List<string>();
            foreach (var p in runner.ActivePlayers)
            {
                runner.TryGetPlayerObject(p, out var player);
                if (player.IsUnityNull()) return;
                player_names.Add(player.GetComponent<PlayerAvater>().NickName.ToString());
            }
        }
        else if (player_names.Count == 2)    //player_nameが二人取得できたら名前表示をセット
        {
            //オブジェクトが読み取れない場合は戻る
            if(runner.GetPlayerObject(players[AttackPlayer_num]).IsUnityNull() || runner.GetPlayerObject(players[AttackPlayer_num]).GetComponent<PlayerAvater>().IsUnityNull()) return;
            //temp
            var attackplayer_avater = runner.GetPlayerObject(players[AttackPlayer_num]).GetComponent<PlayerAvater>();
            Battle_Player_Text = $"{player_names[0]}\nvs\n{player_names[1]}\n先攻:{attackplayer_avater.NickName}";

            //Round開始(電気仕掛け)
            //アタックプレイヤーをセット
            var attack_player = players[AttackPlayer_num];
            int Defence_player_num = 0;

            //ディフェンスプレイヤーナンバー
            switch (AttackPlayer_num)
            {
                case 0:
                    Defence_player_num = 1;
                    break;
                case 1:
                    Defence_player_num = 0;
                    break;
            }

            var defence_player = players[Defence_player_num];

            if (turn == 0)
            {
                //アタックプレイヤーの有効化
                RPCPlayerValid(attack_player, true);

                //ディフェンスプレイヤーの無効化
                RPCPlayerValid(defence_player, false);

                //電気椅子が仕掛けられているか仕掛けられていないか。
                if (elected_chair == 0)
                {
                    RPCPlayerSetSerectable(attack_player, true);
                    elected_chair = attackplayer_avater.selected_chair;
                    attackplayer_avater.selected_chair = 0;
                }
                else
                {
                    if(turn == 0)
                    {
                        round += 1;
                    }
                    turn = 1;
                    RPCPlayerSetSerectable(attack_player, false);
                    RPCPlayerValid(players[1], true);
                    RPCPlayerValid(players[0], true);

                    //ディフェンスが座れるように
                    RPCPlayerSitSerectable(defence_player, true);

                    //アタックがファイナルサンダーをできるように
                    RPCPlayerCanFinalThunder(attack_player, true);
                }
            }
            else
            {
                var defenceobj = runner.GetPlayerObject(defence_player);
                var defence_avater = defenceobj.GetComponent<PlayerAvater>();

                attackplayer_avater.RPCDefenceSitting(defence_avater.isSitting);
            }
        }
    }

    //プレイヤーアバターの有効化処理
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCPlayerValid(PlayerRef player, NetworkBool valid)
    {
        var playerobj = runner.GetPlayerObject(player);
        var playeravater = playerobj.GetComponent<PlayerAvater>();
        playeravater.isValid = valid;
    }

    //プレイヤーの電気仕掛けの有効化処理
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCPlayerSetSerectable(PlayerRef player, NetworkBool serectable)
    {
        var playerobj = runner.GetPlayerObject(player);
        var playeravater = playerobj.GetComponent<PlayerAvater>();
        playeravater.isSetSerectable = serectable;
    }

    //プレイヤーの電気椅子座り有効化処理
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCPlayerSitSerectable(PlayerRef player, NetworkBool serectable)
    {
        var playerobj = runner.GetPlayerObject(player);
        var playeravater = playerobj.GetComponent<PlayerAvater>();
        playeravater.isSitSerectable = serectable;
    }

    //プレイヤーのファイナルサンダー有効化処理
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCPlayerCanFinalThunder(PlayerRef player, NetworkBool canfinalthunder)
    {
        var playerobj = runner.GetPlayerObject(player);
        var playeravater = playerobj.GetComponent<PlayerAvater>();
        playeravater.canFinalThunder = canfinalthunder;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCisFinalThunderSelect(NetworkBool select)
    {
        isFinalThunderSelect = select;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCChangeFinalThunderSelect()
    {
        if (isFinalThunderSelect)
        {
            var player0 = runner.GetPlayerObject(players[0]);
            var player1 = runner.GetPlayerObject(players[1]);

            var avater0 = player0.GetComponent<PlayerAvater>();
            var avater1 = player1.GetComponent<PlayerAvater>();

            avater0.RPCFinalThunderUI();
            avater1.RPCFinalThunderUI();
        }
    }

    //プレイヤーに得点をセット
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCPlayerPointsSet(PlayerRef player, int round, int point)
    {

    }


}
