using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : NetworkBehaviour
{
    [SerializeField] float speed = 5f;

    Rigidbody2D rb;
    Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 0f)
        {
            CmdMove(input.normalized);
        }
        else
        {
            CmdStop();
        }
    }

    [Command]
    void CmdMove(Vector2 direction)
    {
        moveDirection = direction;
    }

    [Command]
    void CmdStop()
    {
        moveDirection = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (!isServer) return;
        rb.velocity = moveDirection * speed;
    }
}
