using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;
    private Vector2 move;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _playerController.CurrentPlayerState.Move(move);
    }
    // used by PlayerInput
    void OnMove(InputValue inputValue)
    {
        move = inputValue.Get<Vector2>();
    }
}
