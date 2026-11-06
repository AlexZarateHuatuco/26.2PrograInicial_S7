using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Nexo : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 20;

    private int vidaActual;

    public static event Action<int, int> OnVidaCambiada;
    public static event Action OnNexoDestruido;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            RecibirDano(10);
        }
    }

    public void RecibirDano(int cantidad)
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
    private void OnEnable()
    {
        // Suscribirse al evento
        OnNexoDestruido += CambiarADerrota;
    }
    void CambiarADerrota()
    {
        SceneManager.LoadScene("Derrota");
    }
}