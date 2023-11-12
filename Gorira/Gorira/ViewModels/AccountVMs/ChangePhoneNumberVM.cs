using System.ComponentModel.DataAnnotations;

namespace Gorira.ViewModels.AccountVMs
{
    public class ChangePhoneNumberVM
    {
        [Phone]
        public string Phone { get; set; }
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }
}
