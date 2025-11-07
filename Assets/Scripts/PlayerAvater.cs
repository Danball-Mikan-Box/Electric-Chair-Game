using System;
using System.Text.RegularExpressions;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class PlayerAvater : NetworkBehaviour
{
    // プレイヤー名のネットワークプロパティを定義する
    [Networked, SerializeField]
    public NetworkString<_16> NickName { get; set; }
    //プレイヤーオブジェクトが有効かどうか
    [Networked, SerializeField]
    public NetworkBool isValid { get; set; }
    //椅子に電気を仕掛けられるか。
    [Networked, SerializeField]
    public NetworkBool isSetSerectable { get; set; }
    //椅子に座れるか
    [Networked, SerializeField]
    public NetworkBool isSitSerectable { get; set; }
    //ファイナルサンダーボタンを押せるか
    [Networked, SerializeField]
    public NetworkBool canFinalThunder{ get; set; }
    //選んだ椅子の番号(無選択=0)
    [Networked, SerializeField]
    public int select_chair { get; set; }

    [Networked, SerializeField]
    public int sitting_chair{ get; set; }

    //固定した選択椅子番号
    [Networked, SerializeField]
    public int selected_chair { get; set; }

    [Networked, SerializeField]
    public NetworkBool isSitting { get; set; }

    [Networked,SerializeField]
    public bool defenceIsSitting{ get; set; }

    public string player_tip;

    public VideoPlayer cutInPlayer;

    public Transform[] sitpoints;

    public Transform[] standpoints;

    //プレイヤー
    private PlayerInput playerInput;

    private InputAction attack;

    private InputAction sprint;
    //ゲームランチャー
    private GameLauncher gameLauncher;

    //ゲームマスター
    private GameMaster master;
    //レンダラーのArray
    private Renderer[] renderers;
    //キャラクターコントローラーの有効
    public bool LocalcharacterController;

    //キャラクターコントローラー
    private NetworkCharacterController characterController;
    //ネットワークアニメーター
    private NetworkMecanimAnimator networkAnimator;
    public override void Spawned()
    {
        //ネットワークキャラクターコントローラーの取得
        characterController = GetComponent<NetworkCharacterController>();
        //アニメーションの取得
        networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();

        //カメラ描画の取得
        var view = GetComponent<PlayerAvatarView>();

        cutInPlayer = GameObject.FindGameObjectWithTag("CutInPlayer").GetComponent<VideoPlayer>();

        //部屋のコライダー
        var confiner = GameObject.Find("Room").GetComponent<Collider>();
        // プレイヤー名をテキストに反映する
        view.SetNickName(NickName.Value);
        // 自身がアバターの権限を持っているなら、カメラの追従対象にする
        if (HasStateAuthority)
        {
            view.MakeCameraTarget();
        }
        //CinemachineConfiner3Dをセット。
        view.ConfinerSet(confiner);

        //レンダーの取得
        renderers = GetComponentsInChildren<Renderer>(true);
        //キャラクターコントローラーの取得(使わないなら削除)
        LocalcharacterController = true;

        //ゲームランチャーの取得
        gameLauncher = FindFirstObjectByType<GameLauncher>();

        master = FindFirstObjectByType<GameMaster>();

        playerInput = GetComponent<PlayerInput>();

        attack = playerInput.actions["Attack"];
        sprint = playerInput.actions["Sprint"];

        //初期値のセット
        isValid = true;
        isSetSerectable = false;
        isSitSerectable = false;
        isSitting = false;
        select_chair = 0;
        selected_chair = 0;
        player_tip = "";
    }

    //ネットワーク同期処理
    public override void FixedUpdateNetwork()
    {
        // アニメーション
        var animator = networkAnimator.Animator;

        if (master.IsUnityNull())
        {
            master = FindFirstObjectByType<GameMaster>();
        }
        //カメラのアングル調整
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        //方向入力
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        //有効な場合
        if (isValid)
        {
            if (LocalcharacterController)
            {
                //キャラ移動
                characterController.Move(cameraRotation * inputDirection);
            }

            Ray ray = new Ray(transform.position + new Vector3(0f, 0.2f, 0f), transform.forward);

            if (isSetSerectable)
            {
                player_tip = "電気を仕掛けよう";
                //前方オブジェクトの検出
                if (Physics.Raycast(ray, out var hit, 0.5f))
                {

                    //セレクト対象があるなら
                    if (!hit.IsUnityNull())
                    {

                        //椅子の数字を代入
                        try
                        {
                            select_chair = int.Parse(Regex.Replace(hit.transform.name, @"[^0-9]", ""));
                        }
                        catch (FormatException)
                        {
                            select_chair = 0;
                        }

                        gameLauncher.SelectChairUI(select_chair);

                        if(attack.IsPressed() && select_chair != 0)
                        {
                            gameLauncher.SelectChairWindow();
                        }
                    }
                }
                else
                {
                    select_chair = 0;
                    gameLauncher.SelectChairUI(select_chair);
                }
            }
            else if (isSitSerectable)
            {
                player_tip = "椅子を選ぼう";
                if (isSitting && selected_chair != 0)
                {
                    characterController.Teleport(sitpoints[selected_chair - 1].position);
                    characterController.transform.rotation = sitpoints[selected_chair - 1].rotation;
                }

                if (master.isFinalThunderSelect)
                {

                }
                else
                {
                    
                }

                //前方オブジェクトの検出
                if (Physics.Raycast(ray, out var hit, 0.5f))
                {

                    //セレクト対象があるなら
                    if (!hit.IsUnityNull())
                    {

                        //椅子の数字を代入
                        try
                        {
                            select_chair = int.Parse(Regex.Replace(hit.transform.name, @"[^0-9]", ""));
                        }
                        catch (FormatException)
                        {
                            select_chair = 0;
                        }

                        gameLauncher.SelectChairUI(select_chair);

                        if (attack.IsPressed() && select_chair != 0 && selected_chair == 0)
                        {
                            selected_chair = select_chair;
                            isSitting = true;
                            animator.SetBool("Sitting", true);
                        }
                    }

                }
                else
                {
                    select_chair = 0;
                    gameLauncher.SelectChairUI(select_chair);
                }

                if (sprint.IsPressed() && selected_chair != 0 && master.isFinalThunderSelect == false)
                {
                    characterController.Teleport(standpoints[selected_chair - 1].position);
                    characterController.Velocity = Vector3.zero;
                    isSitting = false;
                    animator.SetBool("Sitting", false);
                    selected_chair = 0;
                }
            }
            else if (canFinalThunder)
            {
                player_tip = "相手を誘導しよう";
                gameLauncher.Select_Chair_UI.SetActive(false);
                //前方オブジェクトの検出
                if (Physics.Raycast(ray, out var hit, 0.5f))
                {

                    //セレクト対象があるなら
                    if (!hit.IsUnityNull())
                    {
                        if(hit.transform.name == "ボタン" && defenceIsSitting == true)
                        {
                            gameLauncher.SelectChairUI(13);

                            if (attack.IsPressed())
                            {
                                master.RPCisFinalThunderSelect(true);
                            }
                        }
                    }
                }
                else
                {
                    gameLauncher.SelectChairUI(0);
                }
            }
            else
            {
                select_chair = 0;
            }
        }
        if (isValid)
        {
            animator.SetFloat("Speed", characterController.Velocity.magnitude);
            animator.SetFloat("MotionSpeed", 1f);
        }

        //有効処理関数実行
        RPCOnIsValidChanged();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPCOnIsValidChanged()
    {
        //有効かどうか検証
        bool newValue = isValid;

        //レンダー無効
        if (renderers != null)
        {
            foreach (var rend in renderers)
                if (rend != null)
                    rend.enabled = newValue;
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCSelectChair()
    {
        selected_chair = select_chair;
        select_chair = 0;
        gameLauncher.SelectChairUI(select_chair);
        characterController.Teleport(new Vector3(2, 0.5f, 2));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCDefenceSitting(bool sitting)
    {
        defenceIsSitting = sitting;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCFinalThunderUI()
    {
        cutInPlayer.Play();

        gameLauncher.PlayScreenValid(false);
    }
    //自分の手番が終わったときにGameMasterに送信
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPCRoleEnd()
    {
        return;
    }
}
