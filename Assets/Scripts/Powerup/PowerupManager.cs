using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using static GameConst;

public class PowerupManager : BaseSingleton<PowerupManager>
{
    [HideInInspector] public bool DoubleScore = false;
    [HideInInspector] public bool Stake = false, HintSolved;
    public int ScoreStakeIncrease, ScoreStakeDecrease;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void HandlePurchasePowerup(Powerup powerup)
    {
        switch(powerup.PowerupName)
        {
            case DOUBLE_SCORE:
                DoubleScore = true;
                break;
            case STAKE:
                Stake = true;
                break;
            case BOMB:
                break;
            case SHIELD:
                break;
        }
        string content = "Purchase success!";
        ShowNotification.Show(content);
    }

    public void ResetPowerups()
    {
        DoubleScore = false;
        Stake = false;
    }
}
