using System;
using System.Collections;


namespace Comdiv.Transformation{
    /// <summary>
    /// Summary description for OldInterfaces.
    /// </summary>
    [Obsolete]
    //TODO: Попытаться сдвинуть в MT.Transform
    public interface ITransformator{
        bool Transparent { get; set; }
        Hashtable Parameters { get; }
        ITransformator Parent { get; set; }
        string Transform(string data, params object[] parameters);
        bool IsApplicable(string data);
        object GetParameter(object key);
        void SetParameter(object key, object value);
    }
}