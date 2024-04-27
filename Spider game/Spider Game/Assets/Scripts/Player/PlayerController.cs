using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Debug"), SerializeField] bool _printState;

    public PlayerState CurrentPlayerState => _currentPlayerState;

    [SerializeField] PlayerClimbing _playerClimbing;
    [SerializeField] Animator _anim;
    [SerializeField] LimbStepperManager _limbStepperManager;

    private PlayerState _currentPlayerState;
    private PlayerContext _context;
    private Dictionary<Type, PlayerState> playerStates = new Dictionary<Type, PlayerState>();
    void Start()
    {
        List<Type> states = AppDomain.CurrentDomain.GetAssemblies().SelectMany(domainAssembly => domainAssembly.GetTypes())
            .Where(type => typeof(PlayerState).IsAssignableFrom(type) && !type.IsAbstract).ToArray().ToList();


        // CalculateWalkAngle = ChangeState;
        _context = new PlayerContext
        {
            climbing = _playerClimbing,
            limbStepperManager = _limbStepperManager,
            ChangePlayerState = ChangeState,
            anim = _anim,
        };
        PlayerState.GetState getState = GetState;
        foreach (Type state in states)
        {
            playerStates.Add(state, (PlayerState)Activator.CreateInstance(state, getState));
            //playerStates[state].SetUpState(_context);
        }
        _currentPlayerState = GetState(typeof(IdlePlayerState));
        _currentPlayerState.SetUpState(_context);
    }

    public PlayerState GetState(Type state)
    {
        return playerStates[state];
    }
    // Update is called once per frame
    void Update()
    {
        _currentPlayerState.Update();
    }
    private void FixedUpdate()
    {
        _currentPlayerState.FixedUpdate();
    }
    public void ChangeState(PlayerState newState)
    {
        if (_printState) Debug.Log(newState.GetType());
        _currentPlayerState.InterruptState();
        _currentPlayerState = newState;
    }
}
