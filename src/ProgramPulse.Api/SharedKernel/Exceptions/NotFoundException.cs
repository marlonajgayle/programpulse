using System.Globalization;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.SharedKernel.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base("not_found", message, ErrorType.NotFound)
    {
    }

    public NotFoundException(string resource, object key)
        : base(
            "not_found",
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} with id '{1}' was not found.",
                resource,
                key),
            ErrorType.NotFound)
    {
    }
}
