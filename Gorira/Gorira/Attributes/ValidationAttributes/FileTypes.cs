using NuGet.Packaging.Signing;
using System.ComponentModel.DataAnnotations;

namespace Gorira.Attributes.ValidationAttributes
{
    public class FileTypes : ValidationAttribute
    {
        string[] _fileTypes;

        public FileTypes(params string[] fileTypes)
        {
            _fileTypes = fileTypes;
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
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_fileTypes.Contains(file.ContentType) && (extension != ".rar") && (extension != ".zip"))
                {
                    return new ValidationResult("File type must be " + string.Join(", ", _fileTypes) );
                }
            }

            return ValidationResult.Success;
        }
    }
}
