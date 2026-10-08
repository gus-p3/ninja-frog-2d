using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    public float movementSpeed = 4.0f;  // Velocidad horizontal
    public float jumpForce = 12.0f;     // Impulso hacia arriba

    [Header("Detección de suelo")]
    public Transform groundCheck;       // Arrastra aquí el hijo GroundCheck
    public float groundRadius = 0.15f;  // Tamaño del círculo detector
    public LayerMask groundLayer;       // Marca solo la capa Ground
    public bool mostrarDiagnostico = true;

    Vector2 movement = new Vector2();
    Rigidbody2D rb2D;
    ControlesPlayer controles;
    Animator animator;
    string animationState = "AnimationState";

    // Collider del jugador, usado como respaldo en IsGrounded()
    Collider2D playerCollider;

    enum CharStates { walkEast = 1, walkSouth = 2, walkWest = 3, walkNorth = 4, idleSouth = 5 }

    void Awake()
    {
        controles = new ControlesPlayer();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // ── Auto-asignar groundCheck si no se arrastró en el Inspector ──
        if (groundCheck == null)
        {
            Transform hijo = transform.Find("GroundCheck");
            if (hijo != null)
            {
                groundCheck = hijo;
                Debug.LogWarning("[MovementController] groundCheck estaba vacío; se asignó automáticamente el hijo 'GroundCheck'.", this);
            }
            else
            {
                // Crear el hijo por código en la parte inferior central del Collider2D
                GameObject go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);

                if (playerCollider != null)
                {
                    // Posición local relativa: centro X del collider, parte baja Y
                    Vector2 localBottom = new Vector2(
                        playerCollider.offset.x,
                        playerCollider.offset.y - (playerCollider.bounds.extents.y / transform.lossyScale.y)
                    );
                    go.transform.localPosition = new Vector3(localBottom.x, localBottom.y, 0f);
                }
                else
                {
                    go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                }
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                groundCheck = go.transform;
                Debug.LogWarning("[MovementController] No existía el hijo 'GroundCheck'; se creó automáticamente en la base del collider.", this);
            }
        }

        // ── Auto-asignar groundLayer si está en Nothing (0) ──
        if (groundLayer.value == 0)
        {
            int capaGround = LayerMask.NameToLayer("Ground");
            if (capaGround == -1)
            {
                Debug.LogError("[MovementController] ¡La capa 'Ground' no existe en el proyecto! " +
                    "Créala en Edit ▸ Project Settings ▸ Tags and Layers y asígnala al Tilemap.", this);
            }
            else
            {
                groundLayer = LayerMask.GetMask("Ground");
                Debug.LogWarning("[MovementController] groundLayer estaba en Nothing; se asignó automáticamente la capa 'Ground'.", this);
            }
        }

        // ── Advertencia: el jugador NO debería estar en la capa Ground ──
        if (gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.LogError("[MovementController] ¡El PlayerObject está en la capa 'Ground'! " +
                "El detector de suelo se detectará a sí mismo. Cambia la capa del jugador a otra distinta (p. ej. 'Blocking').", this);
        }

        // ── Advertencia: gravedad desactivada ──
        if (rb2D.gravityScale <= 0f)
        {
            Debug.LogWarning("[MovementController] El Rigidbody2D tiene Gravity Scale ≤ 0. " +
                "El personaje no caerá ni se comportará correctamente en plataformas.", this);
        }
    }

    void OnEnable()
    {
        if (controles == null) controles = new ControlesPlayer();
        controles.Enable();
        controles.Player.Jump.performed += OnJump; // Se llama al pulsar la tecla de salto
    }

    void OnDisable()
    {
        if (controles == null) return;
        controles.Player.Jump.performed -= OnJump;
        controles.Disable();
    }

    void OnDestroy() { controles?.Dispose(); }

    void Update()
    {
        movement = controles.Player.Move.ReadValue<Vector2>();
        movement.y = 0; // En plataformas solo importa izquierda/derecha
        UpdateState();
    }

    // Mismo Animator de la Parte 2: no hay estados nuevos
    private void UpdateState()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        if (movement.x > 0)      animator.SetInteger(animationState, (int)CharStates.walkEast);
        else if (movement.x < 0) animator.SetInteger(animationState, (int)CharStates.walkWest);
        else                     animator.SetInteger(animationState, (int)CharStates.idleSouth);
    }

    void FixedUpdate()
    {
        // X la decide el teclado; Y la decide la gravedad (se conserva su valor)
        rb2D.linearVelocity = new Vector2(movement.x * movementSpeed, rb2D.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        bool enSuelo = IsGrounded();
        Debug.Log($"[MovementController] Jump pulsado — IsGrounded(): {enSuelo}");

        // Si no toca el suelo, no pasa nada: por eso no se puede volar
        if (enSuelo)
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return false;

        // ── Método principal: OverlapCircle ──
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Ignorar colliders del propio jugador (no debería pasar si las capas están bien, pero por seguridad)
        if (hit != null && !hit.isTrigger && hit.transform.root != transform.root)
            return true;
        // Si el hit era nulo o era el propio jugador, continuamos al respaldo

        // ── Respaldo: OverlapBox debajo del collider del jugador ──
        if (playerCollider != null)
        {
            Bounds b = playerCollider.bounds;
            // Caja justo debajo del collider: ancho al 90 %, alto de 0.05
            Vector2 boxCenter = new Vector2(b.center.x, b.min.y - 0.025f);
            Vector2 boxSize = new Vector2(b.size.x * 0.9f, 0.05f);

            Collider2D hitBox = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
            if (hitBox != null && !hitBox.isTrigger && hitBox.transform.root != transform.root)
                return true;
        }

        return false;
    }

    void OnGUI()
    {
        if (mostrarDiagnostico)
            GUI.Label(new Rect(10, 10, 300, 25), "En el suelo: " + IsGrounded());
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        // Dibujar también la caja de respaldo
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Bounds b = col.bounds;
            Vector2 boxCenter = new Vector2(b.center.x, b.min.y - 0.025f);
            Vector2 boxSize = new Vector2(b.size.x * 0.9f, 0.05f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}