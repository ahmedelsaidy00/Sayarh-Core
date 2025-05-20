using Sayarah.Authorization.Users;

namespace Sayarah.Interfaces
{
    public interface IHasCreatorAndModeifierUserNavigation
    {
        User CreatorUser { get; set; }
        User LastModifierUser { get; set; }
    }
}
