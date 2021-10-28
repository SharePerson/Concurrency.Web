using Omu.ValueInjecter;
using System.Collections.Generic;
using System.Linq;

namespace Concurrency.Services.Base
{
    public abstract class Mapper
    {
        protected DestinationType MapObject<SourceType, DestinationType>(SourceType source)
            where SourceType : class
            where DestinationType : class
        {
            return Omu.ValueInjecter.Mapper.Map<SourceType, DestinationType>(source);
        }

        protected IEnumerable<DestinationType> MapList<SourceType, DestinationType>(IEnumerable<SourceType> sourceList)
            where SourceType : class
            where DestinationType : class, new()
        {
            return sourceList?.Select(s => new DestinationType().InjectFrom(s)).Cast<DestinationType>();
        }
    }
}
