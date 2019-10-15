using UnityEngine;
/// <summary>
/// 单利
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBase<T> : MonoBehaviour where T :SingletonBase<T> {

    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //GameObject obj = new GameObject(typeof(T).Name);
                //instance = obj.AddComponent<T>(); 
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        instance = this as T;
    }
}
