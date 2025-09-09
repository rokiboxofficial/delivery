using System;
using System.Reflection;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Utils.Primitives;

public class ErrorTests
{
    [Fact]
    public void WhenCheckingIsValueObjectAssignableFromError_ThenResultShouldBeTrue()
    {
        // Act.
        var isValueObjectAssignableFromError = typeof(ValueObject).IsAssignableFrom(typeof(Error));

        // Assert.
        isValueObjectAssignableFromError.Should().BeTrue();
    }

    [Fact]
    public void WhenConstructing_AndCodeIsNotNullOrEmptyAndMessageIsNotNullOrEmptyAndInnerErrorIsNull_ThenCodeAndMessageShouldBeInitializedButInnerErrorShouldNot()
    {
        // Arrange.
        const string code = "test_code";
        const string message = "test_message";

        // Act.
        var error = new Error(code, message);

        // Assert.
        new { error.Code, error.Message, error.InnerError }.Should().Be(
            new { Code = code, Message = message, InnerError = (Error) null });
    }

    [Theory]
    [InlineData("test_code", "")]
    [InlineData("test_code", null)]
    [InlineData("", "test_message")]
    [InlineData(null, "test_message")]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData(null, null)]
    public void WhenConstructing_AndEitherCodeOrMessageIsNullOrEmpty_ThenArgumentExceptionShouldBeThrown(string code, string message)
    {
        // Arrange and Act.
        Action act = ()
            => _ = new Error(code, message);

        // Assert.
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void WhenConstructing_AndCodeAndMessageAndInnerErrorAreNotNullOrEmpty_ThenCodeAndMessageAndInnerErrorShouldBeInitialized()
    {
        // Arrange.
        const string code = "test_code";
        const string message = "test_message";
        var innerError = new Error("inner_test_code", "inner_test_message");

        // Act.
        var error = new Error(code, message, innerError);

        // Assert.
        new { error.Code, error.Message, error.InnerError }.Should().Be(
            new { Code = code, Message = message, InnerError = innerError });
    }
    
    [Theory, MemberData(nameof(ErrorExpectedPairWhenInnerErrorNestingLevelIsVaryFrom0To5))]
    public void WhenSerializing_AndInnerErrorNestingLevelIsVaryFrom0To5_ThenSerializedShouldBeExpectedSerialized(Error error, string expectedSerialized)
    {
        // Arrange and Act.
        var serialized = error.Serialize();

        // Assert.
        serialized.Should().Be(expectedSerialized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(" ")]
    [InlineData("   \n\n\t\t    \t\n")]
    public void WhenDeserializing_AndSerializedIsNullOrEmptyOrWhiteSpace_ThenArgumentExceptionShouldBeThrown(string serialized)
    {
        // Arrange and Act.
        Action act = () => Error.Deserialize(serialized);

        // Assert.
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("plaintext")]
    [InlineData("||")]
    [InlineData("this is part1||")]
    [InlineData("part1||part2||part3")]
    [InlineData("part1||this is part2||part3||part4||part5")]
    [InlineData("part1||part2||part3||this is part4||part5||this is part6||part7")]
    public void WhenDeserializing_AndSerializedContainsEitherLessThan2PartsOrCountOfPartsIsOdd_ThenFormatExceptionShouldBeThrownAndExceptionMessageShouldContainSerialized(string serialized)
    {
        // Arrange and Act.
        Action act = () => Error.Deserialize(serialized);

        // Assert.
        act
            .Should().Throw<FormatException>()
            .Where(exception => exception.Message.Contains(serialized));
    }
    
    [Theory, MemberData(nameof(ExpectedErrorPairWhenInnerErrorNestingLevelIsVaryFrom0To5))]
    public void WhenDeserializing_AndInnerErrorNestingLevelIsVaryFrom0To5_ThenDeserializedShouldBeExpectedError(string serialized, Error expectedError)
    {
        // Arrange and Act.
        var deserialized = Error.Deserialize(serialized);

        // Assert.
        deserialized.Should().Be(expectedError);
    }

    [Theory, MemberData(nameof(ErrorEqualityComponentsPairWhenInnerErrorNestingLevelIsVaryFrom0To5))]
    public void WhenGettingEqualityComponents_AndInnerErrorNestingLevelIsVaryFrom0To5_ThenEqualityComponentsShouldBeExpectedEqualityComponents(Error error, IComparable[] expectedEqualityComponents)
    {
        // Arrange and Act.
        var equalityComponents = typeof(Error)
            .GetMethod("GetEqualityComponents", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(error, null);
        
        // Assert.
        equalityComponents.Should().BeEquivalentTo(expectedEqualityComponents, options => options.WithStrictOrdering());
    }
    
    public static TheoryData<Error, string> ErrorExpectedPairWhenInnerErrorNestingLevelIsVaryFrom0To5()
    {
        return new TheoryData<Error, string>
        {
            { NestingLevel0.Deserialized, NestingLevel0.Serialized},
            { NestingLevel1.Deserialized, NestingLevel1.Serialized},
            { NestingLevel2.Deserialized, NestingLevel2.Serialized},
            { NestingLevel3.Deserialized, NestingLevel3.Serialized},
            { NestingLevel4.Deserialized, NestingLevel4.Serialized},
            { NestingLevel5.Deserialized, NestingLevel5.Serialized},
        };
    }

    public static TheoryData<string, Error> ExpectedErrorPairWhenInnerErrorNestingLevelIsVaryFrom0To5()
    {
        return new TheoryData<string, Error>
        {
            { NestingLevel0.Serialized, NestingLevel0.Deserialized},
            { NestingLevel1.Serialized, NestingLevel1.Deserialized},
            { NestingLevel2.Serialized, NestingLevel2.Deserialized},
            { NestingLevel3.Serialized, NestingLevel3.Deserialized},
            { NestingLevel4.Serialized, NestingLevel4.Deserialized},
            { NestingLevel5.Serialized, NestingLevel5.Deserialized},
        };
    }

    public static TheoryData<Error, IComparable[]> ErrorEqualityComponentsPairWhenInnerErrorNestingLevelIsVaryFrom0To5()
    {
        return new TheoryData<Error, IComparable[]>
        {
            { NestingLevel0.Deserialized, NestingLevel0.EqualityComponents},
            { NestingLevel1.Deserialized, NestingLevel1.EqualityComponents},
            { NestingLevel2.Deserialized, NestingLevel2.EqualityComponents},
            { NestingLevel3.Deserialized, NestingLevel3.EqualityComponents},
            { NestingLevel4.Deserialized, NestingLevel4.EqualityComponents},
            { NestingLevel5.Deserialized, NestingLevel5.EqualityComponents},
        };
    }

    private record SerializedDeserializedPairWithEqualityComponents(string Serialized, Error Deserialized, IComparable[] EqualityComponents);

    private static SerializedDeserializedPairWithEqualityComponents NestingLevel0 { get; } = 
        new(
            "test_code0||test_message0",
            new Error("test_code0", "test_message0"),
            ["test_code0", "test_message0"]
        );
    
    private static SerializedDeserializedPairWithEqualityComponents NestingLevel1 { get; } = 
        new(
            "test_code0||test_message0||test_code1||test_message1",
            new Error("test_code0", "test_message0", new Error("test_code1", "test_message1" )),
            ["test_code0", "test_message0", "test_code1", "test_message1"]
        );
    
    private static SerializedDeserializedPairWithEqualityComponents NestingLevel2  { get; } =
        new(
            "test_code0||test_message0||test_code1||test_message1||test_code2||test_message2",
            new Error(
                "test_code0",
                "test_message0",
                new Error(
                    "test_code1",
                    "test_message1",
                    new Error(
                        "test_code2",
                        "test_message2"
                    )
                )
            ),
            [
                "test_code0", "test_message0",
                "test_code1", "test_message1",
                "test_code2", "test_message2"
            ]
        );

    private static SerializedDeserializedPairWithEqualityComponents NestingLevel3  { get; } = 
        new(
            "test_code0||test_message0||test_code1||test_message1||test_code2||test_message2||test_code3||test_message3",
            new Error(
                "test_code0",
                "test_message0",
                new Error(
                    "test_code1",
                    "test_message1",
                    new Error(
                        "test_code2",
                        "test_message2",
                        new Error(
                            "test_code3",
                            "test_message3"
                        )
                    )
                )
            ),
            [
                "test_code0", "test_message0",
                "test_code1", "test_message1",
                "test_code2", "test_message2",
                "test_code3", "test_message3"
            ]
        );

    private static SerializedDeserializedPairWithEqualityComponents NestingLevel4  { get; } = 
        new(
            "test_code0||test_message0||test_code1||test_message1||test_code2||test_message2||test_code3||test_message3||test_code4||test_message4",
            new Error(
                "test_code0",
                "test_message0",
                new Error(
                    "test_code1",
                    "test_message1",
                    new Error(
                        "test_code2",
                        "test_message2",
                        new Error(
                            "test_code3",
                            "test_message3",
                            new Error(
                                "test_code4",
                                "test_message4"
                            )
                        )
                    )
                )
            ),
            [
                "test_code0", "test_message0",
                "test_code1", "test_message1",
                "test_code2", "test_message2",
                "test_code3", "test_message3",
                "test_code4", "test_message4"
            ]
        );
    
    private static SerializedDeserializedPairWithEqualityComponents NestingLevel5  { get; } =
        new(
            "test_code0||test_message0||test_code1||test_message1||test_code2||test_message2||test_code3||test_message3||test_code4||test_message4||test_code5||test_message5",
            new Error(
                "test_code0",
                "test_message0",
                new Error(
                    "test_code1",
                    "test_message1",
                    new Error(
                        "test_code2",
                        "test_message2",
                        new Error(
                            "test_code3",
                            "test_message3",
                            new Error(
                                "test_code4",
                                "test_message4",
                                new Error(
                                    "test_code5",
                                    "test_message5"
                                )
                            )
                        )
                    )
                )
            ),
            [
                "test_code0", "test_message0",
                "test_code1", "test_message1",
                "test_code2", "test_message2",
                "test_code3", "test_message3",
                "test_code4", "test_message4",
                "test_code5", "test_message5"
            ]
        );
}