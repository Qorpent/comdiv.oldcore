import System.IO
import System.Xml.Linq
import System.Linq.Enumerable from System.Core as Enumerable
import Comdiv.Extensions.StringExtensions from Comdiv.Core
import Comdiv.Xml.Smart from Comdiv.Core
import Comdiv.Booxml
import Comdiv.Themas from Comdiv.Core
import Comdiv.Authorization
[Controller]
class BxlTester :

	def index() :
		pass
		
	def files() :
		PropertyBag["files"] = GetProfileFiles(getfile("*"),"{NAME}")
		
	def compile(code as string, imports as string, name as string, method as string, xpath as string) :
		try :
			fullcode = ""
			importfiles = imports.split()
			for f in GetProfileFiles(getfile("*"),null) :
				fn = Path.GetFileNameWithoutExtension(f)
				if(name!=fn and importfiles.Contains(fn)):
					fullcode += File.ReadAllText(f)
					fullcode += "\r\n"
			fullcode += code
			xml as XElement = BooxmlParser().Parse(fullcode)
			if method == "smart" or method == "themas" :
				xml = SmartXml(xml).Process().Element
				if method == "themas" :
					cfgs = ThemaConfigurationXmlProcessor().Process(xml)
					xml = XElement("root")
					for cfg in cfgs :
						xml.Add(cfg)
			xml = SmartXml.CleanAttributes(xml,"_line","_file","processed")
			xml = SmartXml.FilterByXPath(xml, xpath)
			if (method == "parse") :
				RenderText(xml.ToString())
			else :
				RenderText(BooxmlGenerator().Generate(xml))
		except e:
			Context.Response.StatusCode = 500
			RenderText(e.Message)
	
	def load(name as string) :
		RenderText( ReadFromProfile(getfile(name)) )
	
	def save(name as string, code as string) :
		WriteToProfile(getfile(name), code)
		RenderText("OK")
	
	def delete (name as string) :
		DeleteProfileFiles(getfile(name))
		RenderText("DELETED")
		
	private def getfile(name as string) :
		return "bxltester/${name}.bxl"
		
		
	