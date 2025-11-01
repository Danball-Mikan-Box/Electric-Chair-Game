using System;
using System.Text.RegularExpressions;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAvater : NetworkBehaviour
{
    // プレイヤー名のネットワークプロパティを定義する
    [Networked, SerializeField]
    public NetworkString<_16> NickName { get; set; }
    [Networked]
    public PlayerRef owner { get; set; }
    [Networked, SerializeField]
    public NetworkBool isValid { get; set; }
    [Networked, SerializeField]
    public NetworkBool isSetSerectable { get; set; }
    [Networked, SerializeField]
    public NetworkBool isSitSerectable { get; set; }
    [Networked, SerializeField]
    public NetworkBool canFinalThunder{ get; set; }
    [Networked, SerializeField]
    public int select_chair{ get; set; }

    private Renderer[] renderers;
    private CharacterController LocalcharacterController;
    private NetworkCharacterController characterController;
    private NetworkMecanimAnimator networkAnimator;
    public override void Spawned()
    {
        characterController = GetComponent<NetworkCharacterController>();
        networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
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

        //初期値のセット
        isValid = true;

        isSetSerectable = false;
        isSitSerectable = false;
        select_chair = 0;
    }

    public override void FixedUpdateNetwork()
    {
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (isValid)
        {
            characterController.Move(cameraRotation * inputDirection);
            Ray ray = new Ray(transform.position + new Vector3(0f,0.2f,0f), transform.forward);

            if (isSetSerectable)
            {
                if(Physics.Raycast(ray, out var hit, 0.5f))
                {
                    //セレクト対象
                    if (!hit.IsUnityNull())
                    {
                        try
                        {
                            select_chair = int.Parse(Regex.Replace(hit.transform.name, @"[^0-9]", ""));
                        }
                        catch (FormatException)
                        {
                            select_chair = 0;
                        }
                        Debug.Log(select_chair);
                    }
                    else
                    {
                        select_chair = 0;
                    }
                }
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

        RpcOnIsValidChanged();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcOnIsValidChanged()
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
}
