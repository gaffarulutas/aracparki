using AracParki.Application.Conversations;
using AracParki.Application.Conversations.Dtos;
using AracParki.Application.Conversations.Services;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Queries;
using AracParki.Application.Notifications;
using AracParki.Domain.Listings;
using AracParki.Domain.Notifications;
using NSubstitute;

namespace AracParki.UnitTests;

public sealed class MessagingServiceTests
{
    private readonly IListingQuery _listings = Substitute.For<IListingQuery>();
    private readonly INotificationService _notifications = Substitute.For<INotificationService>();
    private readonly FakeMessageStore _store = new();
    private readonly MessagingService _sut;

    public MessagingServiceTests()
    {
        _sut = new MessagingService(_store, _listings, _notifications);
    }

    [Fact]
    public void NotificationTypes_MessageReceived_IsStable()
    {
        Assert.Equal("message.received", NotificationTypes.MessageReceived);
    }

    [Fact]
    public async Task StartOrGetThread_rejects_own_listing()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 10));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.StartOrGetThreadAsync("AP-1", 10, CancellationToken.None));

        Assert.Contains("Kendi ilan", ex.Message, StringComparison.Ordinal);
        Assert.Empty(_store.Threads);
    }

    [Fact]
    public async Task StartOrGetThread_rejects_unpublished()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 2, status: ListingStatus.PendingReview));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None));
    }

    [Fact]
    public async Task StartOrGetThread_is_unique_per_listing_and_buyer()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 2));

        var first = await _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None);
        var second = await _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None);

        Assert.Equal(first, second);
        Assert.Single(_store.Threads);
    }

    [Fact]
    public async Task GetThread_returns_null_for_third_party()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 2));
        var threadId = await _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None);

        Assert.Null(await _sut.GetThreadAsync(threadId, 99, CancellationToken.None));
        Assert.NotNull(await _sut.GetThreadAsync(threadId, 5, CancellationToken.None));
        Assert.NotNull(await _sut.GetThreadAsync(threadId, 2, CancellationToken.None));
    }

    [Fact]
    public async Task Send_rejects_empty_and_overlong_body()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 2));
        var threadId = await _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SendAsync(threadId, 5, "   ", CancellationToken.None));

        var tooLong = new string('a', MessagingService.BodyMaxLength + 1);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SendAsync(threadId, 5, tooLong, CancellationToken.None));
    }

    [Fact]
    public async Task Send_and_read_notifies_counterparty_and_clears_unread()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 2));
        var threadId = await _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None);

        await _sut.SendAsync(threadId, 5, "Merhaba, fiyat nedir?", CancellationToken.None);

        await _notifications.Received(1).NotifyAsync(
            2,
            NotificationTypes.MessageReceived,
            "Yeni mesaj",
            Arg.Is<string>(b => b != null && b.Contains("AP-1", StringComparison.Ordinal)),
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                d != null
                && d.ContainsKey("url")
                && Equals(d["url"], $"/mesajlarim/{threadId}")),
            Arg.Any<CancellationToken>());

        Assert.Equal(1, await _sut.CountUnreadThreadsAsync(2, CancellationToken.None));
        Assert.Equal(0, await _sut.CountUnreadThreadsAsync(5, CancellationToken.None));

        var sellerView = await _sut.GetThreadAsync(threadId, 2, CancellationToken.None);
        Assert.NotNull(sellerView);
        Assert.Single(sellerView!.Messages);
        Assert.False(sellerView.Messages[0].IsMine);
        Assert.Equal(0, await _sut.CountUnreadThreadsAsync(2, CancellationToken.None));

        await _sut.SendAsync(threadId, 2, "150.000 TL", CancellationToken.None);
        var buyerMsgs = await _sut.ListMessagesAsync(threadId, 5, afterId: null, CancellationToken.None);
        Assert.NotNull(buyerMsgs);
        Assert.Equal(2, buyerMsgs!.Count);
        Assert.True(buyerMsgs[0].IsMine);
        Assert.False(buyerMsgs[1].IsMine);

        Assert.Null(await _sut.ListMessagesAsync(threadId, 99, afterId: null, CancellationToken.None));
    }

    [Fact]
    public async Task Send_strips_control_characters()
    {
        _listings.GetByAdNoAsync("AP-1", ListingAccessContext.Anonymous, Arg.Any<CancellationToken>())
            .Returns(PublishedListing(ownerAccountId: 2));
        var threadId = await _sut.StartOrGetThreadAsync("AP-1", 5, CancellationToken.None);

        await _sut.SendAsync(threadId, 5, "Merhaba\0\u0001\nikinci satır", CancellationToken.None);

        var msgs = await _sut.ListMessagesAsync(threadId, 5, null, CancellationToken.None);
        Assert.NotNull(msgs);
        Assert.Equal("Merhaba\nikinci satır", msgs![0].Body);
    }

    private static ListingDetailDto PublishedListing(long ownerAccountId, string status = ListingStatus.Published) => new()
    {
        Id = 100,
        AdNo = "AP-1",
        Title = "Ekskavatör",
        Description = "desc",
        Category = "Ekskavatör",
        CategorySlug = "ekskavator",
        CategoryId = 1,
        CapacityMetric = "",
        Brand = "Brand",
        BrandId = 1,
        ModelName = "Model",
        PrimaryIntent = ListingIntent.Satilik,
        Intents = [ListingIntent.Satilik],
        Condition = "used",
        ModelYear = 2020,
        Tons = 20,
        CityId = 34,
        City = "İstanbul",
        CitySlug = "istanbul",
        DistrictId = 1,
        District = "Kadıköy",
        Price = 150_000,
        SpecsJson = "{}",
        CoverImageUrl = "/img/x.jpg",
        ImageUrls = [],
        Attachments = [],
        SellerName = "Satıcı",
        SellerType = "dealer",
        ListedAt = DateTimeOffset.UtcNow.AddDays(-1),
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
        Status = status,
        OwnerAccountId = ownerAccountId
    };

    private sealed class FakeMessageStore : IMessageStore
    {
        private long _nextThread = 1;
        private long _nextMessage = 1;

        public List<ThreadRec> Threads { get; } = [];
        public List<MessageRec> Messages { get; } = [];

        public Task<long> GetOrCreateThreadAsync(
            long listingId,
            long buyerAccountId,
            long sellerAccountId,
            CancellationToken cancellationToken)
        {
            var existing = Threads.FirstOrDefault(t =>
                t.ListingId == listingId && t.BuyerAccountId == buyerAccountId);
            if (existing is not null)
                return Task.FromResult(existing.Id);

            var thread = new ThreadRec
            {
                Id = _nextThread++,
                ListingId = listingId,
                BuyerAccountId = buyerAccountId,
                SellerAccountId = sellerAccountId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            Threads.Add(thread);
            return Task.FromResult(thread.Id);
        }

        public Task<MessageThreadDetailDto?> GetThreadForAccountAsync(
            long threadId,
            long accountId,
            CancellationToken cancellationToken)
        {
            var t = Threads.FirstOrDefault(x => x.Id == threadId);
            if (t is null || (t.BuyerAccountId != accountId && t.SellerAccountId != accountId))
                return Task.FromResult<MessageThreadDetailDto?>(null);

            var msgs = Messages
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ThreadId = m.ThreadId,
                    SenderAccountId = m.SenderAccountId,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    IsMine = m.SenderAccountId == accountId
                })
                .ToArray();

            return Task.FromResult<MessageThreadDetailDto?>(new MessageThreadDetailDto
            {
                Id = t.Id,
                ListingId = t.ListingId,
                AdNo = "AP-1",
                ListingTitle = "Ekskavatör",
                CoverImageUrl = "/img/x.jpg",
                Price = 150_000,
                Currency = Currency.Try,
                PrimaryIntent = ListingIntent.Satilik,
                ListingStatus = ListingStatus.Published,
                BuyerAccountId = t.BuyerAccountId,
                SellerAccountId = t.SellerAccountId,
                CounterpartyName = accountId == t.BuyerAccountId ? "Satıcı" : "Alıcı",
                AmBuyer = accountId == t.BuyerAccountId,
                Messages = msgs
            });
        }

        public Task<MessageThreadMetaDto?> GetThreadMetaForAccountAsync(
            long threadId,
            long accountId,
            CancellationToken cancellationToken)
        {
            var t = Threads.FirstOrDefault(x => x.Id == threadId);
            if (t is null || (t.BuyerAccountId != accountId && t.SellerAccountId != accountId))
                return Task.FromResult<MessageThreadMetaDto?>(null);

            return Task.FromResult<MessageThreadMetaDto?>(new MessageThreadMetaDto
            {
                Id = t.Id,
                BuyerAccountId = t.BuyerAccountId,
                SellerAccountId = t.SellerAccountId,
                AdNo = "AP-1"
            });
        }

        public Task<IReadOnlyList<MessageThreadListItemDto>> ListThreadsForAccountAsync(
            long accountId,
            int take,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<MessageThreadListItemDto>>([]);

        public Task<IReadOnlyList<MessageDto>?> ListMessagesAsync(
            long threadId,
            long accountId,
            long? afterId,
            int take,
            CancellationToken cancellationToken)
        {
            var t = Threads.FirstOrDefault(x => x.Id == threadId);
            if (t is null || (t.BuyerAccountId != accountId && t.SellerAccountId != accountId))
                return Task.FromResult<IReadOnlyList<MessageDto>?>(null);

            var rows = Messages
                .Where(m => m.ThreadId == threadId && (afterId is null || m.Id > afterId))
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .Take(take)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ThreadId = m.ThreadId,
                    SenderAccountId = m.SenderAccountId,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    IsMine = m.SenderAccountId == accountId
                })
                .ToArray();

            return Task.FromResult<IReadOnlyList<MessageDto>?>(rows);
        }

        public Task<long> InsertMessageAsync(
            long threadId,
            long senderAccountId,
            string body,
            CancellationToken cancellationToken)
        {
            var t = Threads.FirstOrDefault(x => x.Id == threadId)
                ?? throw new InvalidOperationException("Konuşma bulunamadı.");
            if (t.BuyerAccountId != senderAccountId && t.SellerAccountId != senderAccountId)
                throw new InvalidOperationException("Konuşma bulunamadı.");

            var msg = new MessageRec
            {
                Id = _nextMessage++,
                ThreadId = threadId,
                SenderAccountId = senderAccountId,
                Body = body,
                CreatedAt = DateTimeOffset.UtcNow
            };
            Messages.Add(msg);
            t.LastMessageAt = msg.CreatedAt;
            return Task.FromResult(msg.Id);
        }

        public Task MarkReadAsync(long threadId, long accountId, CancellationToken cancellationToken)
        {
            var t = Threads.FirstOrDefault(x => x.Id == threadId);
            if (t is null) return Task.CompletedTask;
            var now = DateTimeOffset.UtcNow;
            if (t.BuyerAccountId == accountId) t.BuyerLastReadAt = now;
            if (t.SellerAccountId == accountId) t.SellerLastReadAt = now;
            return Task.CompletedTask;
        }

        public Task<int> CountUnreadThreadsAsync(long accountId, CancellationToken cancellationToken)
        {
            var count = Threads.Count(t =>
            {
                if (t.BuyerAccountId != accountId && t.SellerAccountId != accountId)
                    return false;
                var lastRead = t.BuyerAccountId == accountId ? t.BuyerLastReadAt : t.SellerLastReadAt;
                var cutoff = lastRead ?? DateTimeOffset.UnixEpoch;
                return Messages.Any(m =>
                    m.ThreadId == t.Id
                    && m.SenderAccountId != accountId
                    && m.CreatedAt > cutoff);
            });
            return Task.FromResult(count);
        }

        public Task<IReadOnlyList<UnreadMessageAlertDto>> ListUnreadIncomingAsync(
            long accountId,
            int take,
            CancellationToken cancellationToken)
        {
            var rows = Threads
                .Where(t => t.BuyerAccountId == accountId || t.SellerAccountId == accountId)
                .SelectMany(t =>
                {
                    var lastRead = t.BuyerAccountId == accountId ? t.BuyerLastReadAt : t.SellerLastReadAt;
                    var cutoff = lastRead ?? DateTimeOffset.UnixEpoch;
                    return Messages
                        .Where(m =>
                            m.ThreadId == t.Id
                            && m.SenderAccountId != accountId
                            && m.CreatedAt > cutoff)
                        .Select(m => new UnreadMessageAlertDto
                        {
                            MessageId = m.Id,
                            ThreadId = t.Id,
                            AdNo = "AP-1",
                            BodyPreview = m.Body.Length > 160 ? m.Body[..160] : m.Body,
                            CreatedAt = m.CreatedAt
                        });
                })
                .OrderByDescending(x => x.MessageId)
                .Take(take)
                .ToArray();

            return Task.FromResult<IReadOnlyList<UnreadMessageAlertDto>>(rows);
        }

        public sealed class ThreadRec
        {
            public long Id { get; init; }
            public long ListingId { get; init; }
            public long BuyerAccountId { get; init; }
            public long SellerAccountId { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
            public DateTimeOffset? LastMessageAt { get; set; }
            public DateTimeOffset? BuyerLastReadAt { get; set; }
            public DateTimeOffset? SellerLastReadAt { get; set; }
        }

        public sealed class MessageRec
        {
            public long Id { get; init; }
            public long ThreadId { get; init; }
            public long SenderAccountId { get; init; }
            public required string Body { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
        }
    }
}
