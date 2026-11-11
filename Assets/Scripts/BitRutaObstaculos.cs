using System.Collections.Generic;
using UnityEngine;

public static class BitRutaObstaculos
{
    public static List<Vector2> Buscar(Vector2 desde, Vector2 hasta, BoxCollider2D caja, Transform jugador)
    {
        const float paso=.5f;
        Vector2 min=Vector2.Min(desde,hasta)-Vector2.one*6;
        Vector2 max=Vector2.Max(desde,hasta)+Vector2.one*6;
        int w=Mathf.Min(180,Mathf.CeilToInt((max.x-min.x)/paso)+1), h=Mathf.Min(180,Mathf.CeilToInt((max.y-min.y)/paso)+1);
        int sx=Mathf.Clamp(Mathf.RoundToInt((desde.x-min.x)/paso),0,w-1), sy=Mathf.Clamp(Mathf.RoundToInt((desde.y-min.y)/paso),0,h-1);
        int tx=Mathf.Clamp(Mathf.RoundToInt((hasta.x-min.x)/paso),0,w-1), ty=Mathf.Clamp(Mathf.RoundToInt((hasta.y-min.y)/paso),0,h-1);
        int start=sy*w+sx, goal=ty*w+tx;
        var parent=new int[w*h];for(int i=0;i<parent.Length;i++)parent[i]=-2;
        var queue=new Queue<int>();parent[start]=-1;queue.Enqueue(start);
        var overlaps=new Collider2D[16];var filter=new ContactFilter2D();filter.useTriggers=false;filter.SetLayerMask(Physics2D.GetLayerCollisionMask(caja.gameObject.layer));
        Vector2 offset=(Vector2)caja.bounds.center-(Vector2)caja.transform.position;
        int iterations=0;
        while(queue.Count>0&&parent[goal]==-2&&iterations++<12000)
        {
            int cell=queue.Dequeue(),x=cell%w,y=cell/w;
            foreach(int next in new[]{x>0?cell-1:-1,x<w-1?cell+1:-1,y>0?cell-w:-1,y<h-1?cell+w:-1})
            {
                if(next<0||parent[next]!=-2)continue;
                Vector2 p=min+new Vector2(next%w,next/w)*paso;
                int count=Physics2D.OverlapBox(p+offset,(Vector2)caja.bounds.size+Vector2.one*.04f,0,filter,overlaps);
                bool blocked=false;
                for(int i=0;i<count;i++){var c=overlaps[i];if(c==caja||c.transform==jugador||c.transform.IsChildOf(jugador))continue;blocked=true;break;}
                if(blocked){parent[next]=-3;continue;}
                parent[next]=cell;queue.Enqueue(next);
            }
        }
        var result=new List<Vector2>();if(parent[goal]<-1)return result;
        for(int c=goal;c!=start&&c>=0;c=parent[c])result.Add(min+new Vector2(c%w,c/w)*paso);
        result.Reverse();return result;
    }
}
