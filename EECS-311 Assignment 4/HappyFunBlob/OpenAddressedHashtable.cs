using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyFunBlob
{
    public class OpenAddressedHashtable : Dictionary
    {
        string[] hashKey;
        object[] hashValue;
        int hashSize;
        int hashCount;

        private int hashFunc(string key)
        {
            int sum = 0;
            for(int i = 0; i < key.Length; i++)
            {
                sum += key[i];
            }
            return sum * (int)((Math.Sqrt(5) - 1) / 2) % hashSize;
        }

        public OpenAddressedHashtable(int size)
        {
            //throw new NotImplementedException();
            hashKey = new string[size];
            hashValue = new object[size];
            hashSize = size;
            hashCount = 0;
        }

        public override int Count
        {
            get
            {
                //throw new NotImplementedException();
                return hashCount;
            }
        }

        public override void Store(string key, object value)
        {
            //throw new NotImplementedException();
            int i;
            for (i = 0; i < hashSize; i++)
            {
                if (hashKey[(hashFunc(key) + i) % hashSize] == key)
                {
                    hashValue[(hashFunc(key) + i) % hashSize] = value;
                    break;
                }
                else if (hashKey[(hashFunc(key) + i) % hashSize] == null)
                {
                    hashValue[(hashFunc(key) + i) % hashSize] = value;
                    hashKey[(hashFunc(key) + i) % hashSize] = key;
                    hashCount++;
                    break;
                }
            }
            if (i == hashSize)
            {
                throw new HashtableFullException();
            }
        }

        public override object Lookup(string key)
        {
            //throw new NotImplementedException();
            for (int i = 0; i < hashSize; i++)
            {
                if (hashKey[(hashFunc(key) + i) % hashSize] == key)
                {
                    return hashValue[(hashFunc(key) + i) % hashSize];
                }
            }
            throw new DictionaryKeyNotFoundException(key);
        }

        private class HashCell
        {
            public string key;
            public object value;
            public HashCell next;
            public HashCell prev;
        }
    }
}
