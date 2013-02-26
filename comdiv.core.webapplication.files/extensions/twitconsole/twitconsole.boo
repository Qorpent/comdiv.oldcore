import System.IO
import System
import System.Collections.Generic
import Comdiv.Model from Comdiv.Core
import System.Xml.Linq
import System.Linq.Enumerable from System.Core as Enumerable
import System.Linq.Enumerable
import Comdiv.Extensions.StringExtensions from Comdiv.Core
import Comdiv.Application.myapp from Comdiv.Core as myapp
import System.IO
[Controller]
class TwitConsole :
	private def getfolder() as string:
		dir = myapp.files.Resolve('~/usr/twits',false);
		Directory.CreateDirectory(dir)
		return dir

	def send(text as string) :
		filename = Path.Combine(getfolder(), myapp.usrName.Replace("\\","_")+"@"+DateTime.Now.ToString('yyyy-MM-dd HH--mm--ss')+".twit")
		File.WriteAllText(filename, text)
		RenderText("Сообщение отправлено")
	
	def history() :
		files = Directory.GetFiles(getfolder(),"*.twit")
		result = List[of Entity]()
		for f in files :
			e = Entity()
			nameparts = Path.GetFileNameWithoutExtension(f).Split("@"[0])
			e.Code = nameparts[0].Replace('_',"\\")
			e.Name = nameparts[1].Replace('--',':')
			e.Comment = File.ReadAllText(f)
			result.Add(e)
		PropertyBag["result"] = result.OrderByDescending({x as Entity|x.Name}).ToArray()

	def check() :
		lasttimestr = ReadFromProfile('twitconsolelasttime')
		if not lasttimestr :
			lasttimestr = '1900-01-01 00:00:00'
		time = DateTime.Parse(lasttimestr)
		WriteToProfile('twitconsolelasttime', DateTime.Now.ToString('yyyy-MM-dd HH:mm:ss'))
		files = Directory.GetFiles(getfolder(),"*.twit")
		result = List[of Entity]()
		for f in files :
			if FileInfo(f).LastWriteTime > time :
				e = Entity()
				nameparts = Path.GetFileNameWithoutExtension(f).Split("@"[0])
				e.Code = nameparts[0].Replace('_',"\\")
				e.Name = nameparts[1].Replace('--',':')
				e.Comment = File.ReadAllText(f)
				result.Add(e)
		PropertyBag["result"] = result.OrderByDescending({x as Entity|x.Name}).ToArray()