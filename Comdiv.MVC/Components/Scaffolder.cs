#region

using System;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensibility;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Scaffolding;
using Comdiv.MVC.Scaffolding;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

#endregion

namespace Comdiv.Web{

    #region

    #endregion

    //TODO: SCAFF Ќужны ссылки на CustomViews из метаданных, дл€ полной делегации отрисовки содержимого пол€
    //Bug : SCAFF группировка отказываетс€ работать если на вход подать Null

    #region

    #endregion

    public class Scaffolder : ViewComponent{
        private Interpreter interpreter;

        public Type Type{
            get { return ((Type) ComponentParams["type"]); }
        }

        public string Alias{
            get { return ((string) Context.ContextVars["alias"]); }
        }

        public string NameRoot{
            get { return Type.Name; }
        }

        public string Id{
            get { return ComponentParams["id"] as string; }
            set { ComponentParams["id"] = value; }
        }

        public ClassMetadata Metadata{
            get { return ComponentParams["metadata"] as ClassMetadata; }
            set { ComponentParams["metadata"] = value; }
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

        public override void Initialize(){
            try{
                logger.get("comdiv.sys").debug(() => "initialize interpreter");
                interpreter = new Interpreter();
            }
            catch (Exception ex){
                logger.get("comdiv.sys").Error("cannot launch interpreter", ex);
                return;
            }
            Console.WriteLine("in initialize");
            base.Initialize();
            var md = Metadata = Metadata ?? Container.get<IMetadataHelper>().GetMeta(Type, Alias);
            SetDefault("id", "scaffolder");

            ComponentParams["boo"] = interpreter;


            ComponentParams["nameRoot"] = NameRoot;
            SetDefault("path",
                       string.Format("database/objects/{0}", NameRoot));
            SetDefault("hiddens", new object[]{});
            SetDefault("groupby", md.GroupBy);
            SetDefault("commands", md.Commands);

            SetDefault("ioc.getid", null);
            SetDefault("mode", "table");
            SetDefault("targetObject", null);
            SetDefault("propertyRender", "writeoutproperty");
            if (md.CustomRender.hasContent()){
                ComponentParams["propertyRender"] = md.CustomRender;
            }
            ComponentParams["props"] = md.GetVisibleProperties();

            ComponentParams["checked"] = new FunctionHandler<bool, string>(x => x ? " checked='checked' " : string.Empty);
        }

        public void SetDefault(string key, object value){
            if (!ComponentParams.Contains(key) || null == ComponentParams[key]){
                ComponentParams[key] = value;
            }
        }

        public override void Render(){
            base.Render();
            interpreter.Dispose();
        }
    }
}