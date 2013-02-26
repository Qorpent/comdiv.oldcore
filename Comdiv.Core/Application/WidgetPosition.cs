namespace Comdiv.Application {
    /// <summary>
    /// Стандартные позиции виджетов
    /// </summary>
    public enum WidgetPosition {
        
        
        /// <summary>
        /// Позиция нестандартная
        /// </summary>
        None,
        /// <summary>
        /// Панель управления
        /// </summary>
        Toolbar ,

        /// <summary>
        /// Заголовок HTML
        /// </summary>
        HtmlHeader ,
        /// <summary>
        /// Верхний левый фрейм
        /// </summary>
        HeaderLeft,
        /// <summary>
        /// Верхний центральный фрейм
        /// </summary>
        HeaderCenter,
        /// <summary>
        /// Верхний правый фрейм
        /// </summary>
        HeaderRight,
        /// <summary>
        /// Левый фрейм основной части
        /// </summary>
        BodyLeft,
        /// <summary>
        /// Верхний фрейм фрейм основной части
        /// </summary>
        BodyHeader,
        /// <summary>
        /// НИжний фрейм основной части
        /// </summary>
        BodyFooter,
        /// <summary>
        /// Правый фрейм основной части
        /// </summary>
        BodyRight,
        /// <summary>
        /// Левый нижний фрейм
        /// </summary>
        FooterLeft,
        /// <summary>
        /// Центральный нижний фрейм
        /// </summary>
        FooterCenter,
        /// <summary>
        /// Правый нижний фрейм
        /// </summary>
        FooterRight,

        StatusBar,

        Title,
    }
}