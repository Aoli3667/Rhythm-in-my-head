using System;

public interface IObjectPool<T>
{
    void InitPool();

    T CreatePoolObj();

    void OnTakeFromPool(T poolObj);

    void OnReturnedToPool(T poolObj);

    void OnDestroyPool(T poolObj);

    T Get();

    void ReturnToPool(T poolObj);
}
