using MongoDB.Bson.Serialization.Conventions;

namespace TaskManager.Config.Db
{
    public class DbConventions
    {
        public static void RegisterCamelCaseConvention()
        {
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };

            ConventionRegistry.Register(
                "camelCase",
                conventionPack,
                _ => true // Apply to all classes
            );
        }
    }
}
