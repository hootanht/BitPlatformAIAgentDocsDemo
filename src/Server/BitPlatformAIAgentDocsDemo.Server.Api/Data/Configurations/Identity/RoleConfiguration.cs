using BitPlatformAIAgentDocsDemo.Server.Api.Models.Identity;

namespace BitPlatformAIAgentDocsDemo.Server.Api.Data.Configurations.Identity;

public partial class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(role => role.Name).HasMaxLength(50);
    }
}

