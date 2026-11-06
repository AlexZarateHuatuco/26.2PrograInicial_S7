using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class PantallaBase : MonoBehaviour
{
    public abstract void CambiarEscena(string nombre);
}

public class CambioEscenas : PantallaBase
{
    [SerializeField] private GameObject panelCreditos;
    [SerializeField] private bool esNivelDeJuego = false;
    private bool yaTuvoTorres = false;

    public static event Action OnVictoria;

    public GameObject PanelCreditos
    {
        get { return panelCreditos; }
        set { panelCreditos = value; }
    }

    public bool EsNivelDeJuego
    {
        get { return esNivelDeJuego; }
        set { esNivelDeJuego = value; }
    }

    private void Start()
    {
        OnVictoria += IrAVictoria;
    }

    private void OnDestroy()
    {
        OnVictoria -= IrAVictoria;
    }

    private void Update()
    {
        if (esNivelDeJuego == false)
        {
            return;
        }

        VerificarTorres();
    }

    public void VerificarTorres()
    {
        TowerBase[] torresActuales = UnityEngine.Object.FindObjectsByType<TowerBase>(FindObjectsSortMode.None);

        if (torresActuales.Length > 0)
        {
            yaTuvoTorres = true;
        }

        if (yaTuvoTorres == true && torresActuales.Length == 0)
        {
            CambiarEscena("Derrota");
        }
    }

    public static void LlamarVictoria()
    {
        if (OnVictoria != null)
        {
            OnVictoria();
        }
    }

    private void IrAVictoria()
    {
        CambiarEscena("Victoria");
    }

    public override void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }

    public void VerCreditos(bool estado)
    {
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(estado);
        }
    }

    public void Salir()
    {
        Application.Quit();
    }
}