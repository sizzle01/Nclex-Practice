namespace Nclex.Domain.Entities;

/// <summary>What a user is allowed to do (database enum <c>user_role</c>, architecture.md section 6).</summary>
public enum UserRole
{
    /// <summary>Takes practice tests and mock exams. Every new account starts here.</summary>
    Candidate,

    /// <summary>Nurse educator who follows a group of candidates.</summary>
    Educator,

    /// <summary>Writes and edits questions.</summary>
    Author,

    /// <summary>Qualified nurse educator who approves questions for publishing.</summary>
    Reviewer,

    /// <summary>Manages the platform.</summary>
    Admin,
}
