using System;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(TowerBase))]
public class VigilanteVidaTorre : MonoBehaviour
{
    
    private static readonly FieldInfo campoVida = typeof(TowerBase).GetField(
        "vida",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private TowerBase torre;
    private bool notificado;

    public event Action<TowerBase> OnSinVida;

    private void Awake()
    {
        torre = GetComponent<TowerBase>();

        if (campoVida == null)
        {
            Debug.LogError("VigilanteVidaTorre: no se encontró el campo 'vida' en TowerBase.");
            enabled = false;
        }
    }

    private void Update()
    {
        Comprobar();
    }

    private void OnDestroy()
    {
        Comprobar();
    }

    private void Comprobar()
    {
        if (notificado || campoVida == null || (object)torre == null)
        {
            return;
        }

        int vida = (int)campoVida.GetValue(torre);

        if (vida <= 0)
        {
            notificado = true;
            OnSinVida?.Invoke(torre);
        }
    }
}