namespace Comdiv.Application {
    /// <summary>
    /// Типы виджетов
    /// </summary>
    public enum WidgetType {
        /// <summary>
        /// Неопределенный тип
        /// </summary>
        None,
        /// <summary>
        /// Контент прямо указан
        /// </summary>
        Static, 
        /// <summary>
        /// Указано имя вида для виджета
        /// </summary>
        View,
        /// <summary>
        /// указан запрос и параметры, который выполняется в Ajax режиме после загрузки страницы
        /// </summary>
        Ajax, 
        /// <summary>
        /// Кнопка
        /// </summary>
        Button,
        /// <summary>
        /// Раздел тулбара
        /// </summary>
        Tab,
        /// <summary>
        /// указан класс со специальными интерфейсом, ему передается обратная ссылка на вид и на контроллер
        /// </summary>
        Servlet,
        
    }
}