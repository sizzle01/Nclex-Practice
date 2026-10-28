using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Media;

/// <summary>
/// Signs media links with HMAC-SHA256 over the asset id, the part and the expiry, so a link can't
/// be changed to open another file or to live longer.
/// </summary>
public sealed class HmacMediaUrlSigner(IOptions<MediaStorageOptions> options, TimeProvider clock) : IMediaUrlSigner
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(options.Value.SigningKey);

    public MediaLink Sign(Guid mediaId, MediaPart part)
    {
        var expires = clock.GetUtcNow().Add(options.Value.LinkLifetime).ToUnixTimeSeconds();
        return new MediaLink(expires, WebEncoders.Base64UrlEncode(Mac(mediaId, part, expires)));
    }

    public bool Verify(Guid mediaId, MediaPart part, long expires, string signature)
    {
        if (expires < clock.GetUtcNow().ToUnixTimeSeconds())
        {
            return false;
        }

        byte[] given;
        try
        {
            given = WebEncoders.Base64UrlDecode(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        // Constant time, so the comparison reveals nothing about the right signature.
        return CryptographicOperations.FixedTimeEquals(given, Mac(mediaId, part, expires));
    }

    private byte[] Mac(Guid mediaId, MediaPart part, long expires) =>
        HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{mediaId:N}/{part}/{expires}")));
}
