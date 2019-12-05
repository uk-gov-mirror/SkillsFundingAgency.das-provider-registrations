using System.ComponentModel.DataAnnotations;
using SFA.DAS.ProviderRegistrations.Web.Validation;

namespace SFA.DAS.ProviderRegistrations.Web.ViewModels
{
    public class NewEmployerUserViewModel
    {
        [Display(Name = "employer organisation")]
        [MaxLength(100)]
        [Required]
        public string EmployerOrganisation { get; set; }

        [Display(Name = "employer first name")]
        [MaxLength(100)]
        [Required]
        public string EmployerFirstName { get; set; }

        [Display(Name = "employer last name")]
        [MaxLength(100)]
        [Required]
        public string EmployerLastName { get; set; }

        [Display(Name = "employer email address")]
        [MaxLength(100)]
        [Required]
        [EmailAddress]
        [Unsubscribed]
        [InUse]
        public string EmployerEmailAddress { get; set; }
    }
}