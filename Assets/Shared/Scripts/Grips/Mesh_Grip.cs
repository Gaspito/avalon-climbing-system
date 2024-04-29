using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class Quad
    {
        public int a;
        public int b;
        public int c;
        public int d;
        public Vector3 normal;
        public Quad(int v1, int v2, int v3)
        {
            a = v1;
            b = v2;
            c = v3;
        }
        public bool Contains(int i)
        {
            return (a == i || b == i || c == i || d == i);
        }
    }

    public class MeshGripEdge
    {
        public Vector3 start;
        public Vector3 end;
        public Vector3 normal;
    }

    public class MeshGripPoint
    {
        public Vector3 point;
        public Vector3 normal;
    }

    public class Mesh_Grip : Grip
    {
        public Mesh mesh;

        public enum GripType { POINT, EDGE, SURFACE};

        public GripType grip_type;

        public List<MeshGripEdge> edges = new List<MeshGripEdge>();
        public List<MeshGripPoint> points = new List<MeshGripPoint>();

        public override void OnStart()
        {
            points = new List<MeshGripPoint>();
            edges = new List<MeshGripEdge>();
            ReadMesh();
            CreateAABB();
            base.OnStart();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public bool IsInitialized()
        {
            return (mesh != null && (edges.Count > 0 && points.Count > 0));
        }

        void ReadMesh()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null)
            {
                mesh = filter.sharedMesh;
            }
            if (mesh == null) return;

            List<Quad> quads = new List<Quad>();
            //Debug.Log("Tri Count: " + mesh.triangles.Length.ToString());
            for (int i=0; i < mesh.triangles.Length; i+=3)
            {
                int a = mesh.triangles[i];
                int b = mesh.triangles[i+1];
                int c = mesh.triangles[i+2];
                //Debug.Log ("Tri: " + a.ToString() +"; "+b.ToString()+"; "+c.ToString() );
                if (quads.Count > 0)
                {
                    bool part_of_quad = false;
                    foreach (Quad q in quads)
                    {
                        if (IsTrisPartOfQuad(q, a, b, c))
                        {
                            part_of_quad = true;
                            break;
                        }
                    }
                    if (!part_of_quad)
                    {
                        quads.Add(new Quad(a, b, c));
                    }
                } else
                {
                    quads.Add(new Quad(a, b, c));
                }
            }
            //Debug.Log("Quad Count: " + quads.Count.ToString());

            if (grip_type == GripType.EDGE)
            {
                foreach (Quad q in quads)
                {
                    Vector3 a = mesh.vertices[q.a];
                    Vector3 b = mesh.vertices[q.b];
                    Vector3 c = mesh.vertices[q.c];
                    Vector3 d = mesh.vertices[q.d];
                    Vector3[] e = GetEdge(new List<Vector3>() { a, b, c, d });
                    a = mesh.normals[q.a];
                    b = mesh.normals[q.b];
                    c = mesh.normals[q.c];
                    d = mesh.normals[q.d];
                    Vector3 n = Vector3.Normalize(a + b + c + d);
                    MeshGripEdge edge = new MeshGripEdge();
                    edge.start = transform.TransformPoint( e[0]);
                    edge.end = transform.TransformPoint(e[1]);
                    edge.normal = transform.TransformDirection( n);
                    edges.Add(edge);
                }
                //Debug.Log("Edges: " + edges.Count);
            } else if (grip_type== GripType.POINT)
            {
                foreach (Quad q in quads)
                {
                    Vector3 a = mesh.vertices[q.a];
                    Vector3 b = mesh.vertices[q.b];
                    Vector3 c = mesh.vertices[q.c];
                    Vector3 d = mesh.vertices[q.d];
                    Vector3 p = (a + b + c + d) / 4.0f;
                    a = mesh.normals[q.a];
                    b = mesh.normals[q.b];
                    c = mesh.normals[q.c];
                    d = mesh.normals[q.d];
                    Vector3 n = Vector3.Normalize(a + b + c + d);
                    MeshGripPoint point = new MeshGripPoint();
                    point.point = transform.TransformPoint(p);
                    point.normal = transform.TransformDirection(n);
                    points.Add(point);
                }
            }
        }

        Vector3[] GetEdge(List<Vector3> list)
        {
            Vector3 t = list[0];
            float dist = 0;
            int id = -1;
            for (int i = 1; i < list.Count; i++)
            {
                float d = Vector3.Distance(list[i], t);
                if (d < dist || id < 0)
                {
                    id = i;
                    dist = d;
                }
            }
            Vector3 v1 = (t + list[id]) / 2;
            list.Remove(list[id]);
            list.Remove(t);
            Vector3 v2 = (list[0] + list[1]) / 2;
            return new Vector3[2] { v1, v2 };
        }

        bool IsTrisPartOfQuad(Quad q, int a, int b, int c)
        {
            bool qa = q.Contains(a);
            bool qb = q.Contains(b);
            bool qc = q.Contains(c);
            if (qa || qb || qc)
            {
                //Debug.Log ("-> Added to Quad");
                if (!qa) q.d = a;
                if (!qb) q.d = b;
                if (!qc) q.d = c;
                return true;
            }
            return false;
        }

        public override List<GripInfo> GetPoints(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            if (ShouldComputeGripInfo(base_pos))
            {
                if (grip_type == GripType.EDGE)
                {
                    return GetEdgeInfos(input_dir, base_pos, reach);
                }
                else if (grip_type == GripType.POINT)
                {
                    return GetPointInfos(input_dir, base_pos, reach);
                }
            }
            return base.GetPoints(input_dir, base_pos, reach);
        }

        List<GripInfo> GetEdgeInfos(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            List<GripInfo> output = new List<GripInfo>();
            foreach (MeshGripEdge edge in edges)
            {
                Vector3 sp = base_pos - edge.start;
                Vector3 se = edge.end - edge.start;
                Vector3 sq = Vector3.Project(sp, se);
                Vector3 q = edge.start + sq;
                Vector3 pq = q - base_pos;
                if (pq.magnitude == reach)
                {
                    output.Add(new GripInfo(q, edge.normal));
                    continue;
                }
                else if (pq.magnitude > reach)
                {
                    continue;
                }
                float angle = Mathf.Acos(pq.magnitude / reach);
                float aq_length = Mathf.Sin(angle) * reach;
                Vector3 a = q + sq.normalized * aq_length;
                Vector3 b = q - sq.normalized * aq_length;

                Vector3 qa = a - q;
                Vector3 qb = b - q;
                Vector3 sa = a - edge.start;
                Vector3 sb = b - edge.start;
                Vector3 ea = a - edge.end;
                Vector3 eb = b - edge.end;
                Vector3 eq = q - edge.end;

                if (sa.magnitude < se.magnitude && ea.magnitude < se.magnitude)
                    output.Add(new GripInfo(a, edge.normal));
                if (sb.magnitude < se.magnitude && eb.magnitude < se.magnitude)
                    output.Add(new GripInfo(b, edge.normal));
                if (sq.magnitude < se.magnitude && eq.magnitude < se.magnitude)
                    output.Add(new GripInfo(q, edge.normal));
            }
            return output;
        }

        List<GripInfo> GetPointInfos(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            List<GripInfo> output = new List<GripInfo>();
            foreach (MeshGripPoint point in points)
            {
                if (Vector3.Distance(point.point, base_pos) <= reach)
                    output.Add( new GripInfo(point.point, point.normal));
            }
            return output;
        }

        static float compute_distance = 4;

        public Bounds AABB;

        void CreateAABB()
        {
            AABB = new Bounds(transform.position, Vector3.zero);
            foreach(MeshGripEdge edge in edges)
            {
                AABB.Encapsulate(edge.start);
                AABB.Encapsulate(edge.end);
            }
            foreach (MeshGripPoint point in points)
            {
                AABB.Encapsulate(point.point);
            }
        }

        public bool ShouldComputeGripInfo(Vector3 base_pos)
        {
            if (AABB.Contains(base_pos)) return true;
            Vector3 hit = AABB.ClosestPoint(base_pos);
            float dist = Vector3.Distance(hit, base_pos);
            //dist_to_player = dist;
            if (dist <= compute_distance) return true;
            return false;
        }
    }
}