﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vogen.Diagnostics;

internal class DiagnosticCollection : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> _entries = new();
    
    public bool HasErrors { get; private set; }

    private static readonly DiagnosticDescriptor _typeCannotBeNested = CreateDescriptor(
        DiagnosticCode.TypeCannotBeNested,
        "Types cannot be nested",
        "Type '{0}' cannot be nested - remove it from inside {1}");

    private static readonly DiagnosticDescriptor _usingDefaultProhibited = CreateDescriptor(
        DiagnosticCode.UsingDefaultProhibited,
        "Using default of Value Objects is prohibited",
        "Type '{0}' cannot be constructed with default as it is prohibited.");

    private static readonly DiagnosticDescriptor _usingNewProhibited = CreateDescriptor(
        DiagnosticCode.UsingNewProhibited,
        "Using new to create Value Objects is prohibited. Please use the From method for creation.",
        "Type '{0}' cannot be constructed with 'new' as it is prohibited.");

    private static readonly DiagnosticDescriptor _cannotHaveUserConstructors = CreateDescriptor(
        DiagnosticCode.CannotHaveUserConstructors,
        "Cannot have user defined constructors",
        "Cannot have user defined constructors, please use the From method for creation.");

    private static readonly DiagnosticDescriptor _underlyingTypeCannotBeCollection = CreateDescriptor(
        DiagnosticCode.UnderlyingTypeCannotBeCollection,
        "Underlying type cannot be collection",
        "Type '{0}' has an underlying type of {1} which is not valid");

    private static readonly DiagnosticDescriptor _invalidConversions = CreateDescriptor(
        DiagnosticCode.InvalidConversions,
        "Invalid Conversions",
        "The Conversions specified do not match any known conversions - see the Conversions type");

    private static readonly DiagnosticDescriptor _underlyingTypeMustNotBeSameAsValueObject = CreateDescriptor(
        DiagnosticCode.UnderlyingTypeMustNotBeSameAsValueObject,
        "Invalid underlying type",
        "Type '{0}' has the same underlying type - must specify a primitive underlying type");

    private static readonly DiagnosticDescriptor _validationMustReturnValidationType = CreateDescriptor(
        DiagnosticCode.ValidationMustReturnValidationType,
        "Validation returns incorrect type",
        "{0} must return a Validation type");

    private static readonly DiagnosticDescriptor _validationMustBeStatic = CreateDescriptor(
        DiagnosticCode.ValidationMustBeStatic,
        "Validation must be static",
        "{0} must be static");

    private static readonly DiagnosticDescriptor _instanceMethodCannotHaveNullArgumentName = CreateDescriptor(
        DiagnosticCode.InstanceMethodCannotHaveNullArgumentName,
        "Instance attribute cannot have null name",
        "{0} cannot have a null name");

    private static readonly DiagnosticDescriptor _instanceMethodCannotHaveNullArgumentValue = CreateDescriptor(
        DiagnosticCode.InstanceMethodCannotHaveNullArgumentValue,
        "Instance attribute cannot have null value",
        "{0} cannot have a null value");

    private static readonly DiagnosticDescriptor _customExceptionMustDeriveFromException = CreateDescriptor(
        DiagnosticCode.CustomExceptionMustDeriveFromException,
        "Invalid custom exception",
        "{0} must derive from System.Exception");

    public void AddTypeCannotBeNested(INamedTypeSymbol typeModel, INamedTypeSymbol container) => 
        AddDiagnostic(_typeCannotBeNested, typeModel.Locations, typeModel.Name, container.Name);

    public void AddValidationMustReturnValidationType(MethodDeclarationSyntax member) => 
        AddDiagnostic(_validationMustReturnValidationType, member.GetLocation(), member.Identifier);

    public void AddValidationMustBeStatic(MethodDeclarationSyntax member) => 
        AddDiagnostic(_validationMustBeStatic, member.GetLocation(), member.Identifier);

    public void AddUsingDefaultProhibited(Location locationOfDefaultStatement, string voClassName) => 
        AddDiagnostic(_usingDefaultProhibited, voClassName, locationOfDefaultStatement);

    public static Diagnostic UsingDefaultProhibited(Location locationOfDefaultStatement, string voClassName) => 
        BuildDiagnostic(_usingDefaultProhibited, voClassName, locationOfDefaultStatement);

    public static Diagnostic UsingNewProhibited(Location location, string voClassName) => 
        BuildDiagnostic(_usingNewProhibited, voClassName, location);

    public void AddUsingNewProhibited(Location location, string voClassName) => 
        AddDiagnostic(_usingNewProhibited, voClassName, location);

    public void AddCannotHaveUserConstructors(IMethodSymbol constructor) => 
        AddDiagnostic(_cannotHaveUserConstructors, constructor.Locations);

    public void AddUnderlyingTypeMustNotBeSameAsValueObjectType(INamedTypeSymbol underlyingType) => 
        AddDiagnostic(_underlyingTypeMustNotBeSameAsValueObject, underlyingType.Locations, underlyingType.Name);

    public void AddUnderlyingTypeCannotBeCollection(INamedTypeSymbol voClass, INamedTypeSymbol underlyingType) => 
        AddDiagnostic(_underlyingTypeCannotBeCollection, voClass.Locations, voClass.Name, underlyingType);

    public void AddInvalidConversions(Location location) => AddDiagnostic(_invalidConversions, location);

    public void AddInstanceMethodCannotHaveNullArgumentName(INamedTypeSymbol voClass) => 
        AddDiagnostic(_instanceMethodCannotHaveNullArgumentName, voClass.Locations, voClass.Name);

    public void AddInstanceMethodCannotHaveNullArgumentValue(INamedTypeSymbol voClass) => 
        AddDiagnostic(_instanceMethodCannotHaveNullArgumentValue, voClass.Locations, voClass.Name);

    public void AddCustomExceptionMustDeriveFromException(INamedTypeSymbol symbol) => 
        AddDiagnostic(_customExceptionMustDeriveFromException, symbol.Locations, symbol.Name);

    private static DiagnosticDescriptor CreateDescriptor(DiagnosticCode code, string title, string messageFormat, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        string[] tags = severity == DiagnosticSeverity.Error ? new[] { WellKnownDiagnosticTags.NotConfigurable } : Array.Empty<string>();

        return new DiagnosticDescriptor(code.Format(), title, messageFormat, "Vogen", severity, isEnabledByDefault: true, customTags: tags);
    }

    private void AddDiagnostic(DiagnosticDescriptor descriptor, string name, Location location)
    {
        var diagnostic = Diagnostic.Create(descriptor, location, name);
        
        AddDiagnostic(diagnostic);
    }

    private static Diagnostic BuildDiagnostic(DiagnosticDescriptor descriptor, string name, Location location) => 
        Diagnostic.Create(descriptor, location, name);

    private void AddDiagnostic(DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object?[] args)
    {
        var locationsList = (locations as IReadOnlyList<Location>) ?? locations.ToList();

        var diagnostic = Diagnostic.Create(
            descriptor, 
            locationsList.Count == 0 ? Location.None : locationsList[0],
            locationsList.Skip(1), 
            args);
        
        AddDiagnostic(diagnostic);
    }

    private void AddDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[] args) => 
        AddDiagnostic(Diagnostic.Create(descriptor, location ?? Location.None, args));

    private void AddDiagnostic(Diagnostic diagnostic)
    {
        if (diagnostic.Severity == DiagnosticSeverity.Error)
        {
            HasErrors = true;
        }
        
        _entries.Add(diagnostic);
    }

    // Try and get the location of the whole 'Foo foo', and not just 'foo'
    private static IEnumerable<Location> SymbolLocations(ISymbol symbol)
    {
        var declaringReferences = symbol.DeclaringSyntaxReferences;

        return declaringReferences.Length > 0
            ? declaringReferences.Select(x => x.GetSyntax().GetLocation())
            : symbol.Locations;
    }

    public IEnumerator<Diagnostic> GetEnumerator() => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}