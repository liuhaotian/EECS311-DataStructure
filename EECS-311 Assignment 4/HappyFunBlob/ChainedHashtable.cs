using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyFunBlob
{
    public class ChainedHashtable : Dictionary
    {
        HashCell[] hashArray;
        int hashSize;
        int hashCount;

        private int hashFunc(string key)
        {
            int sum = 0;
            for(int i = 0; i < key.Length; i++)
            {
                sum += key[i];
            }
            return (sum * (int)((Math.Sqrt(5) - 1) / 2)) % hashSize;
        }

        public ChainedHashtable(int size)
        {
            //throw new NotImplementedException();
            hashArray = new HashCell[size];
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
            HashCell tempCell = hashArray[this.hashFunc(key)];

            //the first empty cell
            if (tempCell == null)
            {
                hashArray[this.hashFunc(key)] = new HashCell();
                tempCell = hashArray[this.hashFunc(key)];
                tempCell.value = value;
                tempCell.key = key;
                tempCell.next = null;
                tempCell.prev = null;
                hashCount++;
            }
            else
            {

                while (tempCell != null)
                {

                    if (tempCell.key == key)
                    {
                        tempCell.value = value;
                        break;
                    }

                    if (tempCell.next == null)
                    {
                        tempCell.next = new HashCell();
                        hashCount++;
                        tempCell.next.prev = tempCell;
                        tempCell = tempCell.next;
                        tempCell.next = null;
                        tempCell.key = key;
                        tempCell.value = value;
                    }

                    tempCell = tempCell.next;
                }
            }

            

        }

        public override object Lookup(string key)
        {
            //throw new NotImplementedException();
            HashCell tempCell;
            tempCell = hashArray[hashFunc(key)];
            while (tempCell !=null)
            {
                if (tempCell.key == key)
                {
                    return tempCell.value;
                }
                tempCell = tempCell.next;
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
