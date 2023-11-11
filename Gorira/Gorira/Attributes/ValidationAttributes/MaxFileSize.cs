using System.ComponentModel.DataAnnotations;

namespace Gorira.Attributes.ValidationAttributes
{
    public class MaxFileSize : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSize(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            List<IFormFile> files = new List<IFormFile>();

            if (value is List<IFormFile>)
            {
                files = value as List<IFormFile>;
            }
            else if (value is IFormFile)
            {
                files.Add(value as IFormFile);
            }
            foreach (var file in files)
            {
                if (file.Length > (_maxFileSize * 1024 * 1024))
                {
                    return new ValidationResult("File size must be smaller than" + _maxFileSize + "MB");
                }
            }


            return ValidationResult.Success;
        }
    }
}
