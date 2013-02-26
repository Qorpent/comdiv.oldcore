using Castle.MonoRail.Framework;
using Comdiv.Extensions;

namespace Comdiv.Controllers{

    public class EchoController : BaseController{
        public void get(string message) {
            if(message.noContent()) {
                message = "ok";
            }
            RenderText(message);
        }
    }

    
}