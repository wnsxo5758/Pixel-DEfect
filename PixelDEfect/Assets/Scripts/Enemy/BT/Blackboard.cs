using System.Collections.Generic;

public class Blackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public void SetValue<T>(string key, T value)
    {
        if (data.ContainsKey(key))
        {
            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }
    }

    public T GetValue<T>(string key)
    {
        if (data.ContainsKey(key))
        {
            return (T)data[key];
        }
        
        return default(T);
    }

    public bool ContainsKey(string key)
    {
        return data.ContainsKey(key);
    }
}
