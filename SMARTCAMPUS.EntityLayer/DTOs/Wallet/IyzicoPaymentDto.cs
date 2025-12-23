using System.ComponentModel.DataAnnotations;

namespace SMARTCAMPUS.EntityLayer.DTOs.Wallet
{
    public class IyzicoPaymentDto
    {
        [Required]
        [Range(1, 100000, ErrorMessage = "Tutar 1 ile 100.000 arasında olmalıdır")]
        public decimal Amount { get; set; }

        // Müşteri bilgileri (Token'dan alınacak ama override etmek için opsiyonel alanlar)
        public string? HolderName { get; set; }
        public string? Email { get; set; }
        public string? GsmNumber { get; set; }
        
        // Adres bilgileri (Varsayılan veya override)
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; } = "Turkey";
    }
}
