using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Re-grades every past fixed-length result to the F23 pass marks, so an old 70% "likely ready"
    /// does not disagree with a new 70% "close to pass". No schema change: readiness_label is text.
    /// Adaptive results (cat_theta set) keep the pass or fail of their ability estimate.
    /// </summary>
    public partial class F23_PassMarks : Migration
    {
        // Postgres round() takes halves away from zero, like ReadinessPolicy, so the edges agree:
        // 0.8550 shows as 86% and passes, 0.6950 shows as 70% and is close to pass.
        public const string RegradeSql = """
            UPDATE attempt_results
            SET pass_threshold = 0.8600,
                passed = round(score_pct * 100) >= 86,
                readiness_label = CASE
                    WHEN round(score_pct * 100) >= 86 THEN 'likely_ready'
                    WHEN round(score_pct * 100) >= 70 THEN 'close_to_pass'
                    ELSE 'needs_more_practice'
                END
            WHERE cat_theta IS NULL;
            """;

        // Back to the F10 rule: the threshold the set had when the attempt started, two labels,
        // and no estimate at all when the set had no threshold.
        public const string RestoreSql = """
            UPDATE attempt_results r
            SET pass_threshold = (a.settings ->> 'passThreshold')::numeric(5,4),
                passed = r.score_pct >= (a.settings ->> 'passThreshold')::numeric,
                readiness_label = CASE
                    WHEN a.settings ->> 'passThreshold' IS NULL THEN NULL
                    WHEN r.score_pct >= (a.settings ->> 'passThreshold')::numeric THEN 'likely_ready'
                    ELSE 'needs_more_practice'
                END
            FROM attempts a
            WHERE a.id = r.attempt_id AND r.cat_theta IS NULL;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RegradeSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RestoreSql);
        }
    }
}
