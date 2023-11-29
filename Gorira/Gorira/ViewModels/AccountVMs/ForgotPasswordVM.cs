using System.ComponentModel.DataAnnotations;

namespace Gorira.ViewModels.AccountVMs
{
    public class ForgotPasswordVM
    {
        [EmailAddress]
        public string Email { get; set; }

    }
}
