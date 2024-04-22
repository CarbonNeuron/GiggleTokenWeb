using System.Security.Cryptography;
using System.Text;

namespace GiggleTokenWeb;

public enum TokenLength
{
    Length112 = 112,
    Length156 = 156
}

public static class GiggleTokenGenerator
{
    private const string ClientId = "MsOIJ39Q28";
    private const string ClientSecret = "PTDc3H8a)Vi=UYap";

    // Thread-static attribute ensures each thread has its own instance.
    [ThreadStatic] private static SHA256? sha256Hasher;
    [ThreadStatic] private static SHA1? sha1Hasher;

    private static SHA256 Sha256Hasher => sha256Hasher ??= SHA256.Create();
    private static SHA1 Sha1Hasher => sha1Hasher ??= SHA1.Create();

    private static byte[] clientIdBytes = "MsOIJ39Q28"u8.ToArray();
    private static byte[] clientSecretBytes = "PTDc3H8a)Vi=UYap"u8.ToArray();

    public static string Create(Guid installationId, TokenLength tokenLength)
    {
        // Step 1: Generate UUID with hyphens removed
        Span<char> uuidSpan = stackalloc char[33];
        installationId.TryFormat(uuidSpan, out int charsWritten, "N");
        
        Span<byte> inputBytes = stackalloc byte[Encoding.UTF8.GetMaxByteCount(charsWritten)];
        var inputBytesSize = Encoding.UTF8.GetBytes(uuidSpan, inputBytes);
        
        // Step 2: Create hex representation based on token length
        Span<byte> hexSpan = stackalloc byte[tokenLength==TokenLength.Length156?64 : charsWritten];
        
        switch (tokenLength)
        {
            case TokenLength.Length156:
            {
                Span<byte> hashBytes = stackalloc byte[Sha256Hasher.HashSize / 8];
                Sha256Hasher.TryComputeHash(inputBytes.Slice(0, inputBytesSize-1), hashBytes, out int bytesWrittenSha256);
                for (int i = 0; i < bytesWrittenSha256; i++)
                {
                    byte b = hashBytes[i];
                    hexSpan[i * 2] = GetHexCharacter(b >> 4, true);
                    hexSpan[i * 2 + 1] = GetHexCharacter(b & 0xF, true);
                }
                
                break;
            }
            case TokenLength.Length112:
                ConvertToUpperInvariant(inputBytes.Slice(0, charsWritten), hexSpan);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenLength), $"Not expected token length value: {tokenLength}");
        }
        
        //suffix = $"{hex}:{ClientId}:{ClientSecret}";
        
        Span<byte> suffix = stackalloc byte[clientIdBytes.Length + hexSpan.Length + clientSecretBytes.Length + 2];
        hexSpan.CopyTo(suffix);
        suffix[hexSpan.Length] = 58; // :
        clientIdBytes.CopyTo(suffix.Slice(hexSpan.Length+1));
        suffix[hexSpan.Length + 1 + clientIdBytes.Length] = 58; // :
        clientSecretBytes.CopyTo(suffix.Slice(clientIdBytes.Length + hexSpan.Length + 2));
        
        
        Span<byte> suffixHash = stackalloc byte[20];
        Span<byte> suffixHashHex = stackalloc byte[40];
        
        Sha1Hasher.TryComputeHash(suffix, suffixHash, out int bytesWritten);
        for (int i = 0; i < bytesWritten; i++)
        {
            byte b = suffixHash[i];
            suffixHashHex[i * 2] = GetHexCharacter(b >> 4, false);
            suffixHashHex[i * 2 + 1] = GetHexCharacter(b & 0xF, false);
        }
        
        Span<byte> finalToken = stackalloc byte[hexSpan.Length+1+clientIdBytes.Length+1+suffixHashHex.Length];
        hexSpan.CopyTo(finalToken);
        var currentPos = hexSpan.Length;
        finalToken[currentPos] = 95; // _
        currentPos++;
        clientIdBytes.CopyTo(finalToken.Slice(currentPos));
        currentPos += clientIdBytes.Length;
        finalToken[currentPos] = 58; // :
        currentPos++;
        suffixHashHex.CopyTo(finalToken.Slice(currentPos));
        
        return Convert.ToBase64String(finalToken);
    }
    
    private static void ConvertToUpperInvariant(Span<byte> source, Span<byte> destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            char c = (char)source[i];
            // Ensure only hexadecimal characters are converted
            destination[i] = (byte)((c >= 0x61 && c <= 0x66) ? (char)(c - 32) : c);
        }
    }
    
    private static byte GetHexCharacter(int value, bool uppercase)
    {
        if (value < 10)
        {
            return (byte)(value + 0x30); // add to 0
        }
        else
        {
            return (byte)(value - 10 + (uppercase ? 0x41 : 0x61)); //Add to capital A or lowercase a
        }
    }
    
    
    private static void ConvertToUpperInvariant(Span<char> source, Span<char> destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            // Ensure only hexadecimal characters are converted
            destination[i] = (c >= 'a' && c <= 'f') ? (char)(c - 32) : c;
        }
    }
}