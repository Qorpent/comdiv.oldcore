using System;
using System.IO;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Controllers {


    /// <summary>
    /// Позволяет сохранять данные в локальном профиле и считывать их оттуда посредством AJAX
    /// </summary>
    public class ProfileController:BaseController {
        public void save(string code,string content) {
            WriteToProfile(code, content);
            RenderText("OK");
        }
        public void load(string code) {
            RenderText(ReadFromProfile(code));
        }
    }
}