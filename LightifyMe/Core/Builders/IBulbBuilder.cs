using System.Collections.Generic;

namespace LightifyMe.Core.Builders
{
    public interface IBulbBuilder
    {

        IBulbBuilder AddBytes(IEnumerable<byte> bytes);
        Bulb Build();

    }
}
