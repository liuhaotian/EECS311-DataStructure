    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyFunBlob
{
    /// <summary>
    /// An abstract dictionary of objects
    /// </summary>
    public abstract class Dictionary
    {
        /// <summary>
        /// Add object to dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public abstract void Store(string key, object value);


        /// <summary>
        /// Finds and returns the value associated with the key, or null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract object Lookup(string key);


    }
}
