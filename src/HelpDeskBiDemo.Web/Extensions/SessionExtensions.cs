using HelpDeskBiDemo.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HelpDeskBiDemo.Web.Extensions;

public static class SessionExtensions
{
    private const string OwnerAuthorizedKey = "demo.owner.authorized";
    private const string CompanyIdKey = "demo.company.id";
    private const string CompanyNameKey = "demo.company.name";
    private const string RoleKey = "demo.role";
    private const string PersonIdKey = "demo.person.id";
    private const string PersonDisplayNameKey = "demo.person.display";

    public static void SetOwnerAuthorized(this ISession session)
    {
        session.SetString(OwnerAuthorizedKey, "1");
    }

    public static bool IsOwnerAuthorized(this ISession session)
    {
        return session.GetString(OwnerAuthorizedKey) == "1";
    }

    public static void ClearOwnerAuthorization(this ISession session)
    {
        session.Remove(OwnerAuthorizedKey);
    }

    public static void SetCompanyContext(this ISession session, int companyId, string companyName)
    {
        session.SetInt32(CompanyIdKey, companyId);
        session.SetString(CompanyNameKey, companyName);
    }

    public static int? GetCompanyId(this ISession session)
    {
        return session.GetInt32(CompanyIdKey);
    }

    public static string? GetCompanyName(this ISession session)
    {
        return session.GetString(CompanyNameKey);
    }

    public static void ClearCompanyContext(this ISession session)
    {
        session.Remove(CompanyIdKey);
        session.Remove(CompanyNameKey);
        session.ClearRoleContext();
    }

    public static void SetRoleContext(
        this ISession session,
        DemoRole role,
        int? personId = null,
        string? personDisplayName = null)
    {
        session.SetInt32(RoleKey, (int)role);

        if (personId.HasValue)
        {
            session.SetInt32(PersonIdKey, personId.Value);
        }
        else
        {
            session.Remove(PersonIdKey);
        }

        if (!string.IsNullOrWhiteSpace(personDisplayName))
        {
            session.SetString(PersonDisplayNameKey, personDisplayName);
        }
        else
        {
            session.Remove(PersonDisplayNameKey);
        }
    }

    public static DemoRole? GetRole(this ISession session)
    {
        var rawValue = session.GetInt32(RoleKey);

        return rawValue.HasValue ? (DemoRole)rawValue.Value : null;
    }

    public static int? GetPersonId(this ISession session)
    {
        return session.GetInt32(PersonIdKey);
    }

    public static string? GetPersonDisplayName(this ISession session)
    {
        return session.GetString(PersonDisplayNameKey);
    }

    public static void ClearRoleContext(this ISession session)
    {
        session.Remove(RoleKey);
        session.Remove(PersonIdKey);
        session.Remove(PersonDisplayNameKey);
    }
}
