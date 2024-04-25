using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContext
{
    public PlayerClimbing climbing;
    public Animator anim;
    public Action<PlayerState> ChangePlayerState;
}
