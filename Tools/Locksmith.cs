using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

public class LocksmithComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Components().Append<Locksmith9>();
    }
}

public class Locksmith9 : IComponent
{
    private readonly IUserService _userService;
    private readonly LegacyPasswordSecurity _legacyPasswordSecurity;
    private readonly IJsonSerializer _jsonSerializer;
    private GlobalSettings _globalSettings;
    public Locksmith9(IUserService userService, LegacyPasswordSecurity legacyPasswordSecurity, IJsonSerializer jsonSerializer, IOptions<GlobalSettings> globalSettings)
    {
        _userService = userService;
        _legacyPasswordSecurity = legacyPasswordSecurity;
        _jsonSerializer = jsonSerializer;
        _globalSettings = globalSettings.Value;
    }

    public void Initialize()
    {
        string uName = "admin@admin.com";
        //^modify this if you already have a user with this email
        string uPassword = "password123";
        var user = _userService.GetByEmail(uName);
        if (user == null)
        {
            var userGroup = _userService.GetUserGroupByAlias("admin") as IReadOnlyUserGroup;
            var groups = new List<IReadOnlyUserGroup>();
            groups.Add(userGroup);
            var newPerson = new BackOfficeIdentityUser(_globalSettings, 729, groups);
            var hasher = new UmbracoPasswordHasher<BackOfficeIdentityUser>(_legacyPasswordSecurity, _jsonSerializer);
            newPerson.PasswordHash = hasher.HashPassword(newPerson, uPassword);
            newPerson.Name = uName;
            newPerson.Email = uName;
            IUser newUser = _userService.CreateUserWithIdentity(uName, uName);
            newUser.AddGroup(userGroup);
            newUser.RawPasswordValue = newPerson.PasswordHash;
            _userService.Save(newUser);
        }
    }

    public void Terminate()
    {
    }
}
