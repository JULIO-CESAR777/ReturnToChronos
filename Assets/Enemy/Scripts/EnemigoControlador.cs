using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemigoControlador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform transformCamara;
    [SerializeField] private Collider hitboxAtaque;

    [Header("Lógica de IA y Ataque")]
    [SerializeField] private float distanciaAtaque = 2f;
    [SerializeField] private float tiempoEntreAtaques = 1.5f;
    [SerializeField] private float duracionGolpeEspalda = 0.4f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private NavMeshAgent agent;

    private float tiempoSiguienteAtaque;
    private float ultimoDirX;
    private float ultimoDirY;
    private bool estaAtacandoPorContacto;

    // VARIABLE PARA HISTÉRESIS (Evita la oscilación rápida)
    private bool estaEnModoDiagonal = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = false;
        }

        if (transformCamara == null && Camera.main != null)
        {
            transformCamara = Camera.main.transform;
        }
    }

    private void Start()
    {
        if (hitboxAtaque != null)
        {
            hitboxAtaque.enabled = false;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (player == null || transformCamara == null) return;

        // Billboard: Mantener Sprite plano a la cámara
        Transform spriteTransform = spriteRenderer != null ? spriteRenderer.transform : transform;
        spriteTransform.rotation = transformCamara.rotation;

        ActualizarDireccionCamara();
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 posEnemigoPlana = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posJugadorPlana = new Vector3(player.position.x, 0f, player.position.z);

        float distanciaAlJugador = Vector3.Distance(posEnemigoPlana, posJugadorPlana);

        if (distanciaAlJugador <= distanciaAtaque && Time.time >= tiempoSiguienteAtaque)
        {
            EjecutarAtaqueInteligente();
            tiempoSiguienteAtaque = Time.time + tiempoEntreAtaques;
        }
    }

    private void ActualizarDireccionCamara()
    {
        Vector3 camAdelante = transformCamara.forward;
        Vector3 camDerecha = transformCamara.right;

        camAdelante.y = 0f;
        camDerecha.y = 0f;
        camAdelante.Normalize();
        camDerecha.Normalize();

        Vector3 dirJugador = player.position - transform.position;
        dirJugador.y = 0f;

        if (dirJugador.sqrMagnitude < 0.001f) return;
        dirJugador.Normalize();

        ultimoDirX = Vector3.Dot(dirJugador, camDerecha);
        ultimoDirY = Vector3.Dot(dirJugador, camAdelante);

        float absX = Mathf.Abs(ultimoDirX);

        // =========================================================
        // CONTROL DE HISTÉRESIS (Evita el cambio continuo de estado)
        // =========================================================
        if (!estaEnModoDiagonal && absX > 0.38f)
        {
            // Solo pasa a 3/4 si hay una intención horizontal clara
            estaEnModoDiagonal = true;
        }
        else if (estaEnModoDiagonal && absX < 0.20f)
        {
            // Solo vuelve a Frente cuando se haya vuelto a centrar
            estaEnModoDiagonal = false;
        }

        // Definimos las coordenadas objetivo para enviar al Blend Tree
        float animX = estaEnModoDiagonal ? 1f : 0f;
        float animY = estaEnModoDiagonal ? 0f : (ultimoDirY > 0f ? 1f : -1f);

        animator.SetFloat("DireccionX", animX);
        animator.SetFloat("DireccionY", animY);

        // =========================================================
        // MANEJO SEGURO DE FLIP X
        // =========================================================
        if (estaEnModoDiagonal)
        {
            if (ultimoDirX < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else if (ultimoDirX > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // En vista frontal o trasera forzamos flipX = false para eliminar saltos en espejo
            spriteRenderer.flipX = false;
        }
    }

    private void EjecutarAtaqueInteligente()
    {
        if (animator == null) return;

        // Si el jugador está atrás en la pantalla, hace daño directo por contacto sin cambiar animación
        if (ultimoDirY > 0.35f && !estaEnModoDiagonal)
        {
            if (!estaAtacandoPorContacto)
            {
                StartCoroutine(AtaqueContactoEspaldaRoutine());
            }
        }
        else
        {
            animator.SetTrigger("Atacar");
        }
    }

    private IEnumerator AtaqueContactoEspaldaRoutine()
    {
        estaAtacandoPorContacto = true;

        if (hitboxAtaque != null)
        {
            hitboxAtaque.enabled = true;
        }

        yield return new WaitForSeconds(duracionGolpeEspalda);

        if (hitboxAtaque != null)
        {
            hitboxAtaque.enabled = false;
        }

        estaAtacandoPorContacto = false;
    }

    public void EventoAbrirHitbox()
    {
        if (hitboxAtaque != null)
            hitboxAtaque.enabled = true;
    }

    public void EventoCerrarHitbox()
    {
        if (hitboxAtaque != null)
            hitboxAtaque.enabled = false;
    }
}