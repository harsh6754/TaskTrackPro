using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repositories.Models
{
    public class t_Register
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]       
        public int c_userId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string c_firstName { get; set; } 

        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string? c_middleName { get; set; }  // Nullable

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string c_lastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string c_email { get; set; } 

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "Password")]
        public string c_password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [StringLength(100)]
        [Compare("c_password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        [NotMapped]
        public string c_confirmPassword { get; set; }

        [Required(ErrorMessage = "Address is required")]    
        [StringLength(255)]
        [Display(Name = "Address")]
        public string c_address { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]  
        [StringLength(15)]
        [Phone]
        [Display(Name = "Mobile Number")]
        public string c_mobile { get; set; }

        [Required(ErrorMessage = "Gender is required")]   
        [StringLength(10)]
        [Display(Name = "Gender")]
        public string c_gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime c_dob { get; set; } // Changed from string to DateTime

        // Foreign Keys
        [Required]
        [ForeignKey("Country")]
        public int c_countryId { get; set; }
        public virtual t_Country? Country { get; set; }

        [Required]
        [ForeignKey("State")]
        public int c_stateId { get; set; }
        public virtual t_State? State { get; set; }

        [Required]
        [ForeignKey("District")]
        public int c_districtId { get; set; }
        public virtual t_District? District { get; set; }

        [Required]
        [ForeignKey("City")]
        public int c_cityId { get; set; }
        public virtual t_City? City { get; set; }

        [StringLength(4000)]
        [Display(Name = "Profile Picture")]
        public string c_image { get; set; }  // Nullable, as image may not be uploaded initially

        [NotMapped]
        public IFormFile? ImageFile { get; set; }  // Prevents from being mapped to the database
    }
}
