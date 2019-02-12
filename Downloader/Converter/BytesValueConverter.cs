using System;
using System.Globalization;
using System.Windows.Data;
using M3U8Downloader.Infrastruction;

namespace M3U8Downloader.Converter
{
    public class BytesValueConverter : IValueConverter
    {
        #region Methods

        /// <summary>
        /// 数组数值转换器
        /// </summary>
        /// <param name="value">源数组</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">进制（int） 如：10【十进制】 / 8【八进制】</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] data)
            {
                if (int.TryParse(parameter?.ToString(), out var basev))
                    return data.ToString(basev);
                else
                    return data.ToHexString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}