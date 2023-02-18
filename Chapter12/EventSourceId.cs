using Fundamentals;

namespace Chapter12;

public record EventSourceId(string Value) : ConceptAs<string>(Value)
{
    public static EventSourceId New() => new(Guid.NewGuid().ToString());
}