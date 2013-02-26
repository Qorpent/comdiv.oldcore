using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Qorpent.Bxl;

namespace Comdiv.Controllers
{
	public delegate IList<IEntityDataPattern> HqlObjectDelegate(string query);
	public delegate IList<object[]> HqlArrayDelegate(string query);
    /// <summary>
    /// Обеспечивает взаимодействие с моделями NHibernate, основной административный инструмент
    /// активизируется через модуль hql
    /// </summary>
    [Admin]
    public class HqlController:BaseController {
        private string _system;
        public string System { 
            get { return _system; }
            set { PropertyBag["system"] = _system = value; }
        }
        private string _query;
        public string Query
        {
            get { return _query; }
            set { PropertyBag["query"] = _query = value; }
        }

        private string _autosystem;
        public string AutoSystem
        {
            get { return _autosystem; }
            set { PropertyBag["autosystem"] = _autosystem = value; }
        }
        private string _autoquery;
        public string AutoQuery
        {
            get { return _autoquery; }
            set { PropertyBag["autoquery"] = _autoquery = value; }
        }

        private string _type;
        public string Type
        {
            get { return _type; }
            set { PropertyBag["type"] = _type = value; }
        }
        private string _view;
        public string View
        {
            get { return _view; }
            set { PropertyBag["view"] = _view = value; }
        }
        private bool _usecodes;
        public bool UseCodes
        {
            get { return _usecodes; }
            set { PropertyBag["usecodes"] = _usecodes = value; }
        }
        private string _entity;
        public string Entity
        {
            get { return _entity; }
            set { PropertyBag["entity"] = _entity = value; }
        }

        private object _createdobj;
        public object CreatedObj
        {
            get { return _createdobj; }
            set { PropertyBag["createdobj"] = _createdobj = value; }
        }
        private int _createdobjid;
        public int CreatedObjId
        {
            get { return _createdobjid; }
            set { PropertyBag["createdobjid"] = _createdobjid = value; }
        }
        private int _id;
        private string _log;

        public int Id
        {
            get { return _id; }
            set { PropertyBag["id"] = _id = value; }
        }
        public override void Contextualize(Castle.MonoRail.Framework.IEngineContext engineContext, Castle.MonoRail.Framework.IControllerContext context)
        {
            base.Contextualize(engineContext, context);
            this.System = GetParam("system", "Default");
            this.Query = GetParam("query","");
            this.Type = GetParam("type", "");
            this.View = GetParam("view", "");
            this.Id = GetParam("id", "0").toInt();
            this.UseCodes = GetParam("usecodes", "").toBool();
            //if(this.View.hasContent()) {
            //    this.SelectedViewName = this.View;
            //}
            this.Entity = GetParam("entity", "");
            this.AutoQuery = GetParam("autoquery", "");
            this.AutoSystem = GetParam("autosystem", "");


        }

        public void index() {
            
        }

        public void autocomplete() {
            var hql = getAutoCompleteHql();
            PropertyBag["result"] = myapp.storage.GetDefault().WithSystem(System).Query(hql);
        }

        public void cexecute() {
            PropertyBag["result"] = myapp.storage.GetDefault().WithSystem(System).Query(Query);
        }

		public void dexecute(string current)
		{
			PropertyBag["result"] = myapp.storage.GetDefault().WithSystem(System).QueryEntities(Query);
			PropertyBag["selected"] = SlashListHelper.ReadList(current).ToList();
		}


        [Admin]
        public void delete() {
            using (var tts = new TemporaryTransactionSession(myapp.ioc.get<ISessionFactoryProvider>().Get(System))) {
                var map =
                    Enumerable.FirstOrDefault(myapp.ioc.get<IConfigurationProvider>().Get(this.System).ClassMappings,
                                              x =>
                                              {
                                                  if (Type.Contains("."))
                                                  {
                                                      return x.EntityName.ToLower() == Type.ToLower();
                                                  }
                                                  else
                                                  {
                                                      return x.EntityName.ToLower().EndsWith("." + Type.ToLower());
                                                  }
                                              });
                var obj = tts.Session.Load(map.ClassName.toType(), Id);
                tts.Session.Delete(obj);
                tts.Session.Flush();
                tts.Commit();
            }
            RenderText("OK");
        }

        public void execute() {
            PropertyBag["usefields"] = false;
            PropertyBag["useid"] = false;
            PropertyBag["fields"] = new string[] {};
            PropertyBag["ididx"] = -1;
            var q_ = Query.replace(@"/\*[\s\S]+?\*/","");
        	Query = q_;
            if (q_.like(@"^\s*((create)|(transform))\s+")) {
                processCreation();
                return;
                ;
            }
            if (q_.like(@"(?ix)[\r\n]+\s*GO\s*[\r\n]+"))
            {
                var queries =
                    Regex.Split(q_, @"(?ix)[\r\n]+\s*GO\s*[\r\n]+").Select(x => x.Trim()).Where(
                        x => x.hasContent() && x != "GO");
                var results = new List<HqlResult>();
                foreach (var query in queries) {
                    var result = new HqlResult();
                    var parsed = new HqlParserLite().Parse(query);
                    if (parsed.Processed && parsed.IsSimple && parsed.Fields.Count==0)
                    {
                        result.Entity = parsed.TableName;
                        result.Items = myapp.storage.GetDefault().WithSystem(System).Query(query).ToList();
                        var cols1 = new HqlColumnLoader().GetColumns(result.Entity, View);
                        result.Columns = cols1;
                        results.Add(result);
                    }else {
                        throw  new Exception("cannot execute non simple queries in batch");
                    }

                }
                PropertyBag["results"] = results;
                PropertyBag["multiple"] = true;
            }
            else {
                
                var parsed = new HqlParserLite().Parse(q_);

                if (parsed.Processed && parsed.TableName.hasContent()) {
                    Entity = parsed.TableName;
                    Type = Entity;
                }
                if (parsed.Processed && parsed.IsSimple && parsed.Fields.Count != 0) {

                    PropertyBag["usefields"] = true;
                    if (parsed.Fields.Contains("Id")) {
                        PropertyBag["useid"] = true;
                        PropertyBag["ididx"] = parsed.Fields.IndexOf("Id");
                    }
                    PropertyBag["fields"] = parsed.Fields.ToArray();
                }
                PropertyBag["result"] = myapp.storage.GetDefault().WithSystem(System).Query(q_);

            }
            var cols = new HqlColumnLoader().GetColumns(Type, View);
        	foreach (var hqlColumn in cols.Values) {
        		hqlColumn.System = System;
        	}
            PropertyBag["columns"] = cols;
			HqlObjectDelegate hqlobjects = q => myapp.storage.GetDefault().WithSystem(System).Query(q).OfType<IEntityDataPattern>().ToList();
			HqlArrayDelegate hqlarrays = q => myapp.storage.GetDefault().WithSystem(System).Query(q).OfType<object[]>().ToList();
        	PropertyBag["hqlobjects"] = hqlobjects;
        	PropertyBag["hqlarrays"] = hqlarrays;
        }


        public string Log {
            get { return this._log; }
        }

        private void processCreation() {
#if OLDCREATION
            Entity = Regex.Match(Query, @"^\s*create\s+(\w+)").Groups[1].Value;
            Type = Entity;
            var applyes = Regex.Matches(Query,@"(?<name>\w+)\s*=\s*'(?<value>[^']*)'");
            var dict = new Dictionary<string, string>();
            foreach (Match applye in applyes) {
                dict[applye.Groups["name"].Value] = applye.Groups["value"].Value;
            }
            create(dict);
            //Query = "from " + Entity + " where Id = " + CreatedObjId;
            PropertyBag["result"] = new object[] { CreatedObj };
#else

			Query = Query.replace(@"/\*[\s\S]+?\*/", "");
            var createscriptxml = new BxlParser().Parse(this.Query,"main");
        	var transform = createscriptxml.Element("transform");
			if(null!=transform) {
				transform.Remove();
				var file = transform.attr("code")+".xslt";
				var path = myapp.files.Resolve(file,true);
				if(path.noContent()) {
					throw new Exception("cannot find transform file "+file);
				}
				var xslt = new XslCompiledTransform();
				xslt.Load(path,XsltSettings.TrustedXslt,new XmlUrlResolver());
				var sw = new StringWriter();
				var arglist = new XsltArgumentList();
				arglist.AddParam("timestamp","",DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
				xslt.Transform(createscriptxml.CreateReader(),arglist,sw);
				createscriptxml = XElement.Parse(sw.ToString());
			}
            var creations = createscriptxml.Elements("create");
            var result = new List<HqlResult>();
            foreach (var creation in creations) {
                var tree = creation.attr("tree");
                var subresult = new HqlResult();
                subresult.Entity = creation.attr("id");
                subresult.Items = new List<object>();
                if (creation.Elements(subresult.Entity).Count() == 0) {
                    creation.Add(new XElement(subresult.Entity));
                }
                var items = creation.Descendants(subresult.Entity);
                foreach (var item in items) {
                    var selfattributes = item.Attributes();
                    var parentattributes =
                        item.Parent.Attributes().Where(
                            x => null == selfattributes.FirstOrDefault(y => y.Name.LocalName == x.Name.LocalName));
                    var allattributes =
                        selfattributes.Union(parentattributes).Where(x => x.Name.LocalName != "id").ToArray();
                    var dict = new Dictionary<string, string>();
                    foreach (var o in allattributes) {
                        dict[o.Name.LocalName] = o.Value;
                    }
                    if(tree.hasContent() && item.Parent!=null && item.Parent.Name.LocalName!="create") {
                        dict[tree] = item.Parent.attr("code");
                    }
                    create(dict, subresult.Entity);
                    subresult.Items.Add(this.CreatedObj);
                }
                var cols = new HqlColumnLoader().GetColumns(subresult.Entity, View);
                subresult.Columns = cols;
                result.Add(subresult);

                

            }
            PropertyBag["results"] = result;
            PropertyBag["multiple"] = true;

#endif
        }

        private string getAutoCompleteHql() {
            var hql = string.Format("from {0} where ", Type);
            if(Query.like(@"^\s+$")) {
                hql = string.Format("{0} Id = {1} or Code = '{1}'", hql, Query);
            }else {
                var q = Query;
                if(!q.Contains("%")) {
                    q = "%" + Query + "%";
                }
                hql = string.Format("{0} Name like '{1}' or Code like '{1}'", hql, q);
            }
            return hql;
        }

       private void create(IDictionary<string ,string > directdict = null, string  entity = null) {
           entity = entity ?? Entity;
           var dict = ConvertPrefixedParametersToDict("new.");
           if(directdict!=null) {
               foreach (var v in directdict) {
                   dict[v.Key] = v.Value;
               }
           }
           var a = myapp.ioc.all<IHibernatePropertyApplyer>().FirstOrDefault();
           if(null==a) {
               throw new Exception("IHibernatePropertyApplyer не настроен");
           }
           a.WithSystem(System);
           try {
               a.Start(entity,dict.get("code",""));
               foreach (var v in dict) {
                   //if (null != v.Key && null != v.Value) {
                       a.Apply(v.Key, v.Value);
                   //}
               }
               a.Commit();
               CreatedObj = a.Entity;
               CreatedObjId = a.Entity.Id();
           }finally {
               a.Dispose();
           }
	
       }


        [Admin]
        public void update(int id, string prop, string value) {
            var a = myapp.ioc.all<IHibernatePropertyApplyer>().FirstOrDefault();
            if (null == a)
            {
                throw new Exception("IHibernatePropertyApplyer не настроен");
            }
            a.WithSystem(System);
            a.Apply(Entity,id,prop,value);
            RenderText("OK");
        }



    }

    public class HqlResult {
        public string Entity { get; set; }
        public IList<object> Items { get; set; }
        public string Log { get; set; }

        public IDictionary<string, HqlColumn> Columns { get; set; }
    }

}
