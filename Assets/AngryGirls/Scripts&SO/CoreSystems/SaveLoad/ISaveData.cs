using Cysharp.Threading.Tasks;

namespace Angry_Girls
{
    public interface ISaveData<TEntity, TEntitySaveData>
    where TEntity : class, ISaveData<TEntity, TEntitySaveData>
    where TEntitySaveData : class
    {
        public TEntitySaveData ConvertToSaveData();
        public UniTask UpdateFromSaveAsync(TEntitySaveData dto);
        public void ResetData();
    }
}

