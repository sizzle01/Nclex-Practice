namespace Nclex.Domain.Entities;

/// <summary>
/// Membership of a question in a bank. Maps to <c>question_bank_items</c> (database.md section 2.4).
/// It points at the question, not a version, so a bank always serves the current published version.
/// </summary>
public sealed class QuestionBankItem
{
    // For EF Core, which sets properties directly when loading rows.
    private QuestionBankItem()
    {
    }

    public Guid BankId { get; private set; }

    public Guid QuestionId { get; private set; }
}
