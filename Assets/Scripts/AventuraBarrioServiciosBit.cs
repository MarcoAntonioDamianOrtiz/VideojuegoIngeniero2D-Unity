using UnityEngine;

public sealed partial class AventuraBarrio
{
    sealed class SalaServicioBit
    {
        public string Nombre;
        public bool Veterinaria;
        public Vector2 Entrada;
        public Transform Raiz;
        public Vector2 Centro => Raiz.position;
        public Vector2 Puerta => Centro + new Vector2(0, -3.25f);
        public SpriteRenderer Personaje;
        public Sprite[] Poses;
        public Vector2 Mostrador => new Vector2(Personaje.transform.position.x, Centro.y + 1.05f);
    }

    SalaServicioBit hospitalBit, tiendaBit, interiorBit;
    CamaraSeguirJugador camaraExteriorBit;
    Sprite pixelInteriorBit;

    bool EnInteriorBit => interiorBit != null;
    public Vector2? PosicionExteriorBit => interiorBit?.Entrada;

    void ConfigurarServiciosBit()
    {
        if (Camera.main) camaraExteriorBit = Camera.main.GetComponent<CamaraSeguirJugador>();
        if (!camaraExteriorBit) camaraExteriorBit = FindFirstObjectByType<CamaraSeguirJugador>();
        var textura = Texture2D.whiteTexture;
        pixelInteriorBit = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height),
            new Vector2(.5f, .5f), textura.width);

        var hospital = GameObject.Find("Edificio_Clinica");
        var tienda = GameObject.Find("Edificio_Ferreteria");
        
        if (!tienda) tienda = GameObject.Find("Miscelanea");

        if (!hospital)
        {
            hospital = new GameObject("Consultorio veterinario");
            hospital.transform.SetParent(transform);
            hospital.transform.position = (tienda ? tienda.transform.position : jugador.position) + Vector3.right * 8f;
            var dibujo = hospital.AddComponent<SpriteRenderer>();
            dibujo.sprite = Resources.Load<Sprite>("ServiciosBit/FachadaClinica");
            hospital.transform.localScale = Vector3.one * (5f / dibujo.sprite.bounds.size.x);
            dibujo.sortingOrder = 15000 - Mathf.RoundToInt(dibujo.bounds.min.y * 100);
            var solido = hospital.AddComponent<BoxCollider2D>();
            solido.size = dibujo.sprite.bounds.size * .75f;
        }
        if (tienda && tienda.TryGetComponent<SpriteRenderer>(out var comercio))
        {
            var fachada = Resources.Load<Texture2D>("ServiciosBit/FachadaTienda");
            var antes = comercio.sprite.bounds;
            float ppu = fachada.width / antes.size.x;
            var pivote = new Vector2(-antes.min.x / antes.size.x, -antes.min.y * ppu / fachada.height);
            comercio.sprite = CrearSpriteBit(fachada, new Rect(0, 0, fachada.width, fachada.height), pivote, ppu);
        }

        hospitalBit = CrearServicioBit(hospital, true, new Vector2(180, 0));
        tiendaBit = CrearServicioBit(tienda, false, new Vector2(210, 0));
        if (ProgresoTechnopolis.EnfermarBitEnNivel2())
            Avisar("Bit se siente mal. Llévalo al veterinario.");
    }

    SalaServicioBit CrearServicioBit(GameObject edificio, bool veterinaria, Vector2 centro)
    {
        string nombre = veterinaria ? "Hospital veterinario" : "Tienda de croquetas";
        Vector2 entrada;
        var colision = edificio ? edificio.GetComponentInChildren<BoxCollider2D>() : null;
        var fachada = edificio ? edificio.GetComponentInChildren<SpriteRenderer>() : null;
        if (colision)
            entrada = new Vector2(colision.bounds.center.x, colision.bounds.min.y - .85f);
        else if (fachada)
            entrada = new Vector2(fachada.bounds.center.x, fachada.bounds.min.y - .85f);
        else
            entrada = (Vector2)jugador.position + new Vector2(veterinaria ? -4f : 4f, -2f);

        var marca = new GameObject(nombre + " · entrada").transform;
        marca.SetParent(transform);
        marca.position = entrada;
        RectanguloSala(marca, "Placa", new Vector2(0f, .27f), new Vector2(3.35f, .48f),
            new Color32(15, 33, 40, 240), 29299);
        RotuloSala(marca, veterinaria ? "VETERINARIA" : "CROQUETAS", new Vector2(0, .27f), .14f);
        Marcar(marca, entrada, veterinaria ? "● VET" : "● TIENDA", veterinaria ? Verde : Oro);

        var guardada = GameObject.Find("Servicios de Bit/" + nombre);
        var raiz = guardada ? guardada.transform : new GameObject(nombre).transform;
        if (!guardada)
        {
            raiz.SetParent(transform);
            raiz.position = centro;
        }
        var sala = new SalaServicioBit { Nombre = nombre, Veterinaria = veterinaria, Entrada = entrada, Raiz = raiz };
        DecorarSalaBit(sala);
        raiz.gameObject.SetActive(false);

        return sala;
    }

    SalaServicioBit ServicioCercano()
    {
        if (hospitalBit != null && CercaDeServicio(hospitalBit.Entrada, 1.5f)) return hospitalBit;
        if (tiendaBit != null && CercaDeServicio(tiendaBit.Entrada, 1.5f)) return tiendaBit;
        return null;
    }

    bool CercaDeServicio(Vector2 punto, float radio) =>
        ((Vector2)jugador.position - punto).sqrMagnitude <= radio * radio;

    string PromptInteriorBit()
    {
        if (CercaDeServicio(interiorBit.Puerta, .9f)) return "Salir al distrito (E)";
        if (!CercaDeServicio(interiorBit.Mostrador, 1.55f)) return "";
        return interiorBit.Veterinaria ? "Hablar con la veterinaria (E)" : "Hablar con el tendero (E)";
    }

    bool InteractuarServicioBit()
    {
        if (interiorBit != null)
        {
            if (CercaDeServicio(interiorBit.Puerta, .9f)) SalirDeServicioBit();
            else if (CercaDeServicio(interiorBit.Mostrador, 1.55f)) DialogoServicioBit();
            return true;
        }

        var sala = ServicioCercano();
        if (sala == null || !camaraExteriorBit) return false;

        interiorBit = sala;
        sala.Raiz.gameObject.SetActive(true);
        MoverAlexYBit(sala.Puerta + Vector2.up * 1.05f, sala.Puerta + new Vector2(-1.15f, .75f));
        camaraExteriorBit.EnfocarInterior(sala.Centro);
        Avisar(sala.Nombre + " · Acércate al mostrador.");
        ActualizarVisibilidad();
        return true;
    }

    void SalirDeServicioBit()
    {
        var sala = interiorBit;
        interiorBit = null;
        sala.Raiz.gameObject.SetActive(false);
        MoverAlexYBit(sala.Entrada + Vector2.down * .45f, sala.Entrada + new Vector2(-.9f, -.6f));
        camaraExteriorBit.EnfocarInterior(null);
        ActualizarVisibilidad();
    }

    void MoverAlexYBit(Vector2 destinoAlex, Vector2 destinoBit)
    {
        MoverPersonaje(jugador, destinoAlex);
        if (bit) MoverPersonaje(bit, destinoBit);
        Physics2D.SyncTransforms();
    }

    static void MoverPersonaje(Transform personaje, Vector2 destino)
    {
        personaje.position = new Vector3(destino.x, destino.y, personaje.position.z);
        if (!personaje.TryGetComponent<Rigidbody2D>(out var cuerpo)) return;
        cuerpo.position = destino;
        cuerpo.linearVelocity = Vector2.zero;
    }

    void DialogoServicioBit()
    {
        jugando = false;
        movimientoAnterior = movimiento.enabled;
        movimiento.enabled = false;
        modal.SetActive(true);
        tituloModal.text = interiorBit.Personaje.name + " · " + interiorBit.Nombre;

        if (interiorBit.Veterinaria)
        {
            if (!ProgresoTechnopolis.BitEnfermo)
            {
                textoModal.text = "Bit ya se recuperó. Las croquetas de la tienda le ayudarán a recuperar el ánimo.";
                MostrarOpciones(new[] { "Volver a la sala" }, _ => Cerrar());
                return;
            }

            textoModal.text = $"Bit tiene fiebre y necesita tratamiento. La consulta y la medicina cuestan ${ProgresoTechnopolis.PrecioVeterinario:N0}.";
            MostrarOpciones(new[] { $"Pagar tratamiento · ${ProgresoTechnopolis.PrecioVeterinario:N0}", "Ahora no" }, opcion =>
            {
                if (opcion != 0) { Cerrar(); return; }
                textoModal.text = ProgresoTechnopolis.CurarBit()
                    ? "Bit se recuperó. Ya puede comer croquetas y seguir la aventura contigo."
                    : "Todavía no tienes suficiente dinero. Puedes ganar más completando trabajos en el distrito.";
                MostrarOpciones(new[] { "Volver a la sala" }, _ => Cerrar());
            });
            return;
        }

        MostrarCompraCroquetas("Cada ración recupera hasta 30 puntos de bienestar cuando se la das a Bit.");
    }

    void MostrarCompraCroquetas(string mensaje)
    {
        int cantidad = ProgresoTechnopolis.Croquetas;
        textoModal.text = $"{mensaje}\n\nRaciones: {cantidad}/{ProgresoTechnopolis.MaxCroquetas}   ·   Saldo: ${EconomiaBarrio.Obtener().Dinero:N0}";
        if (cantidad >= ProgresoTechnopolis.MaxCroquetas)
        {
            MostrarOpciones(new[] { "Volver a la tienda" }, _ => Cerrar());
            return;
        }
        MostrarOpciones(new[] { $"Comprar una ración · ${ProgresoTechnopolis.PrecioCroquetas:N0}", "Volver a la tienda" }, opcion =>
        {
            if (opcion != 0) { Cerrar(); return; }
            MostrarCompraCroquetas(ProgresoTechnopolis.ComprarCroquetas()
                ? "Ración comprada. Usa «Dar croquetas» cuando Bit la necesite."
                : "No tienes dinero para otra ración.");
        });
    }

    readonly System.Collections.Generic.List<Sprite> spritesBit = new System.Collections.Generic.List<Sprite>();

    Sprite CrearSpriteBit(Texture2D imagen, Rect recorte, Vector2 pivote, float ppu)
    {
        var sprite = Sprite.Create(imagen, recorte, pivote, ppu, 0, SpriteMeshType.FullRect);
        spritesBit.Add(sprite);
        return sprite;
    }

    void DecorarSalaBit(SalaServicioBit sala)
    {
        var raiz = sala.Raiz;
        bool vet = sala.Veterinaria;
        var fondo = Resources.Load<Sprite>("ServiciosBit/" + (vet ? "Consulta" : "Comercio"));
        if (!raiz.TryGetComponent<SpriteRenderer>(out var suelo)) suelo = raiz.gameObject.AddComponent<SpriteRenderer>();
        suelo.sprite = fondo;
        suelo.sortingOrder = -10;

        string nombreNpc = vet ? "Dra. Vera" : "Bruno";
        var npc = raiz.Find(nombreNpc);
        if (!npc)
        {
            npc = new GameObject(nombreNpc).transform;
            npc.SetParent(raiz, false);
            npc.localPosition = new Vector3(.4f, 1.30f, 0);
        }
        if (!npc.TryGetComponent(out sala.Personaje)) sala.Personaje = npc.gameObject.AddComponent<SpriteRenderer>();
        sala.Personaje.sortingOrder = 14000;
        var atlas = Resources.Load<Texture2D>("ServiciosBit/" + (vet ? "Veterinaria" : "Tendero"));
        int ancho = 80;
        sala.Poses = new Sprite[atlas.width / ancho];
        for (int i = 0; i < sala.Poses.Length; i++)
            sala.Poses[i] = CrearSpriteBit(atlas, new Rect(i * ancho, 0, ancho, atlas.height),
                new Vector2(.5f, 0), atlas.height / 2.5f);
        sala.Personaje.sprite = sala.Poses[0];

        if (!raiz.Find("Mostrador"))
        {
            var frente = new GameObject("Mostrador").transform;
            frente.SetParent(raiz, false);
            frente.localPosition = new Vector3(0, 1.625f, 0);
            var mostrador = frente.gameObject.AddComponent<SpriteRenderer>();
            mostrador.sprite = CrearSpriteBit(fondo.texture, new Rect(160, 214, 256, 38), new Vector2(.5f,.5f), 40f);
            mostrador.sortingOrder = 14100;
        }
        ParedSala(raiz, new Vector2(vet ? 0f : -.35f, 1.62f), new Vector2(vet ? 4.6f : 5.5f,.85f));
        ParedSala(raiz, new Vector2(-4.85f,.58f), new Vector2(2.5f,vet ? 1.1f : 1.95f));
        ParedSala(raiz, new Vector2(4.85f,.58f), new Vector2(2.5f,vet ? .95f : 1.95f));
        RotuloSala(raiz, "SALIDA", new Vector2(0,-3.32f), .10f);
        ParedSala(raiz, new Vector2(-7, 0), new Vector2(.25f, 8.2f));
        ParedSala(raiz, new Vector2(7, 0), new Vector2(.25f, 8.2f));
        ParedSala(raiz, new Vector2(0, 2.65f), new Vector2(14, .32f));
        ParedSala(raiz, new Vector2(0, -4.07f), new Vector2(14, .25f));
    }

    void RectanguloSala(Transform padre, string nombre, Vector2 centro, Vector2 tamano, Color color, int orden, bool solido = false)
    {
        var pieza = new GameObject(nombre).transform;
        pieza.SetParent(padre, false);
        pieza.localPosition = centro;
        pieza.localScale = new Vector3(tamano.x, tamano.y, 1);
        var imagen = pieza.gameObject.AddComponent<SpriteRenderer>();
        imagen.sprite = pixelInteriorBit;
        imagen.color = color;
        imagen.sortingOrder = orden;
        if (solido) pieza.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void ParedSala(Transform raiz, Vector2 centro, Vector2 tamano)
    {
        var pared = new GameObject("Pared sólida").transform;
        pared.SetParent(raiz, false);
        pared.localPosition = centro;
        pared.gameObject.AddComponent<BoxCollider2D>().size = tamano;
    }

    static void RotuloSala(Transform raiz, string texto, Vector2 centro, float tamano)
    {
        var objeto = new GameObject(texto);
        objeto.transform.SetParent(raiz, false);
        objeto.transform.localPosition = centro;
        var rotulo = objeto.AddComponent<TextMesh>();
        rotulo.text = texto;
        rotulo.anchor = TextAnchor.MiddleCenter;
        rotulo.fontSize = 60;
        rotulo.characterSize = tamano * .46f;
        rotulo.color = new Color32(244, 250, 229, 255);
        rotulo.GetComponent<MeshRenderer>().sortingOrder = 29300;
    }

    void AnimarDependiente()
    {
        int indice = Mathf.FloorToInt(Time.time * 3f) % (interiorBit.Poses.Length + 7);
        interiorBit.Personaje.sprite = interiorBit.Poses[Mathf.Max(0, indice - 7)];
    }

    void LiberarServiciosBit()
    {
        foreach (var sprite in spritesBit) if (sprite) Destroy(sprite);
        if (pixelInteriorBit) Destroy(pixelInteriorBit);
    }
}
