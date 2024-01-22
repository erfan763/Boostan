namespace Boostan.Application.Contracts.Encryption;

public interface IEncryptionService
{
    /// <summary>
    ///     Decrypt data
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <returns></returns>
    Task<IEnumerable<byte>> DecryptData(byte[] encryptedData);

    /// <summary>
    ///     Encrypt data
    /// </summary>
    /// <returns></returns>
    Task<byte[]> EncryptData(byte[] data);
}