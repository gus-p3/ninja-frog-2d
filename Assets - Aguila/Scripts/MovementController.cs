using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    public float movementSpeed = 5.0f;  // Velocidad de vuelo (X e Y)

    Vector2 movement;
    Rigidbody2D rb2D;
    ControlesPlayer controles;
    Animator animator;
    SpriteRenderer spriteRenderer;
    const string animationState = "AnimationState";

    // 0 = animación aguila_state (quieto), 1 = animación aguila_left (en movimiento)
    enum CharStates { idle = 0, moving = 1 }

    void Awake()
    {
        controles = new ControlesPlayer();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Sin Collider2D el águila atraviesa todo el escenario
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            Debug.LogWarning("[MovementController] El águila no tenía Collider2D; se agregó un BoxCollider2D.", this);
        }
        col.isTrigger = false;

        rb2D.bodyType = RigidbodyType2D.Dynamic;
        rb2D.gravityScale = 0f; // El águila vuela: la gravedad no la afecta
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // evita atravesar paredes delgadas
        rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void OnEnable()
    {
        if (controles == null) controles = new ControlesPlayer();
        controles.Enable();
    }

    void OnDisable()
    {
        if (controles == null) return;
        controles.Disable();
    }

    void OnDestroy() { controles?.Dispose(); }

    void Update()
    {
        movement = controles.Aguila.Mover.ReadValue<Vector2>();
        UpdateState();
    }

    private void UpdateState()
    {
        // El sprite del águila mira a la derecha: se voltea al ir a la izquierda
        if (spriteRenderer != null && movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        if (animator == null || animator.runtimeAnimatorController == null) return;
        CharStates estado = movement.sqrMagnitude > 0 ? CharStates.moving : CharStates.idle;
        animator.SetInteger(animationState, (int)estado);
    }

    void FixedUpdate()
    {
        // Vuelo libre: X e Y las decide el teclado (diagonales a la misma velocidad).
        // La velocidad se aplica por física, así que los colliders frenan al águila.
        rb2D.linearVelocity = movement.normalized * movementSpeed;
    }
}
