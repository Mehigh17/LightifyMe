namespace LightifyMeCore.Builders
{
    public interface IBulbBuilder
    {

        IBulbBuilder AddBytes(byte[] bytes);
        Bulb Build();

    }
}
