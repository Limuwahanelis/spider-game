using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePlayerState : PlayerState
{
    public IdlePlayerState(GetState function) : base(function)
    {
    }

    public override void Update()
    {

    }

    public override void SetUpState(PlayerContext context)
    {
        base.SetUpState(context);
    }

    public override void InterruptState()
    {
     
    }
}