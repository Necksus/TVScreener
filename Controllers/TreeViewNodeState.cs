using Newtonsoft.Json;

namespace TVScreener.Controllers
{
    public class TreeViewNodeState
    {
        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("opened")]
        public bool Opened { get; set; }

        [JsonProperty("checkbox_disabled")]
        public bool Disabled { get; set; }
    }
}
