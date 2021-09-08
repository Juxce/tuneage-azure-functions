using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Domain.TableEntities;

namespace Juxce.Tuneage.Common {
    public class Mapper {
        public static Label LabelEntityToReturnObject(LabelTableEntity entity) {
            return new Label {
                RowKey = entity.RowKey,
                ShortName = entity.ShortName,
                LongName = entity.LongName,
                Url = entity.Url,
                Profile = entity.Profile
            }; 
        }
    }
}