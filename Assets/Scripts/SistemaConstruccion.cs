/*
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class OpcionTorre
{
    public string nombre = "Torre";
    public TowerBase prefab;
    public int costo = 100;
}

public enum ModoConstruccion
{
    Ninguno,
    Construir,
    Demoler
}

public class SistemaConstruccion : MonoBehaviour
{
    [SerializeField] private GestorJuego gestor;
    [SerializeField] private Camera camaraJuego;

    [SerializeField] private List<OpcionTorre> opciones = new List<OpcionTorre>();

    [SerializeField] private string etiquetaTerreno = "Terreno";
    [SerializeField] private float tamanoCelda = 2f;
    [SerializeField] private float distanciaRaycast = 200f;

    [SerializeField, Range(0f, 1f)] private float porcentajeReembolso = 0.5f;

    [SerializeField] private Color colorValido = new Color(0f, 1f, 0f, 0.6f);
    [SerializeField] private Color colorInvalido = new Color(1f, 0f, 0f, 0.6f);

    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelSeleccionTorres;
    [SerializeField] private Button botonConstruir;
    [SerializeField] private Button botonVolver;

    [SerializeField] private RectTransform contenedorBotones;
    [SerializeField] private Button prefabBotonTorre;
    [SerializeField] private Button botonDemoler;
    [SerializeField] private Button botonCancelar;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private TMP_Text textoEstadoOleada;
    [SerializeField] private TMP_Text textoModo;
    [SerializeField] private TMP_Text textoMensaje;
    [SerializeField] private float duracionMensaje = 2f;

    private class TorreConstruida
    {
        public TowerBase torre;
        public VigilanteVidaTorre vigilante;
        public Vector2Int celda;
        public int costo;
    }

    private readonly Dictionary<Vector2Int, TorreConstruida> porCelda = new Dictionary<Vector2Int, TorreConstruida>();
    private readonly Dictionary<TowerBase, TorreConstruida> porTorre = new Dictionary<TowerBase, TorreConstruida>();

    private readonly List<Button> botonesTorre = new List<Button>();
    private GameObject fantasma;
    private readonly List<Material> materialesFantasma = new List<Material>();
    private bool fantasmaValido = true;
    private float tiempoMensaje;

    public ModoConstruccion Modo { get; private set; } = ModoConstruccion.Ninguno;
    public OpcionTorre Seleccionada { get; private set; }

    private void Awake()
    {
        if (camaraJuego == null)
        {
            camaraJuego = Camera.main;
        }
    }

    private void Start()
    {
        CrearBotonesDeTorres();

        botonConstruir.onClick.AddListener(AbrirSeleccionTorres);
        botonVolver.onClick.AddListener(VolverAlMenuPrincipal);
        botonDemoler.onClick.AddListener(ActivarDemolicion);
        botonCancelar.onClick.AddListener(Cancelar);

        if (textoMensaje != null)
        {
            textoMensaje.text = "";
        }

        VolverAlMenuPrincipal();
        RefrescarModo();
    }

    public void AbrirSeleccionTorres()
    {
        panelMenuPrincipal.SetActive(false);
        panelSeleccionTorres.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        panelSeleccionTorres.SetActive(false);
        panelMenuPrincipal.SetActive(true);
    }

    private void CrearBotonesDeTorres()
    {
        for (int i = 0; i < opciones.Count; i++)
        {
            int indice = i;
            OpcionTorre opcion = opciones[i];

            Button boton = Instantiate(prefabBotonTorre, contenedorBotones);
            boton.gameObject.SetActive(true);

            TMP_Text texto = boton.GetComponentInChildren<TMP_Text>();
            if (texto != null)
            {
                texto.text = $"{opcion.nombre}\n{opcion.costo}";
            }

            boton.onClick.AddListener(() => SeleccionarTorre(indice));
            botonesTorre.Add(boton);
        }

        if (prefabBotonTorre.gameObject.scene.IsValid())
        {
            prefabBotonTorre.gameObject.SetActive(false);
        }
    }

    public void SeleccionarTorre(int indice)
    {
        if (indice < 0 || indice >= opciones.Count || opciones[indice].prefab == null)
        {
            return;
        }

        Seleccionada = opciones[indice];
        Modo = ModoConstruccion.Construir;
        CrearFantasma(Seleccionada.prefab.gameObject);
        RefrescarModo();

        VolverAlMenuPrincipal();
    }

    public void ActivarDemolicion()
    {
        Seleccionada = null;
        Modo = ModoConstruccion.Demoler;
        DestruirFantasma();
        RefrescarModo();
        VolverAlMenuPrincipal();
    }

    public void Cancelar()
    {
        Seleccionada = null;
        Modo = ModoConstruccion.Ninguno;
        DestruirFantasma();
        RefrescarModo();
        VolverAlMenuPrincipal();
    }

    private void Update()
    {
        ActualizarMenu();

        if (panelSeleccionTorres.activeSelf
            && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            VolverAlMenuPrincipal();
            return;
        }

        if (Modo == ModoConstruccion.Ninguno)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            Cancelar();
            return;
        }

        bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (Modo == ModoConstruccion.Construir)
        {
            ActualizarConstruccion(sobreUI);
        }
        else
        {
            ActualizarDemolicion(sobreUI);
        }
    }

    private void ActualizarConstruccion(bool sobreUI)
    {
        Vector2Int celda = default;
        Vector3 posicion = default;
        bool hayCelda = !sobreUI && TryObtenerCelda(out celda, out posicion);

        bool libre = hayCelda && !CeldaOcupada(celda);
        bool alcanzaOro = gestor.Monedas >= Seleccionada.costo;

        if (fantasma != null)
        {
            fantasma.SetActive(hayCelda);
            if (hayCelda)
            {
                fantasma.transform.position = posicion;
                ColorearFantasma(libre && alcanzaOro);
            }
        }

        if (Input.GetMouseButtonDown(0) && !sobreUI && hayCelda)
        {
            if (!libre)
            {
                MostrarMensaje("Esa casilla ya está ocupada.");
            }
            else if (!alcanzaOro)
            {
                MostrarMensaje("Monedas insuficientes.");
            }
            else
            {
                Construir(Seleccionada, celda, posicion);
            }
        }
    }

    private void ActualizarDemolicion(bool sobreUI)
    {
        if (sobreUI || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        Ray rayo = camaraJuego.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayo, out RaycastHit hit, distanciaRaycast, ~0, QueryTriggerInteraction.Ignore))
        {
            TowerBase torre = hit.collider.GetComponentInParent<TowerBase>();
            if (torre != null)
            {
                Demoler(torre, true);
            }
        }
    }

    private void Construir(OpcionTorre opcion, Vector2Int celda, Vector3 posicion)
    {
        if (!gestor.Gastar(opcion.costo))
        {
            MostrarMensaje("Monedas insuficientes.");
            return;
        }

        TowerBase torre = Instantiate(opcion.prefab, posicion, Quaternion.identity);

        VigilanteVidaTorre vigilante = torre.GetComponent<VigilanteVidaTorre>();
        if (vigilante == null)
        {
            vigilante = torre.gameObject.AddComponent<VigilanteVidaTorre>();
        }

        TorreConstruida registro = new TorreConstruida
        {
            torre = torre,
            vigilante = vigilante,
            celda = celda,
            costo = opcion.costo
        };

        porCelda[celda] = registro;
        porTorre[torre] = registro;

        vigilante.OnSinVida += DemolerPorDestruccion;
    }

    private void DemolerPorDestruccion(TowerBase torre)
    {
        Demoler(torre, false);
    }

    public void Demoler(TowerBase torre, bool reembolsar)
    {
        if ((object)torre == null)
        {
            return;
        }

        if (porTorre.TryGetValue(torre, out TorreConstruida registro))
        {
            porTorre.Remove(torre);
            porCelda.Remove(registro.celda);

            if (registro.vigilante != null)
            {
                registro.vigilante.OnSinVida -= DemolerPorDestruccion;
            }

            if (reembolsar)
            {
                int reembolso = Mathf.RoundToInt(registro.costo * porcentajeReembolso);
                gestor.Agregar(reembolso);
                MostrarMensaje($"Torre demolida. +{reembolso} monedas.");
            }
        }

        if (torre != null)
        {
            Destroy(torre.gameObject);
        }
    }

    private bool TryObtenerCelda(out Vector2Int celda, out Vector3 posicion)
    {
        celda = default;
        posicion = default;

        Ray rayo = camaraJuego.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(rayo, out RaycastHit hit, distanciaRaycast, ~0, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (!hit.collider.CompareTag(etiquetaTerreno) || hit.normal.y < 0.7f)
        {
            return false;
        }

        celda = new Vector2Int(
            Mathf.RoundToInt(hit.point.x / tamanoCelda),
            Mathf.RoundToInt(hit.point.z / tamanoCelda));

        Vector3 centro = new Vector3(celda.x * tamanoCelda, hit.point.y, celda.y * tamanoCelda);
        Ray abajo = new Ray(centro + Vector3.up * 50f, Vector3.down);

        if (!Physics.Raycast(abajo, out RaycastHit hitCentro, 100f, ~0, QueryTriggerInteraction.Ignore)
            || !hitCentro.collider.CompareTag(etiquetaTerreno))
        {
            return false;
        }

        posicion = hitCentro.point;
        return true;
    }

    private bool CeldaOcupada(Vector2Int celda)
    {
        if (!porCelda.TryGetValue(celda, out TorreConstruida registro))
        {
            return false;
        }

        if (registro.torre == null)
        {
            porCelda.Remove(celda);
            return false;
        }

        return true;
    }


    private void ActualizarMenu()
    {
        textoMonedas.text = $"Monedas: {gestor.Monedas}";

        if (textoEstadoOleada != null)
        {
            textoEstadoOleada.text = gestor.Estado;
        }

        for (int i = 0; i < botonesTorre.Count; i++)
        {
            botonesTorre[i].interactable = gestor.Monedas >= opciones[i].costo;
        }

        if (tiempoMensaje > 0f)
        {
            tiempoMensaje -= Time.unscaledDeltaTime;
            if (tiempoMensaje <= 0f && textoMensaje != null)
            {
                textoMensaje.text = "";
            }
        }
    }

    private void RefrescarModo()
    {
        switch (Modo)
        {
            case ModoConstruccion.Construir:
                textoModo.text = $"Construyendo: {Seleccionada.nombre} ({Seleccionada.costo})";
                break;
            case ModoConstruccion.Demoler:
                textoModo.text = "Modo demolición: clic en una torre";
                break;
            default:
                textoModo.text = "";
                break;
        }

        botonCancelar.gameObject.SetActive(Modo != ModoConstruccion.Ninguno);
    }

    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje == null)
        {
            return;
        }

        textoMensaje.text = mensaje;
        tiempoMensaje = duracionMensaje;
    }

    private void CrearFantasma(GameObject prefab)
    {
        DestruirFantasma();

        fantasma = Instantiate(prefab);
        fantasma.name = "FantasmaConstruccion";

        foreach (Collider c in fantasma.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        foreach (MonoBehaviour m in fantasma.GetComponentsInChildren<MonoBehaviour>())
        {
            m.enabled = false;
        }

        foreach (Renderer r in fantasma.GetComponentsInChildren<Renderer>())
        {
            materialesFantasma.AddRange(r.materials);
        }

        fantasma.SetActive(false);
        fantasmaValido = false;
        ColorearFantasma(true);
    }

    private void ColorearFantasma(bool valido)
    {
        if (valido == fantasmaValido)
        {
            return;
        }

        fantasmaValido = valido;
        Color color = valido ? colorValido : colorInvalido;

        foreach (Material m in materialesFantasma)
        {
            if (m != null)
            {
                m.color = color;
            }
        }
    }

    private void DestruirFantasma()
    {
        if (fantasma != null)
        {
            Destroy(fantasma);
        }

        fantasma = null;
        materialesFantasma.Clear();
    }
}
*/

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class OpcionTorre
{
    public string nombre = "Torre";
    public TowerBase prefab;
    public int costo = 100;
}

public enum ModoConstruccion
{
    Ninguno,
    Construir,
    Demoler
}

public class SistemaConstruccion : MonoBehaviour
{
    [Header("Referencias (objetos de la ESCENA, no del Project)")]
    [SerializeField] private GestorJuego gestor;
    [SerializeField] private Camera camaraJuego;

    [Header("Torres disponibles")]
    [SerializeField] private List<OpcionTorre> opciones = new List<OpcionTorre>();

    [Header("Terreno")]
    [SerializeField] private string etiquetaTerreno = "Terreno";
    [SerializeField] private float tamanoCelda = 2f;
    [SerializeField] private float distanciaRaycast = 200f;

    [Header("Demolición")]
    [SerializeField, Range(0f, 1f)] private float porcentajeReembolso = 0.5f;

    [Header("Previsualización")]
    [SerializeField] private Color colorValido = new Color(0f, 1f, 0f, 0.6f);
    [SerializeField] private Color colorInvalido = new Color(1f, 0f, 0f, 0.6f);

    [Header("Menú de construcción (Canvas)")]
    [SerializeField] private GameObject panelBotonesAccion;     // Panel con los botones Construir y Demoler (ConsDemoPanel)
    [SerializeField] private GameObject panelSeleccionTorres;   // Panel con los botones de torres (TorresPanel)
    [Tooltip("Botones de torre ya creados en el Canvas, en el mismo orden que la lista de torres. " +
             "Si se usa, no hace falta el contenedor ni la plantilla.")]
    [SerializeField] private Button[] botonesDeTorres;
    [Tooltip("Solo si NO usas 'Botones De Torres': contenedor DENTRO del panel de selección donde se crean los botones.")]
    [SerializeField] private RectTransform contenedorBotones;
    [Tooltip("Solo si NO usas 'Botones De Torres': botón plantilla con un TMP_Text hijo.")]
    [SerializeField] private Button prefabBotonTorre;
    [SerializeField] private Button botonConstruir;             // Abre el panel de torres
    [SerializeField] private Button botonDemoler;               // Activa la demolición

    [Header("Textos (opcionales)")]
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private TMP_Text textoEstadoOleada;
    [SerializeField] private TMP_Text textoModo;
    [SerializeField] private TMP_Text textoMensaje;
    [SerializeField] private float duracionMensaje = 2f;

    private class TorreConstruida
    {
        public TowerBase torre;
        public VigilanteVidaTorre vigilante;
        public Vector2Int celda;
        public int costo;
    }

    private readonly Dictionary<Vector2Int, TorreConstruida> porCelda = new Dictionary<Vector2Int, TorreConstruida>();
    private readonly Dictionary<TowerBase, TorreConstruida> porTorre = new Dictionary<TowerBase, TorreConstruida>();

    private readonly List<Button> botonesTorre = new List<Button>();
    private GameObject fantasma;
    private readonly List<Material> materialesFantasma = new List<Material>();
    private bool fantasmaValido = true;
    private float tiempoMensaje;

    public ModoConstruccion Modo { get; private set; } = ModoConstruccion.Ninguno;
    public OpcionTorre Seleccionada { get; private set; }

    private bool SeleccionAbierta => panelSeleccionTorres != null && panelSeleccionTorres.activeSelf;

    // ---------- Inicio ----------

    private void Awake()
    {
        if (camaraJuego == null)
        {
            camaraJuego = Camera.main;
        }
    }

    private void Start()
    {
        ValidarReferencias();
        CrearBotonesDeTorres();

        if (botonConstruir != null)
        {
            botonConstruir.onClick.AddListener(AbrirSeleccionTorres);
        }

        if (botonDemoler != null)
        {
            botonDemoler.onClick.AddListener(ActivarDemolicion);
        }

        if (textoMensaje != null)
        {
            textoMensaje.text = "";
        }

        // Al iniciar el nivel solo se ve el panel Construir/Demoler.
        MostrarPanelTorres(false);
        MostrarPanelAcciones(true);

        RefrescarModo();
        ActualizarBotonesTorres();
    }

    // Avisa en consola de los errores de configuración más habituales.
    private void ValidarReferencias()
    {
        if (gestor == null)
        {
            Debug.LogError("SistemaConstruccion: falta asignar el GestorJuego.", this);
        }
        else if (!gestor.gameObject.scene.IsValid())
        {
            Debug.LogError("SistemaConstruccion: el GestorJuego asignado es un prefab del Project. " +
                           "Arrastra el objeto que está en la escena; el del Project nunca inicia sus monedas.", this);
        }

        if (camaraJuego == null)
        {
            Debug.LogError("SistemaConstruccion: no hay cámara. Asigna una o pon la etiqueta MainCamera.", this);
        }

        if (panelBotonesAccion == null)
        {
            Debug.LogWarning("SistemaConstruccion: 'Panel Botones Accion' no está asignado; " +
                             "el panel Construir/Demoler no se ocultará ni se mostrará.", this);
        }

        bool usarManuales = botonesDeTorres != null && botonesDeTorres.Length > 0;

        if (!usarManuales && (contenedorBotones == null || prefabBotonTorre == null))
        {
            Debug.LogError("SistemaConstruccion: asigna 'Botones De Torres', o bien el contenedor y el botón plantilla.", this);
        }

        if (!usarManuales && contenedorBotones != null && panelSeleccionTorres != null
            && !contenedorBotones.IsChildOf(panelSeleccionTorres.transform))
        {
            Debug.LogError("SistemaConstruccion: el 'Contenedor Botones' debe estar DENTRO del panel de selección " +
                           "de torres; si no, los botones aparecen encima de otros paneles.", this);
        }

        if (panelBotonesAccion != null && transform.IsChildOf(panelBotonesAccion.transform))
        {
            Debug.LogError("SistemaConstruccion: este script está dentro del panel de acciones, que se oculta " +
                           "al elegir torres, y dejaría de funcionar. Muévelo a un objeto que siempre esté activo.", this);
        }
    }

    private void CrearBotonesDeTorres()
    {
        bool usarManuales = botonesDeTorres != null && botonesDeTorres.Length > 0;

        if (!usarManuales && (contenedorBotones == null || prefabBotonTorre == null))
        {
            return;
        }

        for (int i = 0; i < opciones.Count; i++)
        {
            int indice = i; // captura para el listener
            OpcionTorre opcion = opciones[i];

            if (opcion.prefab == null)
            {
                Debug.LogWarning($"SistemaConstruccion: la opción '{opcion.nombre}' no tiene prefab.", this);
            }

            Button boton;

            if (usarManuales)
            {
                boton = i < botonesDeTorres.Length ? botonesDeTorres[i] : null;

                if (boton == null)
                {
                    Debug.LogWarning($"SistemaConstruccion: no hay botón para la opción '{opcion.nombre}'.", this);
                    botonesTorre.Add(null);
                    continue;
                }
            }
            else
            {
                boton = Instantiate(prefabBotonTorre, contenedorBotones);
                boton.gameObject.SetActive(true);
            }

            TMP_Text texto = boton.GetComponentInChildren<TMP_Text>();
            if (texto != null)
            {
                texto.text = $"{opcion.nombre}\n{opcion.costo}";
            }

            boton.onClick.AddListener(() => SeleccionarTorre(indice));
            botonesTorre.Add(boton);
        }

        if (usarManuales)
        {
            // Los botones que sobran (sin opción asignada) se ocultan.
            for (int i = opciones.Count; i < botonesDeTorres.Length; i++)
            {
                if (botonesDeTorres[i] != null)
                {
                    botonesDeTorres[i].gameObject.SetActive(false);
                }
            }
        }
        else if (prefabBotonTorre.gameObject.scene.IsValid())
        {
            // La plantilla del botón se oculta si está en la escena.
            prefabBotonTorre.gameObject.SetActive(false);
        }
    }

    // ---------- Paneles ----------

    private void MostrarPanelAcciones(bool mostrar)
    {
        if (panelBotonesAccion != null)
        {
            panelBotonesAccion.SetActive(mostrar);
        }
    }

    private void MostrarPanelTorres(bool mostrar)
    {
        if (panelSeleccionTorres != null)
        {
            panelSeleccionTorres.SetActive(mostrar);
        }
    }

    // ---------- Modos ----------

    /// <summary>Botón Construir: oculta Construir/Demoler y muestra el panel de torres.</summary>
    public void AbrirSeleccionTorres()
    {
        MostrarPanelAcciones(false);
        MostrarPanelTorres(true);
    }

    /// <summary>Botón de torre: oculta el panel de torres y pasa a elegir la ubicación.</summary>
    public void SeleccionarTorre(int indice)
    {
        if (indice < 0 || indice >= opciones.Count || opciones[indice].prefab == null)
        {
            return;
        }

        Seleccionada = opciones[indice];
        Modo = ModoConstruccion.Construir;
        CrearFantasma(Seleccionada.prefab.gameObject);
        RefrescarModo();

        MostrarPanelTorres(false);
        MostrarPanelAcciones(false);
    }

    /// <summary>Botón Demoler: oculta Construir/Demoler y pasa a elegir la torre a demoler.</summary>
    public void ActivarDemolicion()
    {
        Seleccionada = null;
        Modo = ModoConstruccion.Demoler;
        DestruirFantasma();
        RefrescarModo();

        MostrarPanelTorres(false);
        MostrarPanelAcciones(false);
    }

    /// <summary>Sale del modo activo y vuelve al panel Construir/Demoler.</summary>
    private void Cancelar()
    {
        Seleccionada = null;
        Modo = ModoConstruccion.Ninguno;
        DestruirFantasma();
        RefrescarModo();

        MostrarPanelTorres(false);
        MostrarPanelAcciones(true);
    }

    // ---------- Bucle principal ----------

    private void Update()
    {
        if (gestor == null || camaraJuego == null)
        {
            return;
        }

        ActualizarMenu();

        bool clicDerecho = Input.GetMouseButtonDown(1);
        bool escape = Input.GetKeyDown(KeyCode.Escape);

        // Con el panel de torres abierto, clic derecho (o Esc) vuelve al panel Construir/Demoler.
        if (SeleccionAbierta && (clicDerecho || escape))
        {
            MostrarPanelTorres(false);
            MostrarPanelAcciones(true);
            return;
        }

        if (Modo == ModoConstruccion.Ninguno)
        {
            return;
        }

        // En modo construir o demoler, clic derecho (o Esc) cancela y vuelve al panel Construir/Demoler.
        if (clicDerecho || escape)
        {
            Cancelar();
            return;
        }

        bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (Modo == ModoConstruccion.Construir)
        {
            ActualizarConstruccion(sobreUI);
        }
        else
        {
            ActualizarDemolicion(sobreUI);
        }
    }

    private void ActualizarConstruccion(bool sobreUI)
    {
        Vector2Int celda = default;
        Vector3 posicion = default;
        bool hayCelda = !sobreUI && TryObtenerCelda(out celda, out posicion);

        bool libre = hayCelda && !CeldaOcupada(celda);
        bool alcanzaOro = gestor.Monedas >= Seleccionada.costo;

        if (fantasma != null)
        {
            fantasma.SetActive(hayCelda);
            if (hayCelda)
            {
                fantasma.transform.position = posicion;
                ColorearFantasma(libre && alcanzaOro);
            }
        }

        if (Input.GetMouseButtonDown(0) && !sobreUI && hayCelda)
        {
            if (!libre)
            {
                MostrarMensaje("Esa casilla ya está ocupada.");
            }
            else if (!alcanzaOro)
            {
                MostrarMensaje("Monedas insuficientes.");
            }
            else
            {
                Construir(Seleccionada, celda, posicion);
            }
        }
    }

    private void ActualizarDemolicion(bool sobreUI)
    {
        if (sobreUI || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        Ray rayo = camaraJuego.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayo, out RaycastHit hit, distanciaRaycast, ~0, QueryTriggerInteraction.Ignore))
        {
            TowerBase torre = hit.collider.GetComponentInParent<TowerBase>();
            if (torre != null)
            {
                Demoler(torre, true);
            }
        }
    }

    // ---------- Construcción / demolición ----------

    private void Construir(OpcionTorre opcion, Vector2Int celda, Vector3 posicion)
    {
        if (!gestor.Gastar(opcion.costo))
        {
            MostrarMensaje("Monedas insuficientes.");
            return;
        }

        TowerBase torre = Instantiate(opcion.prefab, posicion, Quaternion.identity);

        // El vigilante (script aparte) detecta cuándo la torre se queda sin vida.
        VigilanteVidaTorre vigilante = torre.GetComponent<VigilanteVidaTorre>();
        if (vigilante == null)
        {
            vigilante = torre.gameObject.AddComponent<VigilanteVidaTorre>();
        }

        TorreConstruida registro = new TorreConstruida
        {
            torre = torre,
            vigilante = vigilante,
            celda = celda,
            costo = opcion.costo
        };

        porCelda[celda] = registro;
        porTorre[torre] = registro;

        vigilante.OnSinVida += DemolerPorDestruccion;
    }

    /// <summary>
    /// Se ejecuta automáticamente cuando una torre se queda sin vida. Sin reembolso.
    /// </summary>
    private void DemolerPorDestruccion(TowerBase torre)
    {
        Demoler(torre, false);
    }

    /// <summary>
    /// Demuele una torre y libera su casilla. Solo reembolsa cuando lo pide el jugador.
    /// </summary>
    public void Demoler(TowerBase torre, bool reembolsar)
    {
        if ((object)torre == null)
        {
            return;
        }

        if (porTorre.TryGetValue(torre, out TorreConstruida registro))
        {
            porTorre.Remove(torre);
            porCelda.Remove(registro.celda);

            if (registro.vigilante != null)
            {
                registro.vigilante.OnSinVida -= DemolerPorDestruccion;
            }

            if (reembolsar)
            {
                int reembolso = Mathf.RoundToInt(registro.costo * porcentajeReembolso);
                gestor.Agregar(reembolso);
                MostrarMensaje($"Torre demolida. +{reembolso} monedas.");
            }
        }

        // La torre puede estar ya destruyéndose (TowerBase llama a Destroy al llegar a 0 de vida).
        if (torre != null)
        {
            Destroy(torre.gameObject);
        }
    }

    // ---------- Terreno / casillas ----------

    private bool TryObtenerCelda(out Vector2Int celda, out Vector3 posicion)
    {
        celda = default;
        posicion = default;

        Ray rayo = camaraJuego.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(rayo, out RaycastHit hit, distanciaRaycast, ~0, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (!hit.collider.CompareTag(etiquetaTerreno) || hit.normal.y < 0.7f)
        {
            return false;
        }

        celda = new Vector2Int(
            Mathf.RoundToInt(hit.point.x / tamanoCelda),
            Mathf.RoundToInt(hit.point.z / tamanoCelda));

        // Centro de la casilla; se comprueba que ese punto también sea terreno (bordes del plano).
        Vector3 centro = new Vector3(celda.x * tamanoCelda, hit.point.y, celda.y * tamanoCelda);
        Ray abajo = new Ray(centro + Vector3.up * 50f, Vector3.down);

        if (!Physics.Raycast(abajo, out RaycastHit hitCentro, 100f, ~0, QueryTriggerInteraction.Ignore)
            || !hitCentro.collider.CompareTag(etiquetaTerreno))
        {
            return false;
        }

        posicion = hitCentro.point;
        return true;
    }

    private bool CeldaOcupada(Vector2Int celda)
    {
        if (!porCelda.TryGetValue(celda, out TorreConstruida registro))
        {
            return false;
        }

        // Si la torre desapareció por otra vía, se limpia el registro.
        if (registro.torre == null)
        {
            porCelda.Remove(celda);
            return false;
        }

        return true;
    }

    // ---------- Menú ----------

    private void ActualizarMenu()
    {
        // Primero los botones, para que ningún texto sin asignar pueda impedir su actualización.
        ActualizarBotonesTorres();

        if (textoMonedas != null)
        {
            textoMonedas.text = $"Monedas: {gestor.Monedas}";
        }

        if (textoEstadoOleada != null)
        {
            textoEstadoOleada.text = gestor.Estado;
        }

        if (tiempoMensaje > 0f)
        {
            tiempoMensaje -= Time.unscaledDeltaTime;
            if (tiempoMensaje <= 0f && textoMensaje != null)
            {
                textoMensaje.text = "";
            }
        }
    }

    // Un botón de torre solo se puede pulsar si hay monedas suficientes.
    private void ActualizarBotonesTorres()
    {
        if (gestor == null)
        {
            return;
        }

        for (int i = 0; i < botonesTorre.Count; i++)
        {
            if (botonesTorre[i] != null)
            {
                botonesTorre[i].interactable = gestor.Monedas >= opciones[i].costo;
            }
        }
    }

    private void RefrescarModo()
    {
        if (textoModo == null)
        {
            return;
        }

        switch (Modo)
        {
            case ModoConstruccion.Construir:
                textoModo.text = $"Construyendo: {Seleccionada.nombre} ({Seleccionada.costo})";
                break;
            case ModoConstruccion.Demoler:
                textoModo.text = "Modo demolición: clic en una torre";
                break;
            default:
                textoModo.text = "";
                break;
        }
    }

    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje == null)
        {
            return;
        }

        textoMensaje.text = mensaje;
        tiempoMensaje = duracionMensaje;
    }

    // ---------- Fantasma de previsualización ----------

    private void CrearFantasma(GameObject prefab)
    {
        DestruirFantasma();

        fantasma = Instantiate(prefab);
        fantasma.name = "FantasmaConstruccion";

        foreach (Collider c in fantasma.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        foreach (MonoBehaviour m in fantasma.GetComponentsInChildren<MonoBehaviour>())
        {
            m.enabled = false;
        }

        foreach (Renderer r in fantasma.GetComponentsInChildren<Renderer>())
        {
            materialesFantasma.AddRange(r.materials);
        }

        fantasma.SetActive(false);
        fantasmaValido = false;
        ColorearFantasma(true);
    }

    private void ColorearFantasma(bool valido)
    {
        if (valido == fantasmaValido)
        {
            return;
        }

        fantasmaValido = valido;
        Color color = valido ? colorValido : colorInvalido;

        foreach (Material m in materialesFantasma)
        {
            if (m != null)
            {
                m.color = color;
            }
        }
    }

    private void DestruirFantasma()
    {
        if (fantasma != null)
        {
            Destroy(fantasma);
        }

        fantasma = null;
        materialesFantasma.Clear();
    }
}