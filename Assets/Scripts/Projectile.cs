using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int danoAplicar;
    private Transform objetivo;
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private float tiempoDeVidaMaximo = 1.2f;
    [SerializeField] private float distanciaMaxima = 10f;

    public void Configurar(Transform objetivoEnemigo, int dano)
    {
        this.objetivo = objetivoEnemigo;
        this.danoAplicar = dano;

        Destroy(gameObject, tiempoDeVidaMaximo);
    }

    private void Update()
    {
        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }

        float distanciaAlObjetivo = Vector3.Distance(transform.position, objetivo.position);
        if (distanciaAlObjetivo > distanciaMaxima)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direccion = (objetivo.position - transform.position).normalized;
        transform.position += direccion * velocidad * Time.deltaTime;
        transform.LookAt(objetivo);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
