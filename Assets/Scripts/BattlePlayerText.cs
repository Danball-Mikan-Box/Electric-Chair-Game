using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BattlePlayerText : MonoBehaviour
{
    [SerializeField] private GameLauncher gameLauncher;
    [SerializeField] private GameMaster gamemaster;
    void Update()
    {
        if (gamemaster.IsUnityNull())
        {
            var gm = FindFirstObjectByType<GameMaster>();
            if(!gm.IsUnityNull())
            {
                gamemaster = gm;
            }
        }
        else
        {
            GetComponent<TMP_Text>().text = (string)gamemaster.Battle_Player_Text;
        }
    }
}
