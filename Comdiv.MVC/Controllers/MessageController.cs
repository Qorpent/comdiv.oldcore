//using System;

using System;
using System.Security;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Messaging;
using Comdiv.Security;

namespace Comdiv.MVC.Controllers
{





    public delegate bool FuncMessageToBool (IMessage message);
    public delegate IUserRecord FuncMessageToUsrRecord(IMessage message);
    [Public]
    public class MessageController:BaseController
    {
        public IMessageRepository Repository { get; set; }

        private IUserRecordRepository _usrRecordResolver;
        public IUserRecordRepository UsrRecordResolver
        {
            get{
                if(_usrRecordResolver==null){
                    _usrRecordResolver = Container.get<IUserRecordRepository>() ?? new UserRecordRepository();
                    
                }
                return _usrRecordResolver;
            }
            set { _usrRecordResolver = value; }
        }

        public override void Contextualize(Castle.MonoRail.Framework.IEngineContext engineContext, Castle.MonoRail.Framework.IControllerContext context)
        {
            base.Contextualize(engineContext, context);
            PropertyBag["repo"] = Repository;
            if(this.Action.ToLower()!="get"){
                Conversation.canbecommited();
            }
            FuncMessageToBool isread = m => Repository.IsReaded(m, myapp.usrName);
            FuncMessageToBool isarc = m => Repository.IsArchived(m, myapp.usrName);
            FuncMessageToBool isreadbt = m => Repository.IsReaded(m, m.Target);
            FuncMessageToUsrRecord target = m => UsrRecordResolver.Get(m.Target);
            FuncMessageToUsrRecord sender = m => UsrRecordResolver.Get(m.Sender);
            PropertyBag["isread"] = isread;
            PropertyBag["isarc"] = isarc;
            PropertyBag["isreadbt"] = isreadbt;
            PropertyBag["target"] = target;
            PropertyBag["sender"] = sender;
            PropertyBag["usr"] = myapp.usrName;
            PropertyBag["usrresolver"] = this.UsrRecordResolver;
        }

        public void show(string code, string viewname){
            
            var message = Repository.Load(code);
            if(isValidAccess(message)){
                PropertyBag["message"] = message;
                if(viewname.hasContent()){
                    SelectedViewName = "/message/customsingle/" + viewname;
                }
            }else{
                myapp.errors.RegisterError(
                    new SecurityException("Вы не имеете прав на просмотр сообщения с кодом " + code));
                CancelView();
            }
        }

        private bool isValidAccess(IMessage message){
            if(myapp.roles.IsAdmin()){
                return true;
            }
            //broadcast
            if(message.Target=="") return true;
            if(message.Sender.ToLower()==myapp.usrName.ToLower()) return true;
            if (message.Target.ToLower() == myapp.usrName.ToLower()) return true;
            if (message.Target == UsrRecordResolver.Get(myapp.usrName).Domain) return true;
            return false;
        }

        public void get(string viewname){
            if(viewname.hasContent()){
                SelectedViewName = "/message/custom/"+viewname;
            }
            
        }

        public void read(string code){
            Repository.MarkRead(code, myapp.usrName, true);
            RenderText("message marked as readed");
        }
        public void unread(string code){
            Repository.MarkRead(code, myapp.usrName, false);
            RenderText("message marked as not readed");
        }
        public void archive(string code){
            read(code);
            Repository.MarkArchive(code, myapp.usrName, true);
            RenderText("message marked as archived");
        }
        public void dearchive(string code){
            Repository.MarkArchive(code, myapp.usrName, false);
            RenderText("message marked as not archived");
        }
        public void answer(string code, string text){
            Repository.Answer(code, myapp.usrName, text);
            RenderText("message successfully answered");
        }

		[Role("PUBLISHER")]
        public void activate(string code, bool activate){
            Repository.Activate(code,activate);
            RenderText("Статус по активации успешно изменен");
        }

        public void send( string to, string type, string text, bool ret,string returl, string  tag){
            fullsend(myapp.usrName,to,type ?? MessageTypes.Support,"",text.cleanHtml(),true,ret,returl, tag);
        }
        public void answerform(string code)
        {
            PropertyBag["code"] = code;
        }
        public void sendform(string viewname){
            if (viewname.hasContent())
            {
                SelectedViewName = "/message/customsend/" + viewname;
            } 
        }
        [Role("PUBLISHER")]
        public void fullsendform(string code,string viewname,bool richedit){
            if(code.hasContent()){
                PropertyBag["message"] = this.Repository.Load(code);
            }
            if (viewname.hasContent())
            {
                SelectedViewName = "/message/customfullsend/" + viewname;
            }
            if (richedit){
                PropertyBag["advancedScripts"] = new[]{
                    "yui/utilities/utilities",
                    "yui/container/container",
                    "yui/menu/menu",
                    "yui/button/button",
                    "yui/editor/editor-beta",
                    "editor"};
                PropertyBag["advancedCss"] = new[]{
                    "scripts/yui/fonts/fonts-min",
                    "scripts/yui/container/assets/skins/sam/container",
                    "scripts/yui/button/assets/skins/sam/button",
                    "scripts/yui/editor/assets/skins/sam/editor",
                    "scripts/yui/yahoo-dom-event/yahoo-dom-event",
                    "scripts/yui/menu/assets/skins/sam/menu"};
            }


        }
		[Role("PUBLISHER")]
        public void fullsend(string from, string to, string type, string code, string text, bool canbearchived,bool ret, string returl, string  tag)
        {
            if (from.noContent()) from = myapp.usrName;
            if (type.noContent()) type = MessageTypes.Default;
            if (code.noContent()) code = from + "--" + to + "-" + DateTime.Now.ToString("yyyyMMddhhmmss");
            var m = Repository.Load(code) ?? Repository.New();
            m.Sender = from;
            m.Target = to;
            m.Type = type;
            m.Code = code;
            m.Text = text;
            m.Tag = tag;
            m.CanBeArchived = canbearchived;
            m.Active = true;
            Repository.Save(m);
            if (ret)
            {
                if (returl.hasContent())
                {
                    RedirectToUrl(returl);
                }
                else{
                    RedirectToReferrer();
                }
            }
            else{
                RenderText("message successfully sended");
            }
        }
    }
}
