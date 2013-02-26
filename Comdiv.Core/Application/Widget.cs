using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Wiki;

namespace Comdiv.Application
{
    public class Widget
    {

      
        // общие настройки виджета

        /// <summary>
        /// Тип виджета
        /// </summary>
        public WidgetType Type { get; set; } //тип виджета
        public string Code { get; set; } //код виджета, уникальный
        public string Name { get; set; } // имя, заголовок виджета

        public bool HasDoc() {
            return myapp.ioc.get<IWikiRepository>().Exists(this.GetDocCode());
        }
        public string GetDocCode() {
            Guid g;
            if(Guid.TryParse(Code,out g)) {
                return "";
            }
            return ("-"+Extension+"/widget-"+Code).Replace("//","/");
        }

        public string _SrcBxlFile { get; set; }
        public string Image { get; set; }
        public string OnClick { get; set; }
        public string Comment { get; set; }
        public string Tab { get; set; }

        public string Quick { get; set; }

        public string SpecClass { get; set; }

        // настройки контейнера
        public bool ShowTitle { get; set; } //признак показа/скрытия заголовка виджета
        public string Classes { get; set; } //дополнительные CSS классы контейнера
        public string Style { get; set; } //дополнительные стили контейнера
        public string Position { get; set; } //строковое описание позиции
        public WidgetPosition StandardPosition {
            get { 
                WidgetPosition result;
                Enum.TryParse(Position, true, out result);
                return result;
            }
            set { this.Position = value.ToString(); }
        }
        public int Idx { get; set; }


        //настройки для статичного виджета
        public string Content { get; set; }

        //настройки для вызова вида
        public string View { get; set; }
        private IDictionary<string,object > viewParameters = new Dictionary<string, object>();
        public IDictionary<string, object> ViewParameters
        {
            get { return viewParameters; }
        }
        
        //настройки для AJAX

        public string DirectUrl { get; set; } //прямой URL (c схемой или относительный) для Ajax
        public string Controller { get; set; } //контроллер для Ajax
        public string Action { get; set; } //действие для Ajax
        private IDictionary<string, string> _ajaxParameters = new Dictionary<string, string>();
        public string JsAjaxParameters { get; set; } //JSON строка, в значениях которой строки обращения к странице, для вызова через EVAL,  добавятся в URL после загрузки страницы
        public IDictionary<string, string> AjaxParameters {
            get { return _ajaxParameters; }
        }


        private string _evaluatedurl;
        public string EvaluateAjaxUrl() {
            lock (this) {
                if (_evaluatedurl.noContent()) {
                    if (DirectUrl.hasContent()) return DirectUrl;
                    var parameters = "";
                    foreach (var parameter in AjaxParameters) {
                        parameters += "&" + parameter.Key + "=" + parameter.Value;
                    }
                    if (parameters.hasContent()) {
                        parameters = "?" + Uri.EscapeUriString(parameters);
                    }
                    _evaluatedurl = "/" + Controller + "/" + this.Action + ".rails" + parameters;
                    
                }
                return _evaluatedurl;
            }
        }


        //настройки для Servlet
        public string ClassName { get; set; }
        public IWidgetServlet CreateServlet() {
            return System.Type.GetType(ClassName).create<IWidgetServlet>();
        }


        //фильтры
        public string[] Roles { get; set; }
        public string UrlRegex { get; set; }

        public void Read(XElement element) {
            this.Code = element.attr("code");
            this.Name = element.attr("name");
            this._SrcBxlFile = element.attr("_srcbxlfile");
            this.Extension =Path.GetFileName( Path.GetDirectoryName(_SrcBxlFile));

            //container
            this.Position = element.attr("position").ToLower();
            this.Idx = element.attr("idx").toInt();
            this.Classes = element.attr("classes");
            this.Style = element.attr("style");
            this.ShowTitle = element.attr("showtitle").toBool();
            this.CustomView = element.attr("customview");
            this.SpecClass = element.attr("specclass", "");

            //button
            this.Image = element.attr("image");
            this.OnClick = element.attr("onclick");
            this.Comment = element.attr("comment");
            this.Tab = element.attr("tab");
            this.Quick = element.attr("quick");
            //static))))))))))))
            this.Content = element.Value;

            //view
            this.View = element.attr("view");
            foreach (var vp in element.Elements("viewparam")) {
                this.ViewParameters[vp.attr("code")] = vp.Value;
            }

            //servlet
            this.ClassName = element.attr("classname");

            //ajax
            this.DirectUrl = element.attr("url");
            this.Controller = element.attr("controller");
            this.Action = element.attr("action");

            var jsp = element.Element("json");
            if(jsp!=null) {
                this.JsAjaxParameters = jsp.Value;
            }

            foreach (var p in element.Elements("ajaxparam")) {
                this.AjaxParameters[p.attr("code")] = p.Value;
            }

            //filters
            this.Roles = element.attr("roles").split().ToArray();
            this.UrlRegex = element.attr("urlregex");

            
            WidgetType type;
            Enum.TryParse(element.attr("type"), true, out type);
            this.Type = type;
            if(element.attr("content")=="1") {
                this.Type = WidgetType.Static;
            }
            if (element.attr("view") == "1")
            {
                this.Type = WidgetType.View;
            }
            if (element.attr("ajax") == "1")
            {
                this.Type = WidgetType.Ajax;
            }
            if (element.Name.LocalName != "widget") {
                if (element.Name.LocalName == "tab") {
                    this.Type = WidgetType.Tab;
                    if(this.Position.noContent()) {
                        this.StandardPosition = WidgetPosition.Toolbar;
                    }
                }
                else if (element.Name.LocalName == "button") {
                    this.Type = WidgetType.Button;
                    if (this.Position.noContent())
                    {
                        this.StandardPosition = WidgetPosition.Toolbar;
                    }
                    if(this.OnClick.noContent()) {
                        this.Content = "";
                        this.OnClick = element.Value;
                    }
                    if(this.Tab.noContent()) {
                        this.Tab = this.Code.Split('_')[0];
                    }
                } else if (element.Name.LocalName == "resources") {
                    this.Code = Guid.NewGuid().ToString();
                    this.Type = WidgetType.Static;
                    this.StandardPosition = WidgetPosition.HtmlHeader;
                    var content = "";
                    var name = element.Value;
                    var basename = name;
                    if(!name.Contains("/")) {
                        name += "/default";
                        
                    }
                    
                    var app = HttpContext.Current.Request.ApplicationPath;
                    var js = myapp.files.ResolveAsLocal("extensions/" + name + ".js");
                    if(js!=null) {
                        content += string.Format("<script type='text/javascript' src='{0}/{1}'></script>\r\n", app, js);
                    }
                    var css = myapp.files.ResolveAsLocal("extensions/" + name + ".css");
                    if(css!=null) {
                        content += string.Format("<link rel='stylesheet' type='text/css' href='{0}/{1}' />\r\n", app, css);
                    }

                    if (basename != name) {
                        name = basename + "/" + basename; //resolves js with same name as extensions


                        js = myapp.files.ResolveAsLocal("extensions/" + name + ".js");
                        if (js != null) {
                            content += string.Format("<script type='text/javascript' src='{0}/{1}'></script>\r\n", app,
                                                     js);
                        }
                        css = myapp.files.ResolveAsLocal("extensions/" + name + ".css");
                        if (css != null) {
                            content += string.Format("<link rel='stylesheet' type='text/css' href='{0}/{1}' />\r\n", app,
                                                     css);

                        }
                    }
                    this.Content = content;
                }
            }

            if(this.Type==WidgetType.Button && this.Tab.noContent()) {
                this.Tab = "Прочее";
            }

            if(this.Type==WidgetType.None) {
                this.guessType();
            }
        }

        public string Extension { get; set; }

        public string CustomView { get; set; }

        private void guessType() {
            if(View.hasContent()) {
                this.Type = WidgetType.View;
                return;
            }
            if(DirectUrl.hasContent() || Controller.hasContent()) {
                this.Type = WidgetType.Ajax;
                return;
            }
            if(ClassName.hasContent()) {
                this.Type = WidgetType.Servlet;
                return;
            }
            this.Type = WidgetType.Static;

        }

        public string GetImage(int size) {
            if (this.Image.noContent()) return "";
            var path = this.Image;
            if(!path.StartsWith("/")) {
                path = "extensions/" + path;
            }
            path = path + size + ".png";
            var img = myapp.files.ResolveAsLocal(path);
            if (img == null) return "";
            return HttpContext.Current.Request.ApplicationPath + "/" + img;
        }
    }
}
