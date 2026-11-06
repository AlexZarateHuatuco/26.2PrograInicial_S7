using UnityEngine;
using System;

public abstract class TowerBase : MonoBehaviour, IMejorable
{
    protected string nombreTorre = "Torre Base";
    protected int nivel = 1;
    protected int dano = 15;

    [SerializeField] protected float rangoAtaque = 8f;
    [SerializeField] protected float tiempoEntreAtaques = 1.5f;
    [SerializeField] protected GameObject prefabBala;
    [SerializeField] protected Transform spawnPoint;

    protected float contadorAtaque = 0f;
    protected Transform enemigoObjetivo;

    public static event Action<string> OnTorreDispara;

    protected virtual void Update()
    {
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
        Debug.Log($"{nombreTorre} subió al Nivel {nivel}! Costo: {costo} oro.");
    }

    public abstract void AtacarEnemigoActual();
   
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

        OnTorreDispara?.Invoke(nombreTorre);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}
