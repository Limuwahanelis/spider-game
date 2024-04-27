using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContext
{
    public LimbStepperManager limbStepperManager;
    public PlayerClimbing climbing;
    public Animator anim;
    public Action<PlayerState> ChangePlayerState;
}
