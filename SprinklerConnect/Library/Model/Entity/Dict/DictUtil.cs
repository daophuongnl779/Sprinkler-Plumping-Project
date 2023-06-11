using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public static class DictUtil
    {
        public static T AddItem<T>(this Dict<T> q)
        {
            var newItem = (T)Activator.CreateInstance(typeof(T), new object[] { });
            q.Items.Add(newItem);

            dynamic newItem_any = newItem;
            try
            {
                newItem_any.Dict = q;
            }
            catch
            {

            }

            return newItem;
        }

        public static Dict<T> ToDict<T>(this IEnumerable<T> items)
        {
            return new Dict<T>
            {
                Items = items.ToList()
            };
        }
    }
}
