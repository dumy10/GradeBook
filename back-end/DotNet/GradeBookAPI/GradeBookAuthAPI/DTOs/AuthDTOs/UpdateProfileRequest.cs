using System.ComponentModel.DataAnnotations;

namespace GradeBookAuthAPI.DTOs.AuthDTOs
{
    public class UpdateProfileRequest
    {
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }
    }
}
