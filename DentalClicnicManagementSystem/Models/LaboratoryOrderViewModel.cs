using CMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CMS.ViewModels
{
    public class LaboratoryOrderViewModel
    {
        public LaboratoryOrder Order { get; set; }
        public IEnumerable<SelectListItem> Patients { get; set; }
    }
}
