using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Seed;

/// <summary>
/// The NCLEX test plans' Client Needs trees and the body-system tags, loaded by the F04_Taxonomy
/// migration. RN follows PRD 2.4 (NCSBN 2023 RN test plan); the PN plan differs in its categories
/// and weights, so it is seeded separately (database.md section 2.3). Weights are percentages and are
/// set on the leaves that questions are tagged to.
/// </summary>
/// <remarks>
/// Ids are fixed so that parents can be referenced and so content and tests can rely on them.
/// The migration moves each table's id sequence past the last seeded id.
/// </remarks>
internal static class TaxonomySeedData
{
    public static object[] ClientNeedsCategories { get; } =
    [
        // RN
        Category(1, ExamTrack.RN, null, "SECE", "Safe & Effective Care Environment", 1),
        Category(2, ExamTrack.RN, 1, "SECE.MOC", "Management of Care", 1, 15, 21),
        Category(3, ExamTrack.RN, 1, "SECE.SIC", "Safety and Infection Control", 2, 10, 16),
        Category(4, ExamTrack.RN, null, "HPM", "Health Promotion and Maintenance", 2, 6, 12),
        Category(5, ExamTrack.RN, null, "PSI", "Psychosocial Integrity", 3, 6, 12),
        Category(6, ExamTrack.RN, null, "PHI", "Physiological Integrity", 4),
        Category(7, ExamTrack.RN, 6, "PHI.BCC", "Basic Care and Comfort", 1, 6, 12),
        Category(8, ExamTrack.RN, 6, "PHI.PPT", "Pharmacological and Parenteral Therapies", 2, 13, 19),
        Category(9, ExamTrack.RN, 6, "PHI.RRP", "Reduction of Risk Potential", 3, 9, 15),
        Category(10, ExamTrack.RN, 6, "PHI.PA", "Physiological Adaptation", 4, 11, 17),

        // PN
        Category(11, ExamTrack.PN, null, "SECE", "Safe & Effective Care Environment", 1),
        Category(12, ExamTrack.PN, 11, "SECE.CC", "Coordinated Care", 1, 18, 24),
        Category(13, ExamTrack.PN, 11, "SECE.SIC", "Safety and Infection Control", 2, 10, 16),
        Category(14, ExamTrack.PN, null, "HPM", "Health Promotion and Maintenance", 2, 6, 12),
        Category(15, ExamTrack.PN, null, "PSI", "Psychosocial Integrity", 3, 9, 15),
        Category(16, ExamTrack.PN, null, "PHI", "Physiological Integrity", 4),
        Category(17, ExamTrack.PN, 16, "PHI.BCC", "Basic Care and Comfort", 1, 7, 13),
        Category(18, ExamTrack.PN, 16, "PHI.PT", "Pharmacological Therapies", 2, 10, 16),
        Category(19, ExamTrack.PN, 16, "PHI.RRP", "Reduction of Risk Potential", 3, 9, 15),
        Category(20, ExamTrack.PN, 16, "PHI.PA", "Physiological Adaptation", 4, 7, 13),
    ];

    public static object[] BodySystems { get; } =
    [
        BodySystem(1, "cardiovascular", "Cardiovascular"),
        BodySystem(2, "respiratory", "Respiratory"),
        BodySystem(3, "neurological", "Neurological"),
        BodySystem(4, "gastrointestinal", "Gastrointestinal"),
        BodySystem(5, "renal-urinary", "Renal and Urinary"),
        BodySystem(6, "endocrine", "Endocrine"),
        BodySystem(7, "musculoskeletal", "Musculoskeletal"),
        BodySystem(8, "integumentary", "Integumentary"),
        BodySystem(9, "hematologic-oncologic", "Hematologic and Oncologic"),
        BodySystem(10, "immune", "Immune"),
        BodySystem(11, "reproductive", "Reproductive"),
        BodySystem(12, "maternal-newborn", "Maternal and Newborn"),
        BodySystem(13, "pediatrics", "Pediatrics"),
        BodySystem(14, "mental-health", "Mental Health"),
        BodySystem(15, "pharmacology", "Pharmacology"),
        BodySystem(16, "fundamentals", "Fundamentals"),
    ];

    // HasData takes anonymous objects, so the entities can keep their private setters.
    private static object Category(
        short id, ExamTrack track, short? parentId, string code, string name, short sortOrder,
        decimal? weightMin = null, decimal? weightMax = null) =>
        new
        {
            Id = id,
            Track = track,
            ParentId = parentId,
            Code = code,
            Name = name,
            WeightMin = weightMin,
            WeightMax = weightMax,
            SortOrder = sortOrder,
        };

    private static object BodySystem(short id, string slug, string name) => new { Id = id, Slug = slug, Name = name };
}
