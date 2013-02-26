using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Comdiv.Useful{
    public abstract class AbstractUniqueStringProvider : IUniqueStringProvider{
       
        public string New(){
            lock (this){
                var result = InternalNew(null);
                while (existedCache.Contains(result)){
                    result = InternalNew(result);
                }
                existedCache.Add(result);
                return result;
            }
        }

        protected abstract string InternalNew(string currentResult);
        public object cacheWriteLock = new object();
        public void Lock(IEnumerable<string> existed){
            lock (cacheWriteLock){
                foreach (var s in existed)
                {
                    if (!existedCache.Contains(s))
                    {
                        existedCache.Add(s);
                    }
                }    
            }
            
        }

        public void Release(IEnumerable<string> notexisted){
            lock (cacheWriteLock){
                foreach (var s in notexisted){
                    existedCache.Remove(s);
                }
            }
        }

        public void Release(string code){
            lock (cacheWriteLock)
            {
                
                    existedCache.Remove(code);
               
            }
        }

        IList<string> existedCache = new List<string>();
        
    }
}