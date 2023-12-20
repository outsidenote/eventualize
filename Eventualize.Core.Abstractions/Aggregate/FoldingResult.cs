namespace Eventualize.Core;

// TODO: [bnaya 2023-12-15] What is the responsibility of this class, is it needed?

public record FoldingResult<T>(T State, long EventCount);
