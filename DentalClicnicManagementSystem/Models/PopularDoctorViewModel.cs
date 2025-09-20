// In: ViewModels/PopularDoctorViewModel.cs
using CMS.Models;
namespace CMS.ViewModels; // Assuming your Doctor model is in this namespace

public class PopularDoctorViewModel
{
    public Doctor Doctor { get; set; }
    public int BookingCount {get; set; }
}
