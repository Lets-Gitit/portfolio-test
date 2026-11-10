using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] GameObject spawnObject;
    [SerializeField] int spawnLimit = 10;
    int spawnCount = 0;

    PlayerInputAction playerInputAction;

    private Rigidbody2D rb;
    private Vector2 move;
    public Vector2 Move => move;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();
    }

    private void Update() {
        GetInputValue();
    }

    private void FixedUpdate() {
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
    }

    private void GetInputValue() {
        if (playerInputAction.Player.Move == null) return;
        move = playerInputAction.Player.Move.ReadValue<Vector2>();
    }
}
    
