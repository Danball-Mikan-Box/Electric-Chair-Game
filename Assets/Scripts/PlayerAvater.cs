using System;
using System.Text.RegularExpressions;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public int selected_chair { get; set; }

    private PlayerInput playerInput;

    private InputAction attack;

    private GameLauncher gameLauncher;

    private Renderer[] renderers;
    public CharacterController LocalcharacterController;
    private NetworkCharacterController characterController;
    private NetworkMecanimAnimator networkAnimator;
    public override void Spawned()
    {
        //ネットワークキャラクターコントローラーの取得
        characterController = GetComponent<NetworkCharacterController>();
        //アニメーションの取得
        networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();

        //カメラ描画の取得
        var view = GetComponent<PlayerAvatarView>();

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
        LocalcharacterController = GetComponent<CharacterController>();

        //ゲームランチャーの取得
        gameLauncher = FindFirstObjectByType<GameLauncher>();

        playerInput = GetComponent<PlayerInput>();

        attack = playerInput.actions["Attack"];

        //初期値のセット
        isValid = true;
        isSetSerectable = false;
        isSitSerectable = false;
        select_chair = 0;
        selected_chair = 0;
    }

    //ネットワーク同期処理
    public override void FixedUpdateNetwork()
    {
        //カメラのアングル調整
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        //方向入力
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        //有効な場合
        if (isValid)
        {
            //キャラ移動
            characterController.Move(cameraRotation * inputDirection);

            Ray ray = new Ray(transform.position + new Vector3(0f, 0.2f, 0f), transform.forward);

            if (isSetSerectable)
            {
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
                return;
            }
            else if (canFinalThunder)
            {
                return;
            }
        }
        // アニメーション
        var animator = networkAnimator.Animator;
        var grounded = characterController.Grounded;
        var vy = characterController.Velocity.y;
        if (isValid)
        {
            animator.SetFloat("Speed", characterController.Velocity.magnitude);
            animator.SetBool("Jump", !grounded && vy > 4f);
            animator.SetBool("Grounded", grounded);
            animator.SetBool("FreeFall", !grounded && vy < -4f);
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
        //キャラクターコントローラの変更
        LocalcharacterController.enabled = isValid;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCSelectChair()
    {
        gameLauncher.SelectChairUI(0);
        selected_chair = select_chair;
    }
    //自分の手番が終わったときにGameMasterに送信
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPCRoleEnd()
    {
        return;
    }
}
