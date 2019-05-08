using System;

namespace GoogleTranslate.Desktop
{
    public static class ExtensionMethods
    {
        //实现js的charAt方法
        public static char charAt(this object obj, int index)
        {
            char[] chars = obj.ToString().ToCharArray();
            return chars[index];
        }
        //实现js的charCodeAt方法
        public static int charCodeAt(this object obj, int index)
        {
            char[] chars = obj.ToString().ToCharArray();
            return (int)chars[index];
        }

        //实现js的Number方法
        public static int Number(object cc)
        {
            try
            {
                long a = Convert.ToInt64(cc.ToString());
                int b = a > 2147483647 ? (int)(a - 4294967296) : a < -2147483647 ? (int)(a + 4294967296) : (int)a;
                return b;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
