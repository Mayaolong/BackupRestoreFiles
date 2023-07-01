using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceFile
{
    public class ObservableDictionary<TKey, TValue>:Dictionary<TKey,TValue>
    {
        public Action DictionaryCountChanged;

        private Dictionary<TKey, TValue> _data;
        public ObservableDictionary():base()
        {
            //_data = new Dictionary<TKey, TValue>(); 
        }

        public new void Add(TKey key, TValue value)
        {
            //_data.Add(key, value);
            base.Add(key, value);
            if (null != DictionaryCountChanged)
            {
                DictionaryCountChanged.Invoke();
            }
        }

        public new  bool Remove(TKey key)
        {
            //var result = _data.Remove(key);
            var result = base.Remove(key);
            if (null != DictionaryCountChanged)
            {
                DictionaryCountChanged.Invoke();
            }
            return result;
        }
        public new  bool ContainsKey(TKey key)
        {
            //return _data.ContainsKey(key);
            return base.ContainsKey(key);
        }
        public bool ContainsValue(TValue value)
        {
            //return _data.ContainsValue(value);
            return base.ContainsValue(value);
        }

        public new  void Clear()
        {
            base.Clear();
            if (null != DictionaryCountChanged)
            {
                DictionaryCountChanged.Invoke();
            }
        }
   
        public new int Count { get { return base.Count; } }

        //
        // Summary:
        //     Gets a collection containing the values in the System.Collections.Generic.Dictionary`2.
        //
        // Returns:
        //     A System.Collections.Generic.Dictionary`2.ValueCollection containing the values
        //     in the System.Collections.Generic.Dictionary`2
        public ValueCollection Values
        {
            get
            {
                return base.Values;
            }
        }

        public KeyCollection Keys
        {
            get
            {
                return base.Keys;
            }
        }

        public new TValue  this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
