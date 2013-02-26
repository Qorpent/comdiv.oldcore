using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Inversion;
using Qorpent;
using Qorpent.Bxl;
using Qorpent.Dsl.XmlInclude;
using Qorpent.Events;
using Qorpent.IO;
using Qorpent.IoC;
using Qorpent.Log;
using Qorpent.Mvc;
using Qorpent.Mvc.Binding;
using Qorpent.Mvc.QView;
using Enumerable = System.Linq.Enumerable;
using IMvcContext = Qorpent.Mvc.IMvcContext;
using InversionExtensions = Comdiv.Inversion.InversionExtensions;

namespace Comdiv.Framework.Quick {
	public class OldAssoiTypeLocator:ITypeResolver {
		public Type Get<BASETYPE>(string name = null, bool throwexception = false) {
			try {
				if (null == name) {
					return myapp.ioc.get<BASETYPE>().GetType();
				}
				else {
					return myapp.ioc.get<BASETYPE>(name).GetType();
				}
			}catch(Exception ex) {
				if (throwexception) throw;
				return null;
			}

		}

		public Type[] GetAll<BASETYPE>(bool throwexception = false)
		{
			try {
				return Enumerable.ToArray<Type>(myapp.ioc.all<BASETYPE>().Select(x => x.GetType()));
			}catch(Exception ex) {
				if (throwexception) throw;
				return Type.EmptyTypes;
			}
		}

		/// <summary>
		/// Приоритет при разрешении типов
		/// </summary>
		public int Idx { get; set; }

		/// <summary>
		/// 	Возвращает сервис указанного типа. (обобщенный)
		/// </summary>
		/// <typeparam name="T"> Тип сервиса </typeparam>
		/// <param name="name"> опциональное имя компонента, если указано - поиск будет производиться только среди с компонентов с указаным именем </param>
		/// <param name="ctorArguments"> Параметры для вызова конструктора, если не указано - будет использован конструктор по умолчанию. </param>
		/// <returns> Сервис указанного типа или <c>null</c> , если подходящий компонент не обнаружен </returns>
		/// <exception cref="ContainerException">При ошибках в конструировании объектов</exception>
		/// <remarks>
		/// 	<invariant>Данный метод является простой оболочкой над
		/// 		<see cref="ITypeResolver.Get" />
		/// 		, см. документацию данного метода для более подробной информации</invariant>
		/// </remarks>
		public T Get<T>(string name = null, params object[] ctorArguments) where T : class {
			try {
				if (!string.IsNullOrWhiteSpace(name)) {
					return myapp.ioc.get<T>(name);
				}
				else {
					return myapp.ioc.get<T>();
				}
			}catch {
				return null;
			}
		}

		///<summary>
		///	Возвращает сервис указанного типа. (прямое указание типа)
		///</summary>
		///<param name="type"> Тип сервиса </param>
		///<param name="ctorArguments"> Параметры для вызова конструктора, если не указано - будет использован конструктор по умолчанию. </param>
		///<param name="name"> опциональное имя компонента, если указано - поиск будет производиться только среди с компонентов с указаным именем </param>
		///<returns> Сервис указанного типа или <c>null</c> , если подходящий компонент не обнаружен </returns>
		///<exception cref="ContainerException">При ошибках в конструировании объектов</exception>
		///<remarks>
		///	При вызове метода, контейнер осуществляет поиск в своей конфигурации и находит компонент, максимаольно соответствующий условию на класс сервиса 
		///	и имя. Если компонент не находится, то возвращается null. <note>При включении параметра
		///		                                                          <see cref="IContainer.ThrowErrorOnNotExistedComponent" />
		///		                                                          =
		///		                                                          <c>true</c>
		///		                                                          ,
		///		                                                          вместо возврата null будет осуществлятся генерация исключения
		///		                                                          <see cref="ContainerException" />
		///		                                                          .</note>
		///	<invariant>
		///		<h3>Разрешение компонентов</h3>
		///		<list type="bullet">
		///			<item>В качестве класса сервиса можно указывать как напрямую интерфейс требуемого сервиса,  так и базовый класс/интерфейс</item>
		///			<item>Поиск сервиса ведется среди зарегистрированных (см.
		///				<see cref="IComponentRegistry.Register" />
		///				,
		///				<see cref="ContainerFactory" />
		///				,
		///				<see cref="IComponentSource.GetComponents" />
		///				) компонентов.</item>
		///			<item>Если имя не указано то поиск ведется
		///				<strong>игнорируя имя</strong>
		///				компонента (а не среди компонентов с именем "" или null)</item>
		///			<item>Так как за одним типом/именем может быть закреплено несколько компонентов, то производится отбор наиболее релевантного:
		///				<list type="number">
		///					<item>Если один из компонентов явно эквивалентен по типу сервиса запрашиваемому, то он имеет приоритет перед теми, которые сконфигурированы через унаследованный интерфейс
		///						<note>На первый взгляд это кажется нелогичным, однако на деле типы сервисов являются явным ключем компонента и запроса, тогда
		///							как поиск по дополнительным сервисам - это дополнительая "фича" контейнера. Соответственно под разрешением типов следует понимать
		///							не "разрешение самого последнего в иерархии", а как "разрешение сначала точного, а затем лишь частично совпадаюшего ключа".
		///							Это помимо всего прочего избавляет от проблем при которых интерфейс наследуется от интерфейса сервиса для решения внутренних задач совместимости,
		///							но при этом он не должен использоваться как комонент для обслуживания базовго сервиса как такового</note>
		///					</item>
		///					<item>Если у компонента явно указан приоритет (см.
		///						<see cref="IComponentDefinition.Priority" />
		///						,
		///						<see cref="ContainerAttribute.Priority" />
		///						,
		///						<see cref="ContainerAttribute.Priority" />
		///						),
		///						то используется компонент с
		///						<strong>большим</strong>
		///						приоритетом</item>
		///					<item>При равенстве или отсутствии приоритетов, используется компонент зарегистрированные
		///						<strong>последним</strong>
		///					</item>
		///				</list>
		///			</item>
		///		</list>
		///		<h3>Возвращаемое значение и циклы жизни</h3>
		///		<para> Циклы жизни настраиваются в составе компонентов ( <see cref="IComponentDefinition.Lifestyle" /> ). И документацию по ним смотрите в <see
		///	 cref="Lifestyle" /> </para>
		///		<para> Циклы жизни влияют на поведение контейнера и метода Get. </para>
		///		<list type="number">
		///			<listheader>Порядок работы контейнера</listheader>
		///			<item>Поиск компонента (описано выше), вызов расширений определения компонента</item>
		///			<item>Идентификация или создание экземпляра (зависит от цикла жизни компонента)</item>
		///			<item>Если это новый экземпляр, то дополнительно производится впрыск зависимостей</item>
		///			<item>Активация компонента (формируется статистика вызова, вызываются расширения активации)</item>
		///			<item>Возврат экземпляра вызываемому коду</item>
		///		</list>
		///		<h3>Создание экземпляров</h3>
		///		Если цикл жизни подразумевает создание нового экземпляра, то его создание производится по следующему алгоритму:
		///		<list type="number">
		///			<item>Вызывается
		///				<see
		///					cref="Activator.CreateInstance(System.Type,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)" />
		///				,
		///				с поддержкой приватных конструкторов и передачей
		///				<paramref name="ctorArguments" />
		///			</item>
		///			<item>Производися впрыск параметров компонента
		///				<see cref="IComponentDefinition.Parameters" />
		///				, впрыск производится в защищенном режиме 
		///				(если свойство не найдено или тип не соответствует, то ничего не производится)</item>
		///			<item>Производится впрыск сервисов на основе
		///				<see cref="ContainerComponentAttribute" />
		///			</item>
		///			<item>Если экземпляр - наследник
		///				<see cref="IContainerBound" />
		///				(см. также
		///				<see cref="ServiceBase" />
		///				), то производится
		///				вызова
		///				<see cref="IContainerBound.SetContainerContext" />
		///				, для выполнения пользовательской настройки на контейнер</item>
		///		</list>
		///	</invariant>
		///</remarks>
		public object Get(Type type, string name = null, params object[] ctorArguments) {
			if(-1!=Array.IndexOf(_exludetypes,type)) {
				return null; // qorpent-only services
			}
			try {
				if (!string.IsNullOrWhiteSpace(name)) {
					return myapp.ioc.get(name);
				}
				else {
					return myapp.ioc.get(type);
				}
			}catch {
				return null;
			}
		}
		//интерфейсы - исключения, никогда не перекрываются из АССОИ
		private readonly Type[] _exludetypes = new[]
			{
				typeof(ILogManager),
				typeof(IEventManager),
				typeof(IMvcContext),
				typeof(IMvcFactory),
				typeof(IMvcHandler),
				typeof(IActionBinder),
				typeof(IQViewMonitor),
				typeof(IFileNameResolver),
				typeof(IXmlIncludeProcessor),
				typeof(IBxlParser)
			};

		///<summary>
		///	Возвращает все объекты указаннго типа (обобщенииый)
		///</summary>
		///<typeparam name="T"> тип сервиса </typeparam>
		///<param name="ctorArguments"> Параметры для вызова конструктора, если не указано - будет использован конструктор по умолчанию. </param>
		///<param name="name"> опциональное имя компонента, если указано - поиск будет производиться только среди с компонентов с указаным именем </param>
		///<returns> Все экземпляры указанного сервиса </returns>
		///<remarks>
		///	<invariant>Метод All применим только для компонентов с циклом жизни
		///		<see cref="Lifestyle.Transient" />
		///		и
		///		<see cref="Lifestyle.Extension" />
		///		.
		///		<note>Не пытайтесь таким образом получить все экземпляры сервисов с другим циклом жизни</note>
		///		<para> Остальные особенности поведения контейнера при поиске и создании компонентов описаны в документации к <see
		///	 cref="ITypeResolver.Get" /> </para>
		///	</invariant>
		///</remarks>
		public IEnumerable<T> All<T>(string name = null, params object[] ctorArguments) where T : class {
			return myapp.ioc.all<T>();
		}

		/// <summary>
		/// 	Возвращает все объекты указаннго типа (прямое указание типа)
		/// </summary>
		/// <param name="ctorArguments"> Параметры для вызова конструктора, если не указано - будет использован конструктор по умолчанию. </param>
		/// <param name="type"> тип сервиса </param>
		/// <param name="name"> опциональное имя компонента, если указано - поиск будет производиться только среди с компонентов с указаным именем </param>
		/// <returns> Все экземпляры указанного сервиса </returns>
		/// <remarks>
		/// 	<invariant>Метод All применим только для компонентов с циклом жизни
		/// 		<see cref="Lifestyle.Transient" />
		/// 		и
		/// 		<see cref="Lifestyle.Extension" />
		/// 		.
		/// 		<note>Не пытайтесь таким образом получить все экземпляры сервисов с другим циклом жизни</note>
		/// 	</invariant>
		/// </remarks>
		public IEnumerable All(Type type, string name = null, params object[] ctorArguments) {
			return myapp.ioc.ResolveAll(type);
		}

		/// <summary>
		/// 	Возвращает объект контейнеру
		/// </summary>
		/// <param name="obj"> Объект, ранее созданнй контейнером </param>
		/// <remarks>
		/// 	<invariant>Метод позволяет контейнеру высвобождать собственные ресурсы и вызывает
		/// 		<see cref="IContainerBound.OnContainerRelease" />
		/// 		метод,
		/// 		работает это только для
		/// 		<see cref="Lifestyle.Pooled" />
		/// 		и
		/// 		<see cref="Lifestyle.PerThread" />
		/// 		, для остальных данный метод игнорируется</invariant>
		/// </remarks>
		public void Release(object obj) {
		}
	}
}