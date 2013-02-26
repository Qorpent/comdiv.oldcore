using System;
using System.Globalization;
using Comdiv.Extensions;

namespace Comdiv.Useful{
    public static class AdvancedDateExtensions{
        public static string toCacheString(this DateTime dateTime)
        {
            return dateTime.isNull() ? "" : dateTime.ToShortDateString();
        }


        /// <summary>
        /// ��������� ����, ������������ ��������
        /// </summary>
        /// <param name="source">�������� ����</param>
        /// <param name="resolver">��������</param>
        /// <returns></returns>
        public static DateTime resolveDateTime(this DateTime source, string resolver)
        {

            //���� ����� ������� ���� - ����� ��
            DateTime directDate = new DateTime();
            var fullDefinition = DateTime.TryParseExact(resolver, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out directDate);
            if (fullDefinition)
            {
                return directDate;
            }
            //���� �����, �� "�� ������� ���� �����"
            if (resolver.like(@"^-?\d+$")) return source.AddDays(-Int32.Parse(resolver));

            var c = 1;

            //���� W -�� "� ������ ������", ����� ���������� �� ������� ��������� ������
            if (resolver.like(@"^[Ww]\d*"))
            {
                var cnt = resolver.find(@"\d+");
                if (cnt.hasContent())
                {
                    c = Int32.Parse(cnt);
                }
                var ds = 1 + (int)source.DayOfWeek;
                if (ds == 7) ds = 0;
                return source.AddDays(-7 * (c - 1) - ds);
            }
            //���� M -�� "� ������ ������", ����� ���������� �� ������� ��������� �������
            if (resolver.like(@"^[Mm]\d*"))
            {
                var cnt = resolver.find(@"\d+");
                if (cnt.hasContent())
                {
                    c = Int32.Parse(cnt);
                }

                return new DateTime(source.Year, source.Month, 1).AddMonths(-(c - 1));
            }
            //���� Y -�� "� ������ ����", ����� ���������� �� ������� ��������� �����
            if (resolver.like(@"^[Yy]\d*"))
            {
                var cnt = resolver.find(@"\d+");
                if (cnt.hasContent())
                {
                    c = Int32.Parse(cnt);
                }

                return new DateTime(source.Year, 1, 1).AddYears(-(c - 1));
            }

            throw new InvalidOperationException("�������� ������ ���������� - " + resolver);
        }
        /// <summary>
        /// normalize year against current - if solid year given (>1900)
        /// it will be returned without change, otherwise offset from current year returned
        /// on value, given in year parameter
        /// </summary>
        /// <param name="year">year or offset</param>
        /// <returns>given year or current year+offset</returns>
        public static int normalizeYear(this int year)
        {
            return normalizeYear(DateTime.Today.Year, year);
        }
        /// <summary>
        /// normalize year against baseyear - if solid year given (>1900)
        /// it will be returned without change, otherwise offset from baseyear returned
        /// on value, given in year parameter
        /// </summary>
        /// <param name="year">year or offset</param>
        /// <param name="baseyear">base year</param>
        /// <returns>given year or baseyear+offset</returns>
        public static int normalizeYear(int baseyear, int year)
        {
            if (year < 1900)
            {
                return baseyear + year;
            }
            return year;
        }
    }
}