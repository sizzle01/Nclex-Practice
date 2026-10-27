namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Finds the template for an outbox row by its name.</summary>
public sealed class EmailTemplates(IEnumerable<IEmailTemplate> templates)
{
    private readonly Dictionary<string, IEmailTemplate> _byName = templates.ToDictionary(t => t.Name);

    /// <summary>The template called <paramref name="name"/>. Throws for unknown names, which fails that email's attempt.</summary>
    public IEmailTemplate Get(string name) =>
        _byName.TryGetValue(name, out var template)
            ? template
            : throw new InvalidOperationException($"No email template is called '{name}'.");
}
