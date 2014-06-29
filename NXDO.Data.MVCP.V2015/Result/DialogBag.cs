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
        /// <summary>
        /// 是否呈现模态窗体
        /// </summary>
        public bool IsMode
        {
            get;
            set;
        }

        /// <summary>
        /// 是否作为子窗体
        /// </summary>
        public bool IsMdiChildren
        {
            get;
            set;
        }

        /// <summary>
        /// 模态窗体的返回值
        /// <para>-1时，非模态窗体</para>
        /// </summary>
        public int ModeDialogResult
        {
            get;
            set;
        }
    }
}
