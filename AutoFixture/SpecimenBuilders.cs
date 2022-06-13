using AutoFixture;
using AutoFixture.Kernel;
using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;

namespace vMotion.Api.Specs;

public class ObjectIdSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property
            && property.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase)
            && property.PropertyType == typeof(ObjectId))
        {
            return ObjectId.GenerateNewId();
        }

        return new NoSpecimen();
    }

    public static ICustomization ToCustomization()
    {
        return new ObjectIdSpecimenBuilder().ToCustomization();
    }
}

public abstract class PropertyNamedSpecimenBuilder<T> : PropertyNamedSpecimenBuilder<T, object>
{
    protected PropertyNamedSpecimenBuilder(string pattern) : base(pattern) { }
}

public abstract class PropertyNamedSpecimenBuilder<T, TR> : ISpecimenBuilder
{
    private readonly Regex _regex;

    protected PropertyNamedSpecimenBuilder(string pattern)
    {
        _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    protected abstract object GenerateValueOnMatch(ISpecimenContext context);

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property
            && (property.DeclaringType?.IsAssignableTo(typeof(TR)) ?? false)
            && _regex.IsMatch(property.Name))
        {
            if (property.PropertyType.IsAssignableFrom(typeof(T)))
                return GenerateValueOnMatch(context);
        }

        if (request is ParameterInfo pinfo
            && _regex.IsMatch(pinfo.Name!))
        {
            if (pinfo.ParameterType.IsAssignableFrom(typeof(T)))
                return GenerateValueOnMatch(context);
        }

        return new NoSpecimen();
    }
}

public class IdInterneSpecimenBuilder : PropertyNamedSpecimenBuilder<string>
{
    public IdInterneSpecimenBuilder(string pattern) : base(pattern)
    {
    }

    protected override string GenerateValueOnMatch(ISpecimenContext context)
    {
        return Guid.NewGuid().ToString("D");
    }

    public static ICustomization ToCustomization()
    {
        return new IdInterneSpecimenBuilder("^id$").ToCustomization();
    }

}

public class EmailAddressStringsGenerator : PropertyNamedSpecimenBuilder<string>
{
    public EmailAddressStringsGenerator(string pattern) : base(pattern)
    {
    }

    protected override object GenerateValueOnMatch(ISpecimenContext context)
    {
        return TryCreateMailAddress(context);
    }

    private static object TryCreateMailAddress(ISpecimenContext context)
    {
        EmailAddressLocalPart addressLocalPart = context.Resolve(typeof(EmailAddressLocalPart)) as EmailAddressLocalPart;
        DomainName domainName = context.Resolve(typeof(DomainName)) as DomainName;
        return addressLocalPart == null || domainName == null ? new NoSpecimen() : new MailAddress(string.Format(CultureInfo.InvariantCulture, "{0} <{0}@{1}>", addressLocalPart, domainName)).Address;
    }

    public static ICustomization ToCustomization()
    {
        return new EmailAddressStringsGenerator("email").ToCustomization();
    }
}

public class LinkSpecimenBuilder : PropertyNamedSpecimenBuilder<string>
{
    public LinkSpecimenBuilder(string pattern) : base(pattern)
    {
    }

    protected override object GenerateValueOnMatch(ISpecimenContext context)
    {
        return $"images-container;{Guid.NewGuid():N}.png";
    }
    public static ICustomization ToCustomization()
    {
        return new LinkSpecimenBuilder("(link|url)$").ToCustomization();
    }
}

public class PostalCodeStringsGenerator : PropertyNamedSpecimenBuilder<string>
{
    public PostalCodeStringsGenerator(string pattern) : base(pattern)
    {
    }

    protected override object GenerateValueOnMatch(ISpecimenContext context)
    {
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        var rnd = new Random();

        return $"{letters[rnd.Next(1, 26)]}{rnd.Next(0, 9)}{letters[rnd.Next(1, 26)]} {rnd.Next(0, 9)}{letters[rnd.Next(1, 26)]}{rnd.Next(0, 9)}";
    }

    public static ICustomization ToCustomization()
    {
        return new PostalCodeStringsGenerator("(postalcode|zipcode)").ToCustomization();
    }
}

public class CountryCodeStringsGenerator : PropertyNamedSpecimenBuilder<string>
{
    public CountryCodeStringsGenerator(string pattern) : base(pattern)
    {
    }

    protected override object GenerateValueOnMatch(ISpecimenContext context)
    {
        return "CA";
    }

    public static ICustomization ToCustomization()
    {
        return new CountryCodeStringsGenerator("country").ToCustomization();
    }
}

public class PhoneStringsGenerator : PropertyNamedSpecimenBuilder<string>
{
    public PhoneStringsGenerator(string pattern) : base(pattern)
    {
    }

    protected override object GenerateValueOnMatch(ISpecimenContext context)
    {
        var rnd = new Random();

        return $"514555{rnd.Next(1, 9999):D4}";
    }

    public static ICustomization ToCustomization()
    {
        return new PhoneStringsGenerator("phone|mobile|fax|homephone").ToCustomization();
    }
}

public class VersionSpecimenBuilder : PropertyNamedSpecimenBuilder<int>
{
    public VersionSpecimenBuilder(string pattern) : base(pattern)
    {
    }

    protected override object GenerateValueOnMatch(ISpecimenContext context)
    {
        return 1;
    }

    public static ICustomization ToCustomization()
    {
        return new VersionSpecimenBuilder("version").ToCustomization();
    }
}

public class RelationshipObject<T> : ISpecimenBuilder
    where T : IEntity
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property
            && (property.DeclaringType?.IsAssignableTo(typeof(One<T>)) ?? false))
        {
            if (property.PropertyType.IsAssignableFrom(typeof(T)))
                return ObjectId.GenerateNewId();
        }

        return new NoSpecimen();
    }
}