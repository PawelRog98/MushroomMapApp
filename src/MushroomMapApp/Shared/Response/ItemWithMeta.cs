namespace MushroomMapApp.Shared.Response;

public class ItemWithMeta<TData>
{
    public TData Data { get; set; }
    public dynamic Meta { get; set; }
}