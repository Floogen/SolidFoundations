using System;
using System.Collections.Generic;

namespace SolidFoundations.Framework.Models.Data
{
    public class RandomValue
    {
        public List<object> Values { get; set; }

        public T Get<T>(Random random)
        {
            if (Values is null || Values.Count == 0)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(Values[random.Next(Values.Count)], typeof(T));
        }
    }
}
