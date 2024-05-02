using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FStudentManagement.Data
{
    public class SUser : IdentityUser
    {
        public string? FullName { get; set; }
        [Required(ErrorMessage = "ادخل ايميل صالح")]
        [DataType(DataType.EmailAddress, ErrorMessage = "ادخل ايميل صالح", ErrorMessageResourceName = "ادخل ايميل صالح")]
        [EmailAddress(ErrorMessage = "ادخل ايميل صالح")]
        public override string Email { get => base.Email; set => base.Email = value; }
        [MinLength(6, ErrorMessage = "الحد الادنى 6 حروف")]
        [DataType(DataType.Password)]
        public override string? PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

    }
}
