using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NXDO.Data.Attribute
{
    /// <summary>
    /// 视图前缀名称。
    /// <para>标识 Controller 所持有 View 前缀名称。仅限于约定的视图不在Views目录下，而是在其子目录下。</para>
    /// <para>框架只搜索 Views 目录下的视图对象，如果Controller需要持有Views子目录下的视图对象，则定义前缀名称为该字目录的名称。子目录风格符可使用./\\字符</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false,Inherited=true)]
    public class MvcViewNameAttribute : System.Attribute
    {
        /// <summary>
        /// 初始化视图前缀名称。
        /// </summary>
        /// <param name="prefixName">Views子目录名称.</param>
        public MvcViewNameAttribute(string prefixName)
        {
            if (!string.IsNullOrEmpty(prefixName))
            {
                prefixName = prefixName.Replace('/', '.');
                prefixName = prefixName.Replace('\\', '.');
                if (prefixName.StartsWith("."))
                    prefixName = prefixName.Substring(1);

                if (prefixName.EndsWith("."))
                    prefixName = prefixName.Substring(0, prefixName.Length - 1);
            }
            this.PrefixName = prefixName;
        }

        /// <summary>
        /// View所在的子目录名称
        /// </summary>
        internal string PrefixName
        {
            get;
            private set;
        }
    }
}
