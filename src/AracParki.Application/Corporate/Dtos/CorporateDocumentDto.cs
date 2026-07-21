namespace AracParki.Application.Corporate.Dtos;

public sealed class CorporateDocumentDto
{
    public long Id { get; init; }
    public long CorporateAccountId { get; init; }
    public required string DocType { get; init; }
    public required string FileName { get; init; }
    public required string StorageKey { get; init; }
    public required string ContentType { get; init; }
    public long ByteSize { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}
