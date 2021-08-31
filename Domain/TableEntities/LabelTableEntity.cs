using System;
using Microsoft.Azure.Cosmos.Table;

namespace Juxce.Tuneage.Domain.TableEntities {
    [Serializable()]
    public class LabelTableEntity : TableEntity {
        public string ShortName { get; set; }

        public string LongName { get; set; }  

        public string Url { get; set; }
        
        public string Profile { get; set; }
    }
}