using System.ComponentModel.DataAnnotations;

namespace ApothecaryShared
{
    public class ImageUploadModel
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; } // Base64 encoded content
    }
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        // Continue to define all other fields that are present in your form...
        // For example:
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }
        public string PreExistingConditions { get; set; }
        public string AdditionalDetails { get; set; }
    }

    public class PrescriptionImageDataModel
    {
        public string Base64Image { get; set; }
    }
    public class MedicineOrder
    {
        public string MedicineName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Add UnitPrice property
        public decimal TotalPrice { get; set; } // Add TotalPrice property
    }

}
