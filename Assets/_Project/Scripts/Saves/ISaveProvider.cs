using System.Threading.Tasks;

namespace IdleBiz.Saves
{
    public interface ISaveProvider
    {
        Task<SaveData> LoadAsync();
        Task SaveAsync(SaveData data);
    }
}
