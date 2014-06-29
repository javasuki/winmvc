using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NXDO.Data.Attribute;

namespace EMP.MvcDemo.Models
{
    public class EmpModel
    {
        [Display("编号")]
        public int ID { get; set; }

        [Display("姓名")]
        public string Name { get; set; }

        [Display(Name = "性别")]
        public bool Sex { get; set; }

        [Display(Name = "出身年月")]
        public DateTime? Birthday { get; set; }
    }
}
