using System;
using System.Collections;
using System.Linq;
using System.Web.SessionState;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Utils{
    internal class SessionAsDictionary : IDictionary{
        public SessionAsDictionary(HttpSessionState session){
            Session = session;
        }

        private HttpSessionState Session { get; set; }

        #region IDictionary Members

        public void CopyTo(Array array, int index){
            Session.CopyTo(array, index);
        }

        public int Count{
            get { return Session.Count; }
        }

        public object SyncRoot{
            get { return Session.SyncRoot; }
        }

        public bool IsSynchronized{
            get { return Session.IsSynchronized; }
        }

        public bool Contains(object key){
            return key.isIn(Session.Keys);
        }

        public void Add(object key, object value){
            Session[key.ToString()] = value;
        }

        public void Clear(){
            Session.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator(){
            var stub = new Hashtable();
            foreach (string o in Session.Keys){
                stub[o] = Session[o];
            }
            return stub.GetEnumerator();
        }


        public void Remove(object key){
            Session.Remove(key as string);
        }

        public object this[object key]{
            get { return Session[key as string]; }
            set { Session[(string) key] = value; }
        }

        public ICollection Keys{
            get { return Session.Keys; }
        }

        public ICollection Values{
            get { return Session.Keys.Cast<string>().Select(k => Session[k]).ToList(); }
        }

        public bool IsReadOnly{
            get { return Session.IsReadOnly; }
        }

        public bool IsFixedSize{
            get { return false; }
        }

        public IEnumerator GetEnumerator(){
            return Session.GetEnumerator();
        }

        #endregion
    }
}