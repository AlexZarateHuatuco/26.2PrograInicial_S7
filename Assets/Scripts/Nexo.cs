using UnityEngine;
using System;

public class Nexo : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 500;

    private int vidaActual;

    public static event Action<int, int> OnVidaCambiada;
    public static event Action OnNexoDestruido;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("EnemyBullet"))
        {
            return;
        }

        Projectile bala = other.GetComponentInParent<Projectile>();
        if (bala == null)
        {
            return;
        }

        RecibirDano(bala.Dano, bala.Atacante);
        Destroy(bala.gameObject);
    }

    public void RecibirDano(int cantidad, GameObject atacante)
    {
        vidaActual -= cantidad;
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);

        if (vidaActual <= 0)
        {
            OnNexoDestruido?.Invoke();
            Debug.Log("El Nexo ha sido destruido.");
            Destroy(gameObject);
        }
    }
}