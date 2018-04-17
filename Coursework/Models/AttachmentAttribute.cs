using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace Coursework.Models
{
    // Custom upload validation by György Balássy, File upload validation in ASP.NET MVC, 15/04/13, https://gyorgybalassy.wordpress.com/2013/04/15/file-upload-validation-in-aspnet-mvc/
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AttachmentAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            HttpPostedFileBase file = value as HttpPostedFileBase;

            // The file is required.
            if (file == null)
            {
                return new ValidationResult("Please attach a file to upload.");
            }

            // The maximum allowed file size is 10MB.
            if (file.ContentLength > 2 * 1024 * 1024)
            {
                return new ValidationResult("Maximum allowed file size is 2MB.");
            }

            // Check extensions
            string ext = Path.GetExtension(file.FileName);
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (String.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext.ToLower()))
            {
                return new ValidationResult("Allowed file formats are jpg, jpeg and png.");
            }

            // Check minimum dimensions
            Image img = System.Drawing.Image.FromStream(file.InputStream);
            if (img.Width < 480 || img.Height < 1024)
            {
                return new ValidationResult("Minimum file dimensions are 1024x480, please upload a larger image.");
            }

            // Everything OK.
            return ValidationResult.Success;
        }
    }

    // Copy validator for not required
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AttachmentNotRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            HttpPostedFileBase file = value as HttpPostedFileBase;

            if (file != null)
            {
                // The maximum allowed file size is 10MB.
                if (file.ContentLength > 2 * 1024 * 1024)
                {
                    return new ValidationResult("Maximum allowed file size is 2MB.");
                }

                // Check extensions
                string ext = Path.GetExtension(file.FileName);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                if (String.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext.ToLower()))
                {
                    return new ValidationResult("Allowed file formats are jpg, jpeg and png.");
                }

                // Check minimum dimensions
                Image img = System.Drawing.Image.FromStream(file.InputStream);
                if (img.Width < 480 || img.Height < 1024)
                {
                    return new ValidationResult("Minimum file dimensions are 1024x480, please upload a larger image.");
                }
            }

            // Everything OK.
            return ValidationResult.Success;
        }
    }
}