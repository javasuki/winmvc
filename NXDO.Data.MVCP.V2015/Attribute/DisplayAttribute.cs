using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NXDO.Data.Attribute
{
    /// <summary>
    /// 标识属性或成员变量的在 View 中的显示文本。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,AllowMultiple=false,Inherited=true)]
    public class DisplayAttribute : System.Attribute
    {
        public DisplayAttribute() { }
        public DisplayAttribute(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            set;
        }

        internal static string GetDisplayName(MemberInfo member)
        {
            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), true);
            if (attrs.Length == 0)
                return string.Empty;

            return (attrs[0] as DisplayAttribute).Name;
        }
    }
}
