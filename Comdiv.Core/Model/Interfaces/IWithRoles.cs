namespace Comdiv.Model.Interfaces {
    public interface IWithRoles
    {
        [Map("roles")]
        string Roles { get; set; }
    }
}