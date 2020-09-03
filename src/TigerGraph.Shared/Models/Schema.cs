using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class SchemaResult
    {
        public bool error { get; set; }
        public string message { get; set; }
        public Schema results { get; set; }
    }

    public class Schema
    {
        public string GraphName { get; set; }
        public Config Config { get; set; }
        public Vertextype[] VertexTypes { get; set; }
        public Edgetype[] EdgeTypes { get; set; }
    }

    public class Vertextype
    {
        public Attribute[] Attributes { get; set; }
        public Primaryid PrimaryId { get; set; }
        public string Name { get; set; }
    }

    public class Config
    {
        public string STATS { get; set; }
        public bool PRIMARY_ID_AS_ATTRIBUTE { get; set; }
    }

    public class Primaryid
    {
        public Attributetype AttributeType { get; set; }
        public bool IsPartOfCompositeKey { get; set; }
        public bool PrimaryIdAsAttribute { get; set; }
        public string AttributeName { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public class Attributetype
    {
        public string Name { get; set; }
    }

    public class Attribute
    {
        public Attributetype1 AttributeType { get; set; }
        public bool IsPartOfCompositeKey { get; set; }
        public bool PrimaryIdAsAttribute { get; set; }
        public string AttributeName { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public class Attributetype1
    {
        public string Name { get; set; }
    }

    public class Edgetype
    {
        public bool IsDirected { get; set; }
        public string ToVertexTypeName { get; set; }
        public string[] ToVertexTypeList { get; set; }
        public EdgeConfig Config { get; set; }
        public object[] Attributes { get; set; }
        public string FromVertexTypeName { get; set; }
        public string[] FromVertexTypeList { get; set; }
        public string Name { get; set; }
    }

    public class EdgeConfig
    {
    }

    public class VertexSchema
    {
        public bool error { get; set; }
        public string message { get; set; }
        public VertexSchemaResult results { get; set; }
    }

    public class VertexSchemaResult
    {
        public Config Config { get; set; }
        public Attribute[] Attributes { get; set; }
        public Primaryid PrimaryId { get; set; }
        public string Name { get; set; }
    }
}
