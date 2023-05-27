namespace passkey_example_backend.Data;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public List<UserCredential> Credentials { get; set; } = new();
}

public class UserCredential
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public User User { get; set; }
    public string DescriptorJson { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }
}
