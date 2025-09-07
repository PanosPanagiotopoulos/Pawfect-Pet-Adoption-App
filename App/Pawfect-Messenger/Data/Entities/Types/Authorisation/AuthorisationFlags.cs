namespace Pawfect_Messenger.Data.Entities.Types.Authorisation
{
    [Flags]
    public enum AuthorizationFlags : int
    {
        None = 1 << 0,
        Permission = 1 << 1,
        Owner = 1 << 2,
        Affiliation = 1 << 3,
        OwnerOrPermissionOrAffiliation = Permission | Owner | Affiliation,
        OwnerOrPermission = Permission | Owner
    }
}
