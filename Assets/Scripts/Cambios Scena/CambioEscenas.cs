using UnityEngine;
using UnityEngine.SceneManagement;
public abstract class PantallaBase : MonoBehaviour
{
    public abstract void CambiarEscena(string nombre);
}
public class CambioEscenas : PantallaBase
{
    public GameObject panelCreditos;
    public override void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
    public void VerCreditos(bool estado)
    {
        panelCreditos.SetActive(estado);
    }
    public void Salir()
    {
        Application.Quit();
    }
}