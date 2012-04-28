using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyFunBlob
{
    public class ListDictionary : Dictionary
    {
        private DictCell head;
        private DictCell tail;
        private DictCell temp;

        public ListDictionary()
        {
            //throw new NotImplementedException();
            head = new DictCell();
            tail = head;
        }

        public override void Store(string key, object value)
        {
            //throw new NotImplementedException();
            try
            {
                temp = new DictCell();
                temp.value = this.Lookup(key);

                temp.value = value;
            }
            catch (DictionaryKeyNotFoundException e)
            {
                temp = new DictCell();
                temp.next = head;
                head.prev = temp;
                temp.value = value;
                temp.key = key;

                head = temp;
            }
        }

        public override object Lookup(string key)
        {
            //throw new NotImplementedException();
            temp = head;
            while (temp != null)
            {
                if (temp.key == key)
                {
                    return temp.value;
                }
                temp = temp.next;
            }
            throw new DictionaryKeyNotFoundException(key);
        }

        public class DictCell
        {
            public DictCell next;
            public DictCell prev;
            public string key;
            public object value;
        }
    }
}
