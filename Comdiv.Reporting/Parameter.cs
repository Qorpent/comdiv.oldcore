// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC;
using Comdiv.Persistence;
using Comdiv.Wiki;

namespace Comdiv.Reporting
{
    public delegate object ParameterEval(Parameter parameter );
    public delegate bool SavedParameterEvalBool(string code);
    public class Parameter
    {
        public override string ToString()
        {
            return string.Format(
                "{0}->{4}, temp:{1}, type:{2}, def:{3}",
                Code, !Static, Type, DefaultValue,RealTarget
                );
        }
        public string AltValue { get; set; }

        public string Name { get; set; }
        public string CustomPreparator { get; set; }
        public string CustomView { get; set; }
        public bool Acl { get; set; }
        public int AclIdx { get; set; }
        public string AclPrefix { get; set; }
        public bool Radio { get; set; }
        public string Group { get; set; }
        public bool IsCss { get; set; }
        public bool IsArea { get; set; }
        public bool IsHidden { get; set; }
        public string Tab { get; set; }

        public string InitialStringValue{
         
            set{
                this.DefaultValue = value;       
            }
        }

        public string BuilderClass
        {
            get;
            set;
        }

        private IParameterConfigurator _builder;
        protected IParameterConfigurator Builder
        {
            get
            {
                if (null == _builder)
                {
                    if (BuilderClass.hasContent())
                    {
                        _builder = BuilderClass.toType().create<IParameterConfigurator>();
                    }
                }
                return _builder;
            }
        }

        public void Prepare()
        {
            if (Builder != null)
            {
                Builder.Configure(this);
            }
        }



        public Parameter()
        {
            Static = true;
            this.RealType = typeof (string);
            ValueList = new List<Entity>();
        }
        public bool Static { get; set; }
        
        public Parameter set(object value)
        {
            this.RawValue = value;
            return this;
        }

    	private string _type;
        public string Type
        {
            get { return _type; }
            set {
            	_type = value;
                RealType = TypeAliases.Get(value);
            }
        }

        public string Target { get; set; }

        public Parameter ofint()
        {
            return settype<int>();
        }
        public Parameter ofnum()
        {
            return settype<decimal>();
        }
        public Parameter ofdate()
        {
            return settype<DateTime>();
        }
        public Parameter ofbool()
        {
            return settype<bool>();
        }

        public Parameter settype<T>()
        {
            return settype(typeof (T));
        }
        public Parameter settype(string pseudo) {
        	Type = pseudo;
            this.RealType = TypeAliases.Get(pseudo);
            return this;
        }
        public Parameter settarget(string target)
        {
            this.Target = target;
            return this;
        }
        public string RealTarget
        {
            get
            {
                return Target.hasContent() ? Target : (Code.hasContent() ? Code : "NONE");
            }
        }
        public Parameter settype(Type type)
        {
            this.RealType = type;
            return this;
        }
        public Parameter settemplate()
        {
            return settemplate(true);
        }
        public Parameter settemplate(bool template)
        {
            this.Static = !template;
            return this;
        }
        

        public object RawValue { get; set; }
        public object Value {
            get
            {
               var result = internalGetValue();
                return result ?? "";
            }
        }

        private object internalGetValue()
        {
            
            var val = RawValue ?? DefaultValue;

            if(this.Type=="bool" && this.AltValue.hasContent()){
                var yes = val.toBool();
                if(yes){
                    return AltValue;
                }else{
                    return "";
                }
            }

            if(null==val  && null!=DefaultValue)
            {
                val = DefaultValue;
            }
            if(typeof(string)==RealType)
            {
                return val.toStr();
            }
            if(val==null||!(val is string) || !((string)val).Contains(","))
            {
                return val.to(RealType);
            }
            return ((string)val).split(false, true, ',').Select(x => x.to(RealType)).ToArray();
        }

        public Parameter setdef(object value)
        {
            this.DefaultValue = value;
            return this;
        }

        public object DefaultValue { get; set; }

        private Type _realType;

        public Type RealType
        {
            get { return _realType ?? (_realType = typeof(string)); }
            set { _realType = value; }
        }

        public string Code
        {
            get; set;
        }

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        private string __listDefinition;
        public string ListDefinition
        {
            get { return __listDefinition; }
            set
            {
                __listDefinition = value;
                if (__listDefinition.StartsWith("$hql:"))
                {
                    var q = __listDefinition.Substring("$hql:".Length);
                    //var ex = Container.get<IHqlEvaluator>();
                    var set =  myapp.storage.GetDefault().Query(q).Cast<IEntityDataPattern>();
                        ValueList = (from x in set
                                     select new Entity(x.Code, x.Name)
                                    ).ToList();
                        ValueList.Add(new Entity("0", "NULL"));
                    
                }
                else
                {
                    ValueList = (from s in __listDefinition.Split('|')
                                 let def = s.Trim()
                                 let rec = def.Split(':')
                                 let uni = rec.Length == 1
                                 let code = uni ? def : rec[0]
                                 let name = uni ? def : rec[1]
                                 select new Entity { Code = code.Trim(), Name = name.Trim() }).ToList();
                }
            }
        }

        public IList<Entity> ValueList { get; set; }

        public int Idx { get; set; }

        public string Role { get; set; }

        public string Level { get; set; }

        public Parameter Clone(){
            var result =(Parameter) this.MemberwiseClone();
            return result;
        }


        public bool HasHelp() {
            return myapp.ioc.get<IWikiRepository>().Exists("rp/" + this.Code);
        }

        public bool Authorize(IPrincipal usr) {
            var param = this;
            if (param.Role.hasContent())
            {
                var roles = param.Role.split().ToList();
                var strict = false;
                var remove = true;
                if (roles.Contains("STRICT"))
                {
                    strict = true;
                    roles.Remove("STRICT");
                }
                if (roles.Contains("REMOVE"))
                {
                    remove = true;
                    roles.Remove("REMOVE");
                }

                bool isadmin = myapp.roles.IsInRole(usr, "ADMIN", true);
                bool isinrole = isadmin;
                if (!isinrole)
                {
                    foreach (var role in roles)
                    {
                        if (myapp.roles.IsInRole(usr, role, true))
                        {
                            isinrole = true;
                            break;
                        }
                    }
                }
                if ((strict && isinrole) || (!strict && !isinrole))
                {
                    if (remove) {
                        return false;
                    }
                    
                }
            }
            return true;
        }

    	public Parameter MarkAsFromLib(string libcode) {
    		this.FromLibrary = true;
    		this.Library = libcode;
    		return this;
    	}

    	public string Library { get; set; }

    	public bool FromLibrary { get; set; }
    }
    public interface IParameterConfigurator
    {
        void Configure(Parameter parameter);
    }
}