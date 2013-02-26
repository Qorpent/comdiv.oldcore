using System.Collections;

namespace Comdiv.FluentView{
    public interface IGenericNode<T> where T :nodeBase, IGenericNode<T>
    {
        T set(IDictionary dictionary);
        T set(string key,object value);
        T call(string name, params object[] parameters);
        T named(string name);
        T isSingle();
        T isContainer();
        T setClass(string name);
        T attr(string name,object value);
        T prefixed(string prefix);
        T content(string text);
        T hide(string name,object text);
        T subview(string name);
        T h3(string text);
        T h2(string text);
        T h1(string text);
        T h(int level,string text);
        T subview(string name, IDictionary parameters);
        T href(string to, string onclick,string content);
        T jsref(string js,string content);
        T href(string to, string onclick, string target,string content);
        T empty(params string[] id_and_clases);
    }
}