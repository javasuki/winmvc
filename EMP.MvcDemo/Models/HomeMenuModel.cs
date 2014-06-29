using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NXDO.Data.Attribute;

namespace EMP.MvcDemo.Models
{
    public class HomeMenuModel
    {
        [Display(Name="员工")]
        public string Emp { get; set; }

        [Display(Name="subview")]
        public string Form1 { get; set; }
    }
}
