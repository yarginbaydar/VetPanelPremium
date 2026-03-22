using System.ComponentModel.DataAnnotations;

namespace VetPanelPremium.Models
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı Adı zorunludur.")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        // Gerçek projelerde şifreler "Hash"lenerek (kriptolanarak) tutulur, 
        // o yüzden uzunluğunu garanti olsun diye 255 yaptık.
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(255)] 
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string FullName { get; set; } = string.Empty; // Sağ üst köşede yazacak isim (Örn: Yargın Hocam)

        [StringLength(50)]
        public string Role { get; set; } = "Admin"; // Şimdilik yetki karmaşası olmasın, giren Admin olsun
    }
}