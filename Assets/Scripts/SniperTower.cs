using UnityEngine;

public class SniperTower : TowerBase
{
    private void Start()
    {
        nombreTorre = "Torre Francotirador";
        tiempoEntreAtaques = 4f;
        dano = 60;
        rangoAtaque = 12f;
    }

    public override void AtacarEnemigoActual()
    {
        if (enemigoObjetivo != null)
        {
            DispararProyectil();
        }
    }
}
