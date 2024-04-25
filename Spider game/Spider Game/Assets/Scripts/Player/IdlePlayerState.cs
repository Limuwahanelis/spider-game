using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePlayerState : PlayerState
{
    public static Type StateType { get; set; }
    public IdlePlayerState(GetState function) : base(function)
    {
        StateType = typeof(IdlePlayerState);
    }

    public override void Update()
    {

    }
    public override void Move(Vector2 direction)
    {
        if(direction != Vector2.zero)
        {
            ChangeState(WalkingPlayerState.StateType);
            return;
        }
    }
    public override void SetUpState(PlayerContext context)
    {
        base.SetUpState(context);
        //context.anim.SetTrigger("Idle");
    }

    public override void InterruptState()
    {
     
    }
}