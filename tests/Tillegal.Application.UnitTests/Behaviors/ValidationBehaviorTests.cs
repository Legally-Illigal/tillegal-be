using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Tillegal.Application.Common.Behaviors;
using Xunit;

namespace Tillegal.Application.UnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record SampleRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_Should_CallNext_WhenNoValidatorsRegistered()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<SampleRequest>>();
        var behavior = new ValidationBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest("ValidName");
        var nextMock = Substitute.For<RequestHandlerDelegate<string>>();
        nextMock.Invoke().Returns(Task.FromResult("SuccessResponse"));

        // Act
        var result = await behavior.Handle(request, nextMock, CancellationToken.None);

        // Assert
        result.Should().Be("SuccessResponse");
        await nextMock.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_Should_CallNext_WhenValidationPasses()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleRequest>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        var behavior = new ValidationBehavior<SampleRequest, string>([validator]);
        var request = new SampleRequest("ValidName");
        var nextMock = Substitute.For<RequestHandlerDelegate<string>>();
        nextMock.Invoke().Returns(Task.FromResult("SuccessResponse"));

        // Act
        var result = await behavior.Handle(request, nextMock, CancellationToken.None);

        // Assert
        result.Should().Be("SuccessResponse");
        await nextMock.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleRequest>>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required.")
        };
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleRequest>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult(failures)));

        var behavior = new ValidationBehavior<SampleRequest, string>([validator]);
        var request = new SampleRequest(string.Empty);
        var nextMock = Substitute.For<RequestHandlerDelegate<string>>();

        // Act
        var act = async () => await behavior.Handle(request, nextMock, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(exception => exception.Errors.Any(f => f.PropertyName == "Name" && f.ErrorMessage == "Name is required."));
        await nextMock.DidNotReceive().Invoke();
    }
}
