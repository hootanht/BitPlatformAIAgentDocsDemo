using BitPlatformAIAgentDocsDemo.Client.Core;

namespace BitPlatformAIAgentDocsDemo.Client.Web;

public class ClientWebSettings : ClientCoreSettings
{

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = base.Validate(validationContext).ToList();


        return validationResults;
    }
}

