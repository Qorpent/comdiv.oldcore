using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility
{
    public static class MacroCache
    {
        public static int GetId(){
            return id++;
        }
        static  IDictionary<string, int> ids = new Dictionary<string, int>();
        public static int GetId(string key){
            if(!ids.ContainsKey(key)){
                ids[key] = 0;
            }
            ids[key] += 1;
            return ids[key];
        }
        public static void Rewind(){
            id = 1;
            ids.Clear();
        }
        static IDictionary<string,IList<TypeMember>> cache = new Dictionary<string, IList<TypeMember>>();
        private static  int id = 1;

        public static void AddMember(string cacheName,TypeMember member){
           if(!cache.ContainsKey(cacheName)){
               cache[cacheName] = new List<TypeMember>();
           }
           cache[cacheName].Add(member);
        }
        public static void BindMembers(string cacheName,TypeDefinition type){
            if (!cache.ContainsKey(cacheName)) return;
            foreach (var member in cache[cacheName]){ 
                type.Members.Add(member);
            }
            cache.Remove(cacheName);

            new Method().Parameters.Add(new ParameterDeclaration("name",new SimpleTypeReference("string")));
        }
    }
}
