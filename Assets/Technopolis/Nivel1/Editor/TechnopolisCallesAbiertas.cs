#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TechnopolisCallesAbiertas
{
    const string Scene="Assets/Scenes/EscenaNivel1_FielReferencia.unity";
    const string Suelo="Assets/Technopolis/Nivel1/Sprites/01_Terreno/calles_barrio_despejadas.png";
    static readonly Rect[] Calles={new Rect(10.2f,2,3.6f,60),new Rect(25.2f,2,3.6f,60),new Rect(47.4f,2,3.6f,60),new Rect(16,9,47,3),new Rect(1,20.5f,62,3),new Rect(1,40.7f,62,3.2f),new Rect(1,52.4f,62,3.4f)};
    static readonly Rect[] Terrazas={new Rect(16,2.8f,47,17.9f),new Rect(12.8f,23,11.3f,9.6f),new Rect(34,22.8f,16.5f,6.8f),new Rect(28.8f,30,19.3f,10.9f),new Rect(29,43,19,8.9f)};
    static Rect Area(Bounds b)=>Rect.MinMaxRect(b.min.x+40,(25-b.max.y)/1.25f,b.max.x+40,(25-b.min.y)/1.25f);
    static Vector3 Mundo(Vector2 p)=>new Vector3(p.x-40,25-p.y*1.25f,0);
    static bool Calle(Rect r)=>Calles.Any(c=>c.Overlaps(r));
    static Rect Margen(Rect r,float m)=>Rect.MinMaxRect(r.xMin-m,r.yMin-m,r.xMax+m,r.yMax+m);
    static SpriteRenderer Dibujo(Transform t)=>t.GetComponentInChildren<SpriteRenderer>();

    [MenuItem("Technopolis/Nivel 1/Organizar manzanas y despejar calles")]
    public static void Aplicar()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!=Scene||EditorApplication.isPlaying)throw new Exception("Abre el nivel 1 fuera de Play.");
        var barrio=GameObject.Find("Grid").transform.Find("Barrio_Fiel_Referencia");
        if(barrio.Find("Calles_Despejadas"))throw new Exception("Las calles ya están aplicadas; no se vuelve a reducir la escala.");
        string copia=Path.Combine(Path.GetTempPath(),"Technopolis_antes_calles_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity");File.Copy(Scene,copia,false);
        var edificios=barrio.Find("Edificios_Sprites");
        var ocupados=new List<Rect>();
        var taller=edificios.Find("Taller");if(taller){taller.localScale*=.9f;taller.position+=Vector3.right;}
        foreach(Transform t in edificios)if(t.gameObject.activeSelf&&!t.name.StartsWith("Vivienda_")){var s=Dibujo(t);if(s)ocupados.Add(Margen(Area(s.bounds),.25f));}
        // Plaza, parque y casa conservan sus plataformas y el espacio comunitario.
        ocupados.Add(new Rect(28.5f,29.7f,19.9f,11.4f));ocupados.Add(new Rect(28.7f,42.8f,19.6f,9.4f));ocupados.Add(new Rect(12.8f,23,11.3f,9.6f));
        int colocadas=0,reserva=0;
        foreach(var t in edificios.Cast<Transform>().Where(t=>t.name.StartsWith("Vivienda_")&&t.gameObject.activeSelf).OrderBy(t=>t.name).ToArray())
        {
            var s=Dibujo(t);if(!s)continue;t.localScale*=.85f;
            Rect original=Area(s.bounds);Rect mejor=original;float coste=float.MaxValue;
            for(float y=2;y+original.height<63;y+=.5f)for(float x=1;x+original.width<63;x+=.5f)
            {
                var candidato=new Rect(x,y,original.width,original.height);
                if(Calle(Margen(candidato,.2f))||ocupados.Any(o=>o.Overlaps(Margen(candidato,.25f))))continue;
                // El basurero queda industrial; no se meten viviendas dentro de él.
                if(candidato.Overlaps(new Rect(0,0,15.5f,20)))continue;
                float distancia=(candidato.center-original.center).sqrMagnitude;
                if(distancia<coste){coste=distancia;mejor=candidato;}
            }
            if(coste==float.MaxValue){t.gameObject.SetActive(false);reserva++;continue;}
            Vector2 delta=mejor.position-original.position;t.position+=new Vector3(delta.x,-delta.y*1.25f,0);ocupados.Add(Margen(mejor,.2f));colocadas++;
        }
        var accesos=new List<Rect>();
        foreach(Transform t in edificios)if(t.gameObject.activeSelf){var s=Dibujo(t);if(s){var r=Area(s.bounds);accesos.Add(new Rect(r.xMin,r.yMax-.1f,r.width,1.1f));}}
        int retirados=0,reubicados=0;
        var objetos=barrio.Find("Objetos_Sprites");
        foreach(Transform t in objetos)
        {
            if(!t.gameObject.activeSelf)continue;
            string n=t.name.ToLowerInvariant();
            if(n.Contains("porton")||n.Contains("salida")||n.Contains("tunel")||n.Contains("arbol_plaza")||n.Contains("fuente_parque")||n.Contains("cancha")||n.Contains("auto_taller")||n=="senal_bloqueo_301")continue;
            var sr=Dibujo(t);if(!sr)continue;
            var col=t.GetComponentInChildren<Collider2D>();
            Rect huella=col?Area(col.bounds):new Rect(t.position.x+40-.25f,(25-t.position.y)/1.25f-.25f,.5f,.5f);
            bool tapa=Calle(Margen(huella,.35f))||accesos.Any(a=>a.Overlaps(huella));
            if(!tapa)continue;
            if(n.StartsWith("poste_luz_"))
            {
                Vector3 antes=t.position;Rect r=Area(sr.bounds);Vector2 ground=new Vector2(t.position.x+40,(25-t.position.y)/1.25f);
                var opciones=new[]{ground+Vector2.left*2.4f,ground+Vector2.right*2.4f,ground+Vector2.up*2.4f,ground+Vector2.down*2.4f};
                bool ok=false;foreach(var point in opciones){var test=new Rect(point.x-.4f,point.y-.35f,.8f,.7f);if(point.x<1||point.x>63||point.y<1||point.y>63||Calle(test)||ocupados.Any(o=>o.Contains(point)))continue;t.position=Mundo(point);ok=true;break;}
                if(ok){var glow=barrio.Find("Efectos_Luz/Halo_"+t.name);if(glow)glow.position+=t.position-antes;reubicados++;continue;}
            }
            t.gameObject.SetActive(false);var halo=barrio.Find("Efectos_Luz/Halo_"+t.name);if(halo)halo.gameObject.SetActive(false);retirados++;
        }
        CrearSuelo(barrio);
        Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log($"CALLES_BARRIO_OK viviendas={colocadas}, reserva={reserva}, decoracionFueraDePaso={retirados}, farolasReubicadas={reubicados}, respaldo={copia}");
    }
    static void CrearSuelo(Transform barrio)
    {
        const int lado=1024;var texture=new Texture2D(lado,lado,TextureFormat.RGBA32,false);var pixels=new Color32[lado*lado];
        for(int y=0;y<lado;y++)for(int x=0;x<lado;x++)
        {
            Vector2 p=new Vector2((x+.5f)/16f,64-(y+.5f)/16f);bool road=Calles.Any(c=>c.Contains(p));
            if(!road||Terrazas.Skip(1).Any(t=>t.xMin!=34&&t.Contains(p)))continue;
            uint hash=Hash(x,y);
            float moteado=Mathf.PerlinNoise(x*.018f,y*.018f);
            int ruido=(int)(hash&7)-3+Mathf.RoundToInt((moteado-.5f)*24);
            float edge=Calles.Where(c=>c.Contains(p)).Max(c=>Mathf.Min(Mathf.Min(p.x-c.xMin,c.xMax-p.x),Mathf.Min(p.y-c.yMin,c.yMax-p.y)));
            float irregular=.08f+Mathf.PerlinNoise(x*.09f,y*.09f)*.16f;
            byte alpha=(byte)(edge<irregular?95:edge<irregular+.15f?185:255);
            uint celda=Hash(x/4,y/4);bool piedra=(celda&127)==0&&x%4<2&&y%4<2;
            bool desgaste=Mathf.PerlinNoise(x*.07f+50,y*.07f+50)>.71f;
            int extra=desgaste?-9:0;
            // Tierra apisonada en el centro, ripio irregular en las orillas.
            int centro=Mathf.RoundToInt(Mathf.SmoothStep(0,1,Mathf.Clamp01(edge/1.1f))*9);
            bool ripio=edge<.38f&&(hash&31)<5;
            bool surco=edge>.55f&&edge<.72f&&moteado>.46f;
            if(surco)extra-=10;
            bool bordePiedra=edge>.18f&&edge<.63f;
            int junta=(x+(y/4%2)*3)%6;
            if(bordePiedra){int tono=(int)(Hash(x/6,y/4)%25)-12;bool union=junta==0||y%4==0;pixels[y*lado+x]=union?new Color32(105,92,67,alpha):new Color32((byte)(158+tono),(byte)(145+tono),(byte)(114+tono),alpha);}
            else pixels[y*lado+x]=piedra||ripio?new Color32((byte)(139+ruido),(byte)(126+ruido),(byte)(99+ruido),alpha):new Color32((byte)(179+ruido+extra+centro),(byte)(140+ruido+extra+centro),(byte)(87+ruido+extra+centro),alpha);
        }
        texture.SetPixels32(pixels);texture.Apply();File.WriteAllBytes(Suelo,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(Suelo);
        var importer=(TextureImporter)AssetImporter.GetAtPath(Suelo);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.spritePixelsPerUnit=16;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.userData="Suelo original procedural para las calles de TECHNOPOLIS. Generado por TechnopolisCallesAbiertas.cs, sin fuentes externas.";importer.SaveAndReimport();
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Suelo);var existing=barrio.Find("Calles_Despejadas");var go=existing?existing.gameObject:new GameObject("Calles_Despejadas",typeof(SpriteRenderer));go.transform.SetParent(barrio,false);go.transform.position=new Vector3(-8,-15,0);var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=-840;
        go.transform.localScale=new Vector3(64/sprite.bounds.size.x,64/sprite.bounds.size.y,1);
        go.transform.position+=new Vector3(-8,-15,0)-renderer.bounds.center;
    }
    static uint Hash(int x,int y){unchecked{uint h=(uint)(x*374761393+y*668265263);h=(h^(h>>13))*1274126177;return h^(h>>16);}}
    public static void RefrescarSuelo(){var b=GameObject.Find("Grid").transform.Find("Barrio_Fiel_Referencia");CrearSuelo(b);EditorSceneManager.SaveScene(b.gameObject.scene);}
    public static void Verificar()
    {
        var barrio=GameObject.Find("Grid").transform.Find("Barrio_Fiel_Referencia");if(!barrio.Find("Calles_Despejadas"))throw new Exception("Faltan las calles");
        int count=0;foreach(Transform t in barrio.Find("Edificios_Sprites"))if(t.gameObject.activeSelf&&t.name.StartsWith("Vivienda_")){var s=Dibujo(t);if(Calle(Margen(Area(s.bounds),.1f)))throw new Exception("Vivienda invade avenida: "+t.name);count++;}
        if(count<25)throw new Exception("Faltan viviendas activas: "+count);
        Debug.Log("CALLES_VERIFICADAS viviendas="+count+"; avenidas sin fachadas encima");
    }
}
#endif
