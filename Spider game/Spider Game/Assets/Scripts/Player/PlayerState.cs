using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerContext _context;
    public delegate PlayerState GetState(Type state);
    protected static GetState _getType;
    
    public PlayerState(GetState function)
    {
        _getType = function;
    }
    public abstract void InterruptState();
    public abstract void Update();
    public virtual void FixedUpdate() { }
    public virtual void SetUpState(PlayerContext context)
    {
        _context = context;
    }
    public virtual void Move(Vector2 direction) { }
    public virtual void Jump() { }
    public virtual void Attack() { }
    public virtual void ChangeCurrentState()
    {
        _context.ChangePlayerState(this);
    }
    public void ChangeState(Type newStateType)
    {
        PlayerState state = _getType(newStateType);
        _context.ChangePlayerState(state);
        state.SetUpState(_context);
    }
}

