using System;
using Newtonsoft.Json;

namespace Juxce.Tuneage.Domain {
    [Serializable()]
    public class Label {
        [JsonProperty(PropertyName = "shortName")]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "longName")]
        public string LongName { get; set; }
        
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "profile")]
        public string Profile { get; set; }
    }
}