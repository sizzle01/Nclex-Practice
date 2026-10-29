using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nclex.Application.Admin.Banks;
using Nclex.Application.Admin.Media;
using Nclex.Application.Admin.Membership;
using Nclex.Application.Admin.Questions;
using Nclex.Application.Admin.Questions.CreateQuestion;
using Nclex.Application.Admin.Questions.CreateVersion;
using Nclex.Application.Admin.Questions.PreviewGrade;
using Nclex.Application.Admin.Questions.Publish;
using Nclex.Application.Admin.Questions.Retire;
using Nclex.Application.Admin.Questions.Review;
using Nclex.Application.Admin.Questions.Search;
using Nclex.Application.Admin.Questions.UpdateDraft;
using Nclex.Application.Admin.TestSets;
using Nclex.Application.Admin.Users;
using Nclex.Application.Attempts;
using Nclex.Application.Attempts.DiscardAttempt;
using Nclex.Application.Attempts.Expiry;
using Nclex.Application.Attempts.GetAttempt;
using Nclex.Application.Attempts.GetItem;
using Nclex.Application.Attempts.Grading;
using Nclex.Application.Attempts.ListAttempts;
using Nclex.Application.Attempts.Navigate;
using Nclex.Application.Attempts.OpenAttempt;
using Nclex.Application.Attempts.PauseAttempt;
using Nclex.Application.Attempts.Results;
using Nclex.Application.Attempts.ResumeAttempt;
using Nclex.Application.Attempts.Reveal;
using Nclex.Application.Attempts.Review;
using Nclex.Application.Attempts.SaveResponse;
using Nclex.Application.Attempts.StartAttempt;
using Nclex.Application.Attempts.SubmitAttempt;
using Nclex.Application.Attempts.SubmitItem;
using Nclex.Application.Attempts.UpdateItemMarks;
using Nclex.Application.Auth.ConfirmEmailChange;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Auth.ForgotPassword;
using Nclex.Application.Auth.Lockout;
using Nclex.Application.Auth.Login;
using Nclex.Application.Auth.Logout;
using Nclex.Application.Auth.LogoutAll;
using Nclex.Application.Auth.Refresh;
using Nclex.Application.Auth.ResendVerification;
using Nclex.Application.Auth.ResetPassword;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Auth.Signup;
using Nclex.Application.Auth.VerifyEmail;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Application.Catalog.GetTestSet;
using Nclex.Application.Catalog.ListQuestionBanks;
using Nclex.Application.Catalog.ListTestSets;
using Nclex.Application.Common;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Application.Email;
using Nclex.Application.Flags.Moderation;
using Nclex.Application.Flags.MyFlags;
using Nclex.Application.Flags.ReportIssue;
using Nclex.Application.Media.GetMedia;
using Nclex.Application.Media.OpenMediaFile;
using Nclex.Application.Notifications;
using Nclex.Application.Notifications.DraftReminders;
using Nclex.Application.Notifications.Feed;
using Nclex.Application.Notifications.Preferences;
using Nclex.Application.Notifications.ReadState;
using Nclex.Application.Notifications.Stream;
using Nclex.Application.Performance;
using Nclex.Application.Performance.GetBreakdown;
using Nclex.Application.Performance.GetSummary;
using Nclex.Application.Performance.GetTrend;
using Nclex.Application.Performance.RebuildSnapshots;
using Nclex.Application.Performance.Snapshots;
using Nclex.Application.Taxonomy.GetTaxonomy;
using Nclex.Application.Users;
using Nclex.Application.Users.ChangeEmail;
using Nclex.Application.Users.ChangePassword;
using Nclex.Application.Users.DeleteAccount;
using Nclex.Application.Users.GetMe;
using Nclex.Application.Users.UpdateMe;
using Nclex.Domain.Grading;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Dropdown;
using Nclex.Domain.ItemTypes.FillBlankCalc;
using Nclex.Domain.ItemTypes.Grouped;
using Nclex.Domain.ItemTypes.Highlight;
using Nclex.Domain.ItemTypes.Matrix;
using Nclex.Domain.ItemTypes.MultipleChoice;
using Nclex.Domain.ItemTypes.OrderedResponse;
using Nclex.Domain.ItemTypes.Sata;
using Nclex.Domain.ItemTypes.SelectN;

namespace Nclex.Application;

public static class DependencyInjection
{
    // Phase 2 types (bowtie, hotspot...) are switched on when their modules are built.
    private const Phase EnabledItemTypePhase = Phase.Mvp;

    /// <summary>Registers the use-case handlers and request validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SignupRequestValidator>();

        services.AddScoped<SessionService>();
        services.AddScoped<EmailTokenService>();
        services.AddScoped<AccountEmails>();
        services.AddScoped<SecurityNotifications>();
        services.AddScoped<LoginThrottle>();
        services.AddScoped<AccountLockout>();
        services.AddScoped<Reauthentication>();
        services.AddScoped<CurrentAccount>();

        // Auth
        services.AddScoped<SignupHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<LogoutAllHandler>();
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<ResendVerificationHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ConfirmEmailChangeHandler>();

        // Users
        services.AddScoped<GetMeHandler>();
        services.AddScoped<UpdateMeHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<ChangeEmailHandler>();
        services.AddScoped<DeleteAccountHandler>();

        // Taxonomy
        services.AddScoped<GetTaxonomyHandler>();

        // Catalog
        services.AddScoped<PlanTierGuard>();
        services.AddScoped<LastAttempts>();
        services.AddScoped<ListQuestionBanksHandler>();
        services.AddScoped<ListTestSetsHandler>();
        services.AddScoped<GetTestSetHandler>();

        // Custom test builder
        services.AddScoped<QuestionPool>();
        services.AddScoped<GetAvailableCountHandler>();
        services.AddScoped<CreateCustomSetHandler>();

        // Attempts
        services.AddScoped<AttemptStore>();
        services.AddScoped<AttemptWriteGuard>();
        services.AddScoped<AttemptStateReader>();
        services.AddScoped<ResponseValidator>();
        services.AddScoped<RevealReader>();
        services.AddScoped<ItemViewReader>();
        services.AddScoped<StartAttemptHandler>();
        services.AddScoped<ListAttemptsHandler>();
        services.AddScoped<GetAttemptHandler>();
        services.AddScoped<OpenAttemptHandler>();
        services.AddScoped<GetItemHandler>();
        services.AddScoped<SaveResponseHandler>();
        services.AddScoped<UpdateItemMarksHandler>();
        services.AddScoped<NavigateHandler>();
        services.AddScoped<DiscardAttemptHandler>();
        services.AddScoped<PauseAttemptHandler>();
        services.AddScoped<ResumeAttemptHandler>();
        services.AddScoped<SubmitItemHandler>();
        services.AddScoped<SubmitAttemptHandler>();
        services.AddScoped<GetResultHandler>();
        services.AddScoped<GetReviewHandler>();

        // Notifications
        services.AddScoped<ListNotificationsHandler>();
        services.AddScoped<UnreadCountHandler>();
        services.AddScoped<MarkReadHandler>();
        services.AddScoped<MarkAllReadHandler>();
        services.AddScoped<IssueStreamTicketHandler>();
        services.AddScoped<PreferenceRows>();
        services.AddScoped<GetPreferencesHandler>();
        services.AddScoped<UpdatePreferencesHandler>();

        // Content feedback (flags)
        services.AddScoped<ReportIssueHandler>();
        services.AddScoped<ListMyFlagsHandler>();
        services.AddScoped<FlagQueueReader>();
        services.AddScoped<ListFlagQueueHandler>();
        services.AddScoped<UpdateFlagHandler>();

        // Media
        services.AddScoped<GetMediaHandler>();
        services.AddScoped<OpenMediaFileHandler>();

        // Performance
        services.AddScoped<PerformanceViewers>();
        services.AddScoped<GetSummaryHandler>();
        services.AddScoped<GetBreakdownHandler>();
        services.AddScoped<GetTrendHandler>();
        services.AddScoped<RequestSnapshotRebuildHandler>();

        // Admin console (F14)
        services.AddScoped<AuthoringValidator>();
        services.AddScoped<VersionLoader>();
        services.AddScoped<MediaReadiness>();
        services.AddScoped<AdminQuestionReader>();
        services.AddScoped<SearchQuestionsHandler>();
        services.AddScoped<CreateQuestionHandler>();
        services.AddScoped<CreateVersionHandler>();
        services.AddScoped<UpdateDraftHandler>();
        services.AddScoped<SubmitForReviewHandler>();
        services.AddScoped<ApproveVersionHandler>();
        services.AddScoped<RejectVersionHandler>();
        services.AddScoped<PublishVersionHandler>();
        services.AddScoped<RetireQuestionHandler>();
        services.AddScoped<PreviewGradeHandler>();
        services.AddScoped<CreateUploadUrlHandler>();
        services.AddScoped<UploadMediaFileHandler>();
        services.AddScoped<UpdateMediaHandler>();
        services.AddScoped<QuestionMembership>();
        services.AddScoped<AdminTestSetsHandler>();
        services.AddScoped<AdminBanksHandler>();
        services.AddScoped<AdminUsersHandler>();

        services.AddAttemptExpiry();
        services.AddPerformanceSnapshots();
        services.AddContentImport();

        return services;
    }

    /// <summary>
    /// Registers only what the content importer needs, so the worker's import-questions command can
    /// run without the API's auth services. <see cref="AddApplication"/> includes it.
    /// </summary>
    public static IServiceCollection AddContentImport(this IServiceCollection services)
    {
        services.AddItemTypes();
        services.AddScoped<CatalogImporter>();
        services.AddScoped<MediaImporter>();
        services.AddScoped<CaseStudyImporter>();
        services.AddScoped<ImportQuestionsHandler>();
        return services;
    }

    /// <summary>
    /// Registers what the expiry jobs need (architecture.md 5.1, rules 2 and 3), so the worker can run
    /// them without the API's auth services. Finalizing grades the attempt, so this includes grading.
    /// <see cref="AddApplication"/> includes it.
    /// </summary>
    public static IServiceCollection AddAttemptExpiry(this IServiceCollection services)
    {
        services.AddItemTypes();
        // Grading and the sweeper record metrics; both hosts share this one instance (architecture.md section 9).
        services.TryAddSingleton<NclexMetrics>();
        services.AddSingleton<IReadinessPolicy, ReadinessPolicy>();
        services.AddScoped<DimensionNames>();
        services.AddScoped<IGradingService, GradingService>();
        services.AddScoped<AttemptFinalizer>();
        services.AddScoped<ExpireAttemptHandler>();
        services.AddScoped<SweepExpiredAttemptsHandler>();
        return services;
    }

    /// <summary>
    /// Registers the draft reminder use case for the worker's recurring job (architecture.md 5.3),
    /// without the API's auth services.
    /// </summary>
    public static IServiceCollection AddDraftReminders(this IServiceCollection services, DraftReminderSettings settings)
    {
        services.AddSingleton(settings);
        services.AddScoped<SendDraftRemindersHandler>();
        return services;
    }

    /// <summary>
    /// Registers the snapshot jobs' use cases (architecture.md 5.5), so the worker can run them
    /// without the API's auth services. <see cref="AddApplication"/> includes it.
    /// </summary>
    public static IServiceCollection AddPerformanceSnapshots(this IServiceCollection services)
    {
        services.AddScoped<SnapshotCatchUp>();
        services.AddScoped<RefreshSnapshotsHandler>();
        services.AddScoped<RebuildSnapshotsHandler>();
        return services;
    }

    /// <summary>
    /// Checks at startup that every item type of the enabled phase has a module (interfaces.md
    /// section 3). Hosts that grade or import items call it right after building.
    /// </summary>
    public static void EnsureItemTypeModules(this IServiceProvider services) =>
        services.GetRequiredService<ItemTypeRegistry>().EnsureEveryTypeHasAModule(EnabledItemTypePhase);

    /// <summary>
    /// Registers one module per item type that has been built (interfaces.md section 3). Graders are
    /// pure and stateless, so singletons are safe. Both the importer and grading need them, so
    /// registering twice adds nothing.
    /// </summary>
    private static IServiceCollection AddItemTypes(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, MultipleChoiceModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, SataModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, SelectNModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, MatrixMcModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, MatrixMrModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, MultipleResponseGroupingModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, DropdownClozeModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, DropdownTableModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, HighlightTextModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, HighlightTableModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, FillBlankCalcModule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IItemTypeModule, OrderedResponseModule>());
        services.TryAddSingleton<ItemTypeRegistry>();
        return services;
    }
}
