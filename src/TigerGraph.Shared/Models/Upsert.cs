using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class UpsertAttr
    {
        public object value { get; set; }
        public string op { get; set; }

    }

    public class UpsertOp
    {
        public static readonly string ignore_if_exists = "ignore";

        public static readonly string add = "add";

        public static readonly string and = "and";

        public static readonly string or = "or";

        public static readonly string max = "max";

        public static readonly string min = "min";
    }

    public class VerticesUpsert : Dictionary<string, Dictionary<string, Dictionary<string, UpsertAttr>>> { }

    public class EdgesUpsert : Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, UpsertAttr>>>>> { }

    public class Upsert
    {
        public VerticesUpsert vertices { get; set; }
        public EdgesUpsert edges { get; set; }

        public Upsert()
        {
            vertices = new VerticesUpsert();
            edges = new EdgesUpsert();
        }
        public Upsert AddVertexType(string name)
        {
            vertices.Add(name, new Dictionary<string, Dictionary<string, UpsertAttr>>());
            return this;
        }

        public void AddVertexTypes(params string[] names) => Array.ForEach(names, (n) => AddVertexType(n));

        public Upsert AddVertex(string typeName, string vertexId)
        {
            var attrs = new Dictionary<string, UpsertAttr>();
            vertices[typeName].Add(vertexId, attrs);
            return this;
        }

        public Upsert AddVertexAttributes(string typeName, string vertexId, string vertexOP, params ValueTuple<string, object>[] attrs)
        {
            foreach (var kv in attrs)
            {
                vertices[typeName][vertexId].Add(kv.Item1, new UpsertAttr() { value = kv.Item2, op = vertexOP});
            }
            return this;
        }
    }

    public class UpsertResult
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public UpsertResultCount[] results { get; set; }
        public string code { get; set; }
    }


    public class UpsertResultCount
    {
        public int accepted_vertices { get; set; }
        public int accepted_edges { get; set; }
    }


    public class EdgeUpsert
    {
        public string v_id { get; set; }
        public string v_type { get; set; }
        public Dictionary<string, ValueTuple<object, string>> attributes { get; set; }
    }


    public class UpsertError
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public string code { get; set; }
    }

}
