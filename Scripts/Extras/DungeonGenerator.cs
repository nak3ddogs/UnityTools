using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using _2HeadedDog.Math;

namespace DungeonGenerator
{

    [Serializable]
    public class DRoom
    {
        public DRoom(Vector2 _pos, Vector2 _size)
        {
            _pos.x = Mathf.RoundToInt(_pos.x);
            _pos.y = Mathf.RoundToInt(_pos.y);
            _size.x = Mathf.RoundToInt(_size.x);
            _size.y = Mathf.RoundToInt(_size.y);
            rect = new Rect(_pos, _size);
        }

        public Rect rect;
    }

    public class DungeonGenerator : MonoBehaviour
    {
        /*
         * 1.legeneraljuk a szobakat.
         * 2.avoidancel szettoljuk oket
         * 3.valami faktorral kivalasztjuk a fo szobakat
         * 4.csinalunk ra egy haromszog halot
         * 4.1 ebbol legeneralunk egy listat amiben minden edge csak egyszer szerepel
         * 5.spanning tree
         * 5.1 visszaadunk par folyosot
         * 6. folyoso epites
         * 
         */


        public bool drawGizmo = false;
        public int roomAmount = 20;
        public int radius = 5;
        public int roomMinWidth = 3;
        public int roomMaxWidth = 12;
        public string seed;

        bool foundOverlap = false;

        protected DRoom[] rooms;
        List<Edge> corridors = new List<Edge>();

        void Start()
        {
            StartCoroutine(Process());
        }


        IEnumerator Process()
        {
            while (true)
            {
                while (!Input.GetKeyDown(KeyCode.Space))
                {
                    yield return 0;
                }
                rooms = null;
                corridors.Clear();
                Generate();
                yield return 0;
                do
                {
                    SeparateRooms();
                    yield return 0;
                } while (foundOverlap);

                //triangulator
                Vector2[] roomsCenters = new Vector2[rooms.Length];
                for (int i = 0; i < rooms.Length; i++)
                {
                    roomsCenters[i] = rooms[i].rect.center;
                }

                int[] TriangulatePolygons = Triangulator.TriangulatePolygon(roomsCenters);
                //edges
                Debug.Log(TriangulatePolygons.Length);
                for (int i = 0; i < TriangulatePolygons.Length / 3; i++)
                {
                    int p1 = i * 3;
                    int p2 = p1 + 1;
                    int p3 = p1 + 2;
                    p1 = TriangulatePolygons[p1];
                    p2 = TriangulatePolygons[p2];
                    p3 = TriangulatePolygons[p3];
                    corridors.Add(new Edge(p1, p2));
                    corridors.Add(new Edge(p2, p3));
                    corridors.Add(new Edge(p3, p1));
                }

                corridors = KruskalAlgorithm.Kruskal(rooms.Length, corridors);

            }
        }

        void Generate()
        {
            rooms = new DRoom[roomAmount];
            if (seed != "")
            {
                UnityEngine.Random.InitState(seed.GetHashCode());
            }
            for (int i = 0; i < rooms.Length; i++)
            {
                Vector2 pos = UnityEngine.Random.insideUnitCircle * radius;

                Vector2 dimension = UnityEngine.Random.insideUnitCircle;

                dimension.x = UnityEngine.Random.Range(roomMinWidth, roomMaxWidth);

                dimension.y = UnityEngine.Random.Range(roomMinWidth, roomMaxWidth);
                rooms[i] = new DRoom(pos, dimension);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!drawGizmo)
                return;

            //drawRooms
            if (rooms == null)
                return;
            Gizmos.color = Color.red;
            foreach (var item in rooms)
            {
                Gizmos.DrawWireCube(new Vector3(item.rect.center.x, 0, item.rect.center.y), new Vector3(item.rect.size.x, 1f, item.rect.size.y));
            }
            //drawRooms end

            //draw triangles

            foreach (var item in corridors)
            {
                //if (rooms.Length <= Mathf.Max(item.p1, item.p2))
                //    return;
                Gizmos.color = Color.yellow;
                Vector3 p1 = rooms[item.p1].rect.center;
                p1.z = p1.y;
                p1.y = 0;
                Vector3 p2 = rooms[item.p2].rect.center;
                p2.z = p2.y;
                p2.y = 0;

                Gizmos.DrawLine(p1, p2);
            }
        }
#endif

        void SeparateRooms()
        {
            foundOverlap = false;
            foreach (DRoom room in rooms)
            {
                Vector2 oldPos = room.rect.position;
                Vector2 separation = computeSeparation(room);
                Vector2 newPos = new Vector2(oldPos.x += separation.x, oldPos.y += separation.y);
                room.rect.position = newPos;
            }

        }

        Vector2 computeSeparation(DRoom agent)
        {

            int neighbours = 0;
            Vector2 v = new Vector2();

            foreach (DRoom room in rooms)
            {

                if (room != agent)
                {

                    if (agent.rect.Overlaps(room.rect))
                    {
                        ///
                        Vector2 _dir = (agent.rect.center - room.rect.center);
                        _dir = _dir.normalized * Mathf.Max(Mathf.Max(agent.rect.width / 2, agent.rect.height / 2), Mathf.Max(room.rect.width / 2, room.rect.height / 2));
                        _dir.x = Mathf.RoundToInt(_dir.x);
                        _dir.y = Mathf.RoundToInt(_dir.y);
                        v = _dir;
                        neighbours++;
                    }
                }
            }
            if (neighbours == 0)
            {
                return v;
            }
            if (v == Vector2.zero)
            {
                v = agent.rect.center;
                v.x = Mathf.Sign(v.x);
                v.y = Mathf.Sign(v.y);
            }
            foundOverlap = true;
            return v;
        }
    }

}