using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("email-category")]

public readonly partial record struct PersonEmailCategoryUpdated(int PersonId, string Email, string Category);

