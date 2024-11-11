using System.Security.Cryptography;
using System.Text;

using PaymentGateway.Models.Requests;

namespace PaymentGateway.Api.Services;

public class PaymentIdProvider :  IPaymentIdProvider
{
    public Guid Generate(PostPaymentRequest paymentRequest)
    {
        string concatenatedInputs = new StringBuilder()
            .Append(paymentRequest.CardNumberLastFour)
            .Append(paymentRequest.ExpiryYear)
            .Append(paymentRequest.ExpiryMonth)
            .Append(paymentRequest.Amount)
            .Append(DateTime.Now).ToString();
        
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedInputs));

            // Use the first 16 bytes to create a GUID
            byte[] guidBytes = new byte[16];
            Array.Copy(hashBytes, guidBytes, 16);

            // Set version to 4 (random) and variant to RFC 4122
            guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); 

            return new Guid(guidBytes);
        }
    }
}