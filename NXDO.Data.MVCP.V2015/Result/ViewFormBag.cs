using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 视图与控制器间的数据传递包
    /// </summary>
    internal class ViewFormBag
    {
        ///// <summary>
        ///// 是否呈现模态窗体
        ///// </summary>
        //public bool IsMode
        //{
        //    get;
        //    set;
        //}

        ///// <summary>
        ///// 是否作为子窗体
        ///// 0:普通Form，1：始终创建子MDI，2：如果存在则激活子MDI
        ///// </summary>
        //public int MdiChildCreateOrActive
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// 模态窗体的返回值
        /// <para>-1时，非模态窗体</para>
        /// </summary>
        public int ModeDialogResult
        {
            get;
            set;
        }

        /// <summary>
        /// true:始终创建窗体(Form/FormMdi)
        /// </summary>
        public bool IsAlwaysCreate
        {
            get;
            set;
        }

        /// <summary>
        /// 视图创建模式
        /// </summary>
        public ViewCreateFlag CreateMode
        {
            get
            {
                if (System.Windows.Forms.Application.OpenForms.Count == 0)
                    return ViewCreateFlag.FormApplication;

                return createMode;
            }
            set
            {
                createMode = value;
            }
        }ViewCreateFlag createMode = ViewCreateFlag.Form;
    }

    /// <summary>
    /// 指定标识符以指示视图创建模式
    /// </summary>
    internal enum ViewCreateFlag
    {
        /// <summary>
        /// 应用程序第一个窗体
        /// </summary>
        FormApplication,

        /// <summary>
        /// 对话框窗体
        /// </summary>
        FormDialog,

        /// <summary>
        /// 普通窗体
        /// </summary>
        Form,

        /// <summary>
        /// Mdi子窗体
        /// </summary>
        FormMdi
    }
}
