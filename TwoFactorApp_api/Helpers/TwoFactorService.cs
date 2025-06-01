using OtpNet;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
namespace TwoFactorApp_api.Helpers;


public class TwoFactorService
{
    private int KEY_LENGTH = 20; // 160-bit key length for TOTP

    public string GenerateSecret()
    {
        var secret = KeyGeneration.GenerateRandomKey(KEY_LENGTH); // 160-bit
        
        // Base32 format, Google Authenticator uyumlu 
        return Base32Encoding.ToString(secret);;
    }
   
    public bool VerifyCode(string code, string secretKey)
    {
        var bytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(bytes);
        return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));
    }
   
    public MemoryStream GenerateQrCodeUri(string email, string secret)
    {
        if (string.IsNullOrEmpty(secret?.Trim()))
        {
            throw new ArgumentException("Secret key cannot be empty.", nameof(secret));
        }

        // Issuer ve URI oluştur
        string issuer = "Two Factor App";
        string uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits=6";

        // QR kodu oluştur
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        
        // QR kodunun ham verilerini al (modül matrisi)
        var qrCodeMatrix = qrCodeData.ModuleMatrix;

        // ImageSharp ile 250x250 piksel bir görüntü oluştur
        int moduleSize = 5; // Her modül 5x5 piksel
        int imageSize = qrCodeMatrix.Count * moduleSize;
        using var image = new Image<Rgba32>(imageSize, imageSize);

        // Görüntüyü beyaz arka planla doldur
        image.Mutate(ctx => ctx.BackgroundColor(Color.White));

        // QR kodunun modüllerini çiz
        for (int y = 0; y < qrCodeMatrix.Count; y++)
        {
            for (int x = 0; x < qrCodeMatrix.Count; x++)
            {
                var color = qrCodeMatrix[y][x] ? Color.Black : Color.White;
                image.Mutate(ctx => ctx.Fill(color, new RectangleF(x * moduleSize, y * moduleSize, moduleSize, moduleSize)));
            }
        }

        // Görüntüyü MemoryStream'e kaydet
        var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
    
}