using System.ComponentModel.DataAnnotations;

namespace Gorira.ViewModels.AccountVMs
{
    public class ChangePasswordVM
    {
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}
