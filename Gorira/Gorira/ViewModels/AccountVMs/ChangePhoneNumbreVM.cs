using System.ComponentModel.DataAnnotations;

namespace Gorira.ViewModels.AccountVMs
{
    public class ChangePhoneNumbreVM
    {
        [Phone]
        public string Phone { get; set; }
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }
}
