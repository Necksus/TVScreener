using Newtonsoft.Json;

namespace TVScreener.Controllers
{
    public class JsTreeModel
    {
        public const string RootId = "#";      // for jsTree compatibility

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("text")]
        public string Name { get; }

        [JsonProperty("parent")]
        public string ParentId { get; }

        [JsonProperty("hasChildren")]
        public bool HasChildren { get; }

        [JsonIgnore] public bool IsRoot => ParentId == RootId;

        public JsTreeModel(string id, string name, string parentId, bool hasChildren)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
            HasChildren = hasChildren;
        }

        public override string ToString() => $"{Name} ({Id})";
    }
}
