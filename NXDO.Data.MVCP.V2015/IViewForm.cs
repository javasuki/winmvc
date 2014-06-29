using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NXDO.Data.Factory;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// Form视图接口
    /// <para>用户定义的窗体需实现本接口。</para>
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    public interface IViewForm<TModel> where TModel : class, new()
    {
        /// <summary>
        /// 获取或设置视图帮助器，该帮助器用于实现模型数据间呈现等操作。
        /// </summary>
        MvcHelper<TModel> Mvc { get; set; }

        /// <summary>
        /// 将模型的数据关系,操作方法等关联注册到视图。
        /// </summary>
        /// <param name="gui">视图帮助器</param>
        void Register(MvcHelper<TModel> mvc);
    }
}
