using System.Collections.Generic;

namespace Infrastructure
{
    public class Repository<T> where T : INamed
    {
        private Dictionary<string, T> elements = new();

        public void Add(T element)
        {
            elements[element.Name] = element;
        }

        public void Remove(string name)
        {
            if (elements.ContainsKey(name))
            {
                elements.Remove(name);
            }
        }

        public T Get(string name)
        {
            return elements[name];
        }

        public IEnumerable<T> GetAll()
        {
            return elements.Values;
        }
    }
}
