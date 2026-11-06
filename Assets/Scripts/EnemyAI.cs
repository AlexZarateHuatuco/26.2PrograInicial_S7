using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Nexo nexo;

    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float intervaloActualizarDestino = 0.25f;

    [SerializeField] private GameObject prefabBala;
    [SerializeField] private Transform puntaPistola;
    [SerializeField] private float rangoAtaque = 8f;
    [SerializeField] private float tiempoEntreAtaques = 1.5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float velocidadGiro = 8f;

    [SerializeField] private int cantidadBalas = 6;
    [SerializeField] private float tiempoRecarga = 2f;

    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private float distanciaMaximaPersecucionTorre = 15f;

    public NavMeshAgent Agent { get; private set; }

    private int vidaActual;
    private Transform torreAgresora;
    private float contadorAtaque;
    private float contadorDestino;

    private int balasActuales;
    private bool recargando;
    private float contadorRecarga;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        vidaActual = vidaMaxima;
        balasActuales = cantidadBalas;

        if (nexo == null)
        {
            nexo = FindFirstObjectByType<Nexo>();
        }
    }

    private void Start()
    {
        Agent.speed = velocidad;
        Agent.stoppingDistance = 0f;
    }

    private void Update()
    {
        ActualizarRecarga();

        Transform objetivo = ObtenerObjetivoActual();

        if (objetivo == null)
        {
            Agent.isStopped = true;
            return;
        }

        contadorAtaque += Time.deltaTime;

        float distancia = Vector3.Distance(transform.position, objetivo.position);

        if (distancia <= rangoAtaque)
        {
            Atacar(objetivo);
        }
        else
        {
            Mover(objetivo);
        }
    }

    private Transform ObtenerObjetivoActual()
    {
        if (torreAgresora != null)
        {
            float distanciaTorre = Vector3.Distance(transform.position, torreAgresora.position);

            if (distanciaTorre <= distanciaMaximaPersecucionTorre)
            {
                return torreAgresora;
            }

            torreAgresora = null;
        }

        if (nexo != null)
        {
            return nexo.transform;
        }

        return null;
    }

    private void Mover(Transform objetivo)
    {
        Agent.isStopped = false;

        contadorDestino += Time.deltaTime;

        if (contadorDestino >= intervaloActualizarDestino)
        {
            contadorDestino = 0f;
            Agent.SetDestination(objetivo.position);
        }
    }

    private void Atacar(Transform objetivo)
    {
        Agent.isStopped = true;
        Agent.ResetPath();

        MirarHacia(objetivo);

        if (recargando)
        {
            return;
        }

        if (contadorAtaque >= tiempoEntreAtaques)
        {
            Disparar(objetivo);
            contadorAtaque = 0f;
        }
    }

    private void MirarHacia(Transform objetivo)
    {
        Vector3 direccion = objetivo.position - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionDeseada,
            velocidadGiro * Time.deltaTime
        );
    }

    private void Disparar(Transform objetivo)
    {
        if (prefabBala == null || puntaPistola == null)
        {
            return;
        }

        GameObject nuevaBala = Instantiate(prefabBala, puntaPistola.position, puntaPistola.rotation);

        Projectile scriptBala = nuevaBala.GetComponent<Projectile>();
        if (scriptBala != null)
        {
            scriptBala.Configurar(objetivo, damage);
        }

        balasActuales--;

        if (balasActuales <= 0)
        {
            recargando = true;
            contadorRecarga = 0f;
        }
    }

    private void ActualizarRecarga()
    {
        if (!recargando)
        {
            return;
        }

        contadorRecarga += Time.deltaTime;

        if (contadorRecarga >= tiempoRecarga)
        {
            balasActuales = cantidadBalas;
            recargando = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("TowerBullet"))
        {
            return;
        }

        Projectile bala = other.GetComponentInParent<Projectile>();
        if (bala == null)
        {
            return;
        }

        RecibirDano(bala.Dano);
        Destroy(bala.gameObject);

        torreAgresora = BuscarTorreMasCercana();
    }

    private Transform BuscarTorreMasCercana()
    {
        TowerBase[] torres = FindObjectsByType<TowerBase>(FindObjectsSortMode.None);
        Transform masCercana = null;
        float menorDistancia = Mathf.Infinity;

        foreach (TowerBase torre in torres)
        {
            float distancia = Vector3.Distance(transform.position, torre.transform.position);

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                masCercana = torre.transform;
            }
        }

        return masCercana;
    }

    public void RecibirDano(int cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            Destroy(gameObject);
        }
    }
}