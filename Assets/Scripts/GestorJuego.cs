using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GrupoEnemigos
{
    public GameObject prefab;
    public int cantidad = 5;
    public float intervalo = 1f;
    public int recompensa = 10;
}

[Serializable]
public class Oleada
{
    public List<GrupoEnemigos> grupos = new List<GrupoEnemigos>();
}

public class GestorJuego : MonoBehaviour
{
    [SerializeField] private int monedasIniciales = 200;
    [SerializeField] private Transform[] puntosSpawn;
    [SerializeField] private float tiempoAntesPrimeraOleada = 10f;
    [SerializeField] private float tiempoEntreOleadas = 15f;
    [SerializeField] private List<Oleada> oleadas = new List<Oleada>();

    public int Monedas { get; private set; }
    public string Estado { get; private set; } = "";

    private class EnemigoVivo
    {
        public GameObject objeto;
        public int recompensa;
    }

    private readonly List<EnemigoVivo> vivos = new List<EnemigoVivo>();

    private void Awake()
    {
        Monedas = monedasIniciales;
    }

    private void OnEnable()
    {
        Nexo.OnNexoDestruido += AlPerder;
    }

    private void OnDisable()
    {
        Nexo.OnNexoDestruido -= AlPerder;
    }

    private void Start()
    {
        StartCoroutine(RutinaOleadas());
    }

    private void Update()
    {
        for (int i = vivos.Count - 1; i >= 0; i--)
        {
            if (vivos[i].objeto == null)
            {
                Monedas += vivos[i].recompensa;
                vivos.RemoveAt(i);
            }
        }
    }

    public void Agregar(int cantidad)
    {
        Monedas += cantidad;
    }

    public bool Gastar(int cantidad)
    {
        if (Monedas < cantidad)
        {
            return false;
        }

        Monedas -= cantidad;
        return true;
    }

    private IEnumerator RutinaOleadas()
    {
        yield return Cuenta(tiempoAntesPrimeraOleada, "Primera oleada en");

        for (int i = 0; i < oleadas.Count; i++)
        {
            Estado = $"Oleada {i + 1}/{oleadas.Count}";

            foreach (GrupoEnemigos grupo in oleadas[i].grupos)
            {
                for (int n = 0; n < grupo.cantidad; n++)
                {
                    GenerarEnemigo(grupo);
                    yield return new WaitForSeconds(grupo.intervalo);
                }
            }

            while (vivos.Count > 0)
            {
                yield return null;
            }

            if (i < oleadas.Count - 1)
            {
                yield return Cuenta(tiempoEntreOleadas, $"Oleada {i + 2} en");
            }
        }

        Estado = "¡Victoria!";
    }

    private IEnumerator Cuenta(float segundos, string texto)
    {
        for (float t = segundos; t > 0f; t -= Time.deltaTime)
        {
            Estado = $"{texto} {Mathf.CeilToInt(t)}s";
            yield return null;
        }
    }

    private void GenerarEnemigo(GrupoEnemigos grupo)
    {
        if (grupo.prefab == null || puntosSpawn.Length == 0)
        {
            return;
        }

        Transform punto = puntosSpawn[UnityEngine.Random.Range(0, puntosSpawn.Length)];
        Vector3 posicion = punto.position;

        if (NavMesh.SamplePosition(posicion, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            posicion = hit.position;
        }

        GameObject enemigo = Instantiate(grupo.prefab, posicion, punto.rotation);
        vivos.Add(new EnemigoVivo { objeto = enemigo, recompensa = grupo.recompensa });
    }

    private void AlPerder()
    {
        StopAllCoroutines();
        Estado = "Derrota";
    }
}