using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Controllers;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    public static class MvcExtension{
        public static bool Contains(this IController controller, string paramname){
            return -1 != Array.IndexOf(((Controller) controller).Params.AllKeys, paramname);
        }

        public static int ExtractInt(this IController controller, string paramname){
            var c = ((Controller) controller);
            return c.Contains(paramname) ? c.Params[paramname].toInt() : 0;
        }

        public static DateTime ExtractDate(this IController controller, string paramname){
            var c = ((Controller) controller);
            return c.Contains(paramname) ? c.Params[paramname].toDate() : DateExtensions.Begin;
        }

        public static T SetUser<T>(this T context, IPrincipal user) where T : IMvcContext{
            context.User = user;
            return context;
        }

        public static T AdminAlways<T>(this T context) where T : IMvcContext{
            context.Controller = new StubController();
            return context;
        }

        public static T Apply<T>(this T context, IMvcDescriptor descriptor) where T : IMvcContext{
            context.Area = descriptor.Area;
            context.Name = descriptor.Name;
            context.Action = descriptor.Action;
            return context;
        }

        public static T Attach<T>(this T context, object tag) where T : IMvcContext{
            context.Tag = tag;
            return context;
        }

        public static T SetParam<T>(this T context, string name, object value) where T : IMvcContext{
            context.ParamSource[name] = value == null ? "" : value.ToString();
            return context;
        }
    }

    public class MvcContext : IMvcContext{
        // TODO: Fagim хорошо бы небольшие краткие комментарии по членам класса. by Alert.
        private static readonly object sync = new object();
        [ThreadStatic] private static IMvcContext _current;
        private IDictionary<string, object> paramSource;

        #region IMvcContext Members

        public string Category { get; set; }
        public object Tag { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }

        public IPrincipal User { get; set; }
        public string Url { get; set; }

        public string ParametersString{
            get { return extractParametersString(ParamSource); }
        }

        public object Controller { get; set; }

        public IDictionary<string, object> ParamSource{
            get { return paramSource ?? (paramSource = new Dictionary<string, object>()); }
            set { paramSource = value; }
        }

        #endregion

        public static IMvcContext Current(){
            lock (sync){
                return Current(null);
            }
        }

        public static IMvcContext Current(IController controller){
            lock (sync){
                if (null == _current){
                    if (null != controller){
                        _current = Create((Controller) controller);
                    }
                }

                return _current;
            }
        }

        public static MvcContext Create(string area, string controllerName, string action, string url,
                                        IPrincipal principal,
                                        NameValueCollection parameters, object controller){
            var pars = new Dictionary<string, object>();
            if (parameters != null){
                foreach (var c in parameters.AllKeys){
                    pars[c] = parameters[c];
                }
            }
            return new MvcContext{
                                     Area = area ?? "",
                                     Name = controllerName ?? "",
                                     Action = action ?? "",
                                     Url = url ?? "",
                                     User = principal,
                                     ParamSource = pars,
                                     Controller = controller
                                 };
        }

        public static MvcContext Create(string area, string controllerName, string action, string url, string user,
                                        string[] roles, NameValueCollection parameters, object controller){
            return Create(area, controllerName, action, url, new GenericPrincipal(new GenericIdentity(user), roles),
                          parameters,
                          controller);
        }

        public static MvcContext Create(Controller controller){
            return Create(controller.Context, controller.ControllerContext, controller);
        }


        public static MvcContext Create(IEngineContext engine, IControllerContext context, object controller){
            var pars = new Dictionary<string, object>();
            foreach (var key in engine.Request.Form.AllKeys){
                if (null == key){
                    continue;
                }

                pars[key] = engine.Request.Form[key];
            }
            foreach (var key in engine.Request.QueryString.AllKeys){
                if (null == key){
                    continue;
                }

                pars[key] = engine.Request.QueryString[key];
            }
            return new MvcContext{
                                     Controller = controller,
                                     Area = context.AreaName,
                                     Name = context.Name,
                                     Action = context.Action,
                                     User = myapp.usr,
                                     Url = engine.Request.Url,
                                     ParamSource = pars
                                 };
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

        private static string extractParametersString(IDictionary<string, object> collection){
            if (collection.no()){
                return "";
            }
            var substrings = new List<string>();
            foreach (var c in collection){
                substrings.Add(c.Key + "=" + (c.Value ?? string.Empty));
            }
            return string.Join("|", substrings.ToArray());
        }

        public override string ToString(){
            var sb = new StringBuilder("[MVC Context]:");
            sb.Append("\r\n\tProperty [Category] is [").Append(Category).Append("]")
                .Append("\r\n\tProperty [Area] is [").Append(Area).Append("]")
                .Append("\r\n\tProperty[Action] is [").Append(Action).Append("]")
                .Append("\r\n\tProperty [ControllerName] is [").Append(Name).Append("]")
                //.Append("\r\n\r\n\tParametersString: ").Append(ParametersString)
                .Append("\r\n\tProperty [Url] is [").Append(Url).Append("]")
                .Append("\r\n\tProperty [User.Identity.Name] is [").Append(User.Identity.Name).Append("]");
            return sb.ToString();
        }
    }
}