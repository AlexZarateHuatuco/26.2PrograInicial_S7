using UnityEngine;

public class TowerBase : MonoBehaviour
{
    protected string nombreTorre = "Torre Base";
    protected int nivel = 1;
    protected int vidaActual = 100;
    protected int dano = 15;

    [SerializeField] protected float rangoAtaque = 8f;
    [SerializeField] protected float tiempoEntreAtaques = 1.5f;
    [SerializeField] protected GameObject prefabBala;
    [SerializeField] protected Transform spawnPoint;

    protected float contadorAtaque = 0f;
    protected Transform enemigoObjetivo;

    protected virtual void Update()
    {
        if (vidaActual <= 0)
        {
            return;
        }
        BuscarEnemigoCercano();

        contadorAtaque += Time.deltaTime;

        if (contadorAtaque >= tiempoEntreAtaques && enemigoObjetivo != null)
        {
            AtacarEnemigoActual();
            contadorAtaque = 0f;
        }
    }

    protected virtual void BuscarEnemigoCercano()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, rangoAtaque);
        float distanciaMasCercana = Mathf.Infinity;
        Transform objetivoCercano = null;

        foreach (Collider elementoDetectado in colliders)
        {
            if (elementoDetectado.CompareTag("Enemy"))
            {
                float distancia = Vector3.Distance(transform.position, elementoDetectado.transform.position);

                if (distancia < distanciaMasCercana)
                {
                    distanciaMasCercana = distancia;
                    objetivoCercano = elementoDetectado.transform;
                }
            }
        }

        enemigoObjetivo = objetivoCercano;
    }

    public int CalcularFibonacci(int n)
    {
        if (n <= 0)
        {
            return 0;
        }

        if (n == 1)
        {
            return 1;
        }

        int a = 0;
        int b = 1;
        int c = 0;

        for (int i = 2; i <= n; i++)
        {
            c = a + b;
            a = b;
            b = c;
        }

        return c;
    }

    public virtual void MejorarTorre()
    {
        int costo = CalcularFibonacci(nivel + 1) * 50;
        nivel++;
        dano += 15;
        vidaActual += 50;
        Debug.Log($"{nombreTorre} subió al Nivel {nivel}! Costo: {costo} oro.");
    }

    public virtual void AtacarEnemigoActual()
    {
        if (enemigoObjetivo != null)
        {
            DispararProyectil();
        }
    }

    protected virtual void DispararProyectil()
    {
        if (prefabBala == null)
        {
            return;
        }

        Vector3 puntoOrigen;

        if (spawnPoint != null)
        {
            puntoOrigen = spawnPoint.position;
        }
        else
        {
            puntoOrigen = transform.position;
        }

        GameObject nuevaBala = Instantiate(prefabBala, puntoOrigen, Quaternion.identity);

        Projectile scriptBala = nuevaBala.GetComponent<Projectile>();
        if (scriptBala != null)
        {
            scriptBala.Configurar(enemigoObjetivo, dano);
        }
    }

    public virtual void RecibirDano(int cantidad)
    {
        vidaActual -= cantidad;
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}
