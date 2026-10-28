using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Questions.CreateQuestion;

/// <summary>Body of POST /admin/questions.</summary>
/// <param name="Track">RN or PN; fixed for the life of the question.</param>
/// <param name="AccessTier">Free or premium; free when left out.</param>
/// <param name="Draft">Version 1's content and tags.</param>
public sealed record CreateQuestionRequest(ExamTrack Track, PlanTier? AccessTier, DraftInput Draft);
