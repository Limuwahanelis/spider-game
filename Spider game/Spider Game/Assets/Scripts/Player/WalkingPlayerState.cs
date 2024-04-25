using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingPlayerState : PlayerState
{
    public static Type StateType { get; set; }
    public WalkingPlayerState(GetState function) : base(function)
    {
        StateType=typeof(WalkingPlayerState);
    }

    public override void Update()
    {

    }
    public override void FixedUpdate()
    {
        _context.climbing.SetUpLimbs();
    }
    public override void Move(Vector2 direction)
    {
        //Debug.Log($"In state {direction}");
        _context.climbing.Climb(direction);

    }
    public override void SetUpState(PlayerContext context)
    {
        base.SetUpState(context);
        //_context.anim.SetTrigger("Walk");
        _context.climbing.InitHelper();
    }

    public override void InterruptState()
    {
     
    }
}