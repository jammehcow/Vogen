﻿using System.Threading.Tasks;
using FluentAssertions;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;
using Vogen;

namespace Vogen.Tests.DiagnosticsTests.LocalConfig.Happy;

[UsesVerify] 
public class HappyTests
{
    private readonly ITestOutputHelper _output;

    public HappyTests(ITestOutputHelper output) => _output = output;
    
    [Theory]
    [InlineData("partial class")]
    [InlineData("partial struct")]
    [InlineData("readonly partial struct")]
    [InlineData("partial record class")]
    [InlineData("partial record struct")]
    [InlineData("readonly partial record struct")]
    public void Type_override(string type)
    {
        var source = $@"using System;
using Vogen;
namespace Whatever;

[ValueObject(typeof(float))]
public {type} CustomerId {{ }}";
        
        var (diagnostics, output) = TestHelper.GetGeneratedOutput<ValueObjectGenerator>(source);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("partial class")]
    [InlineData("partial struct")]
    [InlineData("readonly partial struct")]
    [InlineData("partial record class")]
    [InlineData("partial record struct")]
    [InlineData("readonly partial record struct")]
    public void Exception_override(string type)
    {
        var source = $@"using System;
using Vogen;
namespace Whatever;

[ValueObject(throws: typeof(MyValidationException))]
public {type} CustomerId
{{
    private static Validation Validate(int value) => value > 0 ? Validation.Ok : Validation.Invalid(""xxxx"");
}}

public class MyValidationException : Exception
{{
    public MyValidationException(string message) : base(message) {{ }}
}}
";

        var (diagnostics, output) = TestHelper.GetGeneratedOutput<ValueObjectGenerator>(source);

        diagnostics.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("partial class")]
    [InlineData("partial struct")]
    [InlineData("readonly partial struct")]
    [InlineData("partial record class")]
    [InlineData("partial record struct")]
    [InlineData("readonly partial record struct")]
    public void Conversion_override(string type)
    {
        var source = $@"using System;
using Vogen;
namespace Whatever;

[ValueObject(conversions: Conversions.None)]
public {type} CustomerId {{ }}";
        
        var (diagnostics, output) = TestHelper.GetGeneratedOutput<ValueObjectGenerator>(source);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("partial class")]
    [InlineData("partial struct")]
    [InlineData("readonly partial struct")]
    [InlineData("partial record class")]
    [InlineData("partial record struct")]
    [InlineData("readonly partial record struct")]
    public void Conversion_and_exceptions_override(string type)
    {
        var source = $@"using System;
using Vogen;
namespace Whatever;

[ValueObject(conversions: Conversions.DapperTypeHandler, throws: typeof(Whatever.MyValidationException))]
public {type} CustomerId
{{
    private static Validation Validate(int value) => value > 0 ? Validation.Ok : Validation.Invalid(""xxxx"");
}}


public class MyValidationException : Exception
{{
    public MyValidationException(string message) : base(message) {{ }}
}}
";
        
        var (diagnostics, output) = TestHelper.GetGeneratedOutput<ValueObjectGenerator>(source);

        diagnostics.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("partial class")]
    [InlineData("partial struct")]
    [InlineData("readonly partial struct")]
    [InlineData("partial record class")]
    [InlineData("partial record struct")]
    [InlineData("readonly partial record struct")]
    public void Override_global_config_locally(string type)
    {
        var source = $@"using System;
using Vogen;

[assembly: VogenDefaults(underlyingType: typeof(string), conversions: Conversions.None, throws:typeof(Whatever.MyValidationException))]

namespace Whatever;

[ValueObject(underlyingType:typeof(float))]
public {type} CustomerId
{{
    private static Validation Validate(float value) => value > 0 ? Validation.Ok : Validation.Invalid(""xxxx"");
}}

public class MyValidationException : Exception
{{
    public MyValidationException(string message) : base(message) {{ }}
}}
";
        
        var (diagnostics, output) = TestHelper.GetGeneratedOutput<ValueObjectGenerator>(source);

        diagnostics.Should().BeEmpty();
    }
}
