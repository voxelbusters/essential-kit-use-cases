using UnityEngine;
using UnityEngine.Pool;

namespace VoxelBusters.UseCases
{
    public class VisualPrefabPool<T> where T : Component
    {
        private readonly IObjectPool<T> m_pool;
        private T m_prefab;
        public VisualPrefabPool(T prefab)
        {
            m_prefab    = prefab;
            m_pool      = new UnityEngine.Pool.ObjectPool<T>(createFunc: CreateObject, actionOnGet: EnableObject, actionOnRelease: DisableObject);
        }

        public T Get()
        {
            return m_pool.Get();
        }

        public void Release(T element)
        {
            m_pool.Release(element);
        }

        private T CreateObject()
        {
            T element = Object.Instantiate(m_prefab);
            return element;
        }
        private void EnableObject(T element)
        {
            element.gameObject.SetActive(true);
        }

        private void DisableObject(T element)
        {
            element.gameObject.SetActive(false);
        }
    }
}
