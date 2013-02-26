using System.Linq;
using Castle.MonoRail.Framework;
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

namespace Comdiv.MVC.Controllers{
    public class ConversationFilter : Filter{
        protected override bool OnBeforeAction(IEngineContext context, IController controller,
                                               IControllerContext controllerContext){
          //  ioc.get<IRequestConversationHandler>().PrepareConversationOnBegin();
            ((Controller) controller).PropertyBag["conversation"] = myapp.conversation.Current;
            ((Controller) controller).PropertyBag["conversationKey"] = myapp.conversation.Current.Code;

            return true;
        }

        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
            var c = ((Controller) controller).PropertyBag;
            var converation = myapp.conversation.Current;
            if (null != converation){
                foreach (var pair in converation.Data){
                    if (!c.Contains(pair.Key)){
                        c[pair.Key] = pair.Value;
                    }
                }
            }
        }
    }
}