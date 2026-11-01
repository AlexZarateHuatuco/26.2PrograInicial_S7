using UnityEngine;

public class SmallTower : TowerBase
{
    private void Start()
    {
        nombreTorre = "Torre Pequeña";
        tiempoEntreAtaques = 0.5f;
        dano = 10;
    }

    public override void AtacarEnemigoActual()
    {
        if (enemigoObjetivo != null)
        {
            Debug.Log($"{nombreTorre} dispara ráfaga rápida");
            DispararProyectil();
        }
    }

    public override void MejorarTorre()
    {
        base.MejorarTorre();
        tiempoEntreAtaques -= 0.1f;

        if (tiempoEntreAtaques < 0.2f)
        {
            tiempoEntreAtaques = 0.2f;
        }
    }
}
