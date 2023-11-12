using System.ComponentModel.DataAnnotations;

namespace Gorira.ViewModels.AccountVMs
{
    public class ChangeUserNameVM
    {
     
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }
}
