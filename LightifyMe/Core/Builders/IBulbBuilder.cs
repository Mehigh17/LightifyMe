using System.Collections.Generic;

namespace LightifyMe.Core.Builders
{
    public interface IBulbBuilder
    {

        IBulbBuilder AddBytes(byte[] bytes);
        Bulb Build();

    }
}
