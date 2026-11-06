using UnityEngine;

public class BuffTower : TowerBase
{
    private void Start()
    {
        nombreTorre = "Torre Pesada";
        tiempoEntreAtaques = 3f;
        dano = 40;
    }

    public override void AtacarEnemigoActual()
    {
        if (enemigoObjetivo != null)
        {
            DispararProyectil();
        }
    }

    public override void MejorarTorre()
    {
        base.MejorarTorre();
        dano += 10;
    }
}
