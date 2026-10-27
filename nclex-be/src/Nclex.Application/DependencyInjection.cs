using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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
using Nclex.Application.Content.ImportQuestions;
using Nclex.Application.Email;
using Nclex.Application.Taxonomy.GetTaxonomy;
using Nclex.Application.Users;
using Nclex.Application.Users.ChangeEmail;
using Nclex.Application.Users.ChangePassword;
using Nclex.Application.Users.DeleteAccount;
using Nclex.Application.Users.GetMe;
using Nclex.Application.Users.UpdateMe;
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
        services.AddScoped<SecurityEmails>();
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
        services.AddScoped<ListQuestionBanksHandler>();
        services.AddScoped<ListTestSetsHandler>();
        services.AddScoped<GetTestSetHandler>();

        // Custom test builder
        services.AddScoped<QuestionPool>();
        services.AddScoped<GetAvailableCountHandler>();
        services.AddScoped<CreateCustomSetHandler>();

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
        services.AddScoped<ImportQuestionsHandler>();
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
    /// pure and stateless, so singletons are safe.
    /// </summary>
    private static IServiceCollection AddItemTypes(this IServiceCollection services)
    {
        services.AddSingleton<IItemTypeModule, MultipleChoiceModule>();
        services.AddSingleton<IItemTypeModule, SataModule>();
        services.AddSingleton<IItemTypeModule, SelectNModule>();
        services.AddSingleton<IItemTypeModule, MatrixMcModule>();
        services.AddSingleton<IItemTypeModule, MatrixMrModule>();
        services.AddSingleton<IItemTypeModule, MultipleResponseGroupingModule>();
        services.AddSingleton<IItemTypeModule, DropdownClozeModule>();
        services.AddSingleton<IItemTypeModule, DropdownTableModule>();
        services.AddSingleton<IItemTypeModule, HighlightTextModule>();
        services.AddSingleton<IItemTypeModule, HighlightTableModule>();
        services.AddSingleton<IItemTypeModule, FillBlankCalcModule>();
        services.AddSingleton<IItemTypeModule, OrderedResponseModule>();
        services.AddSingleton<ItemTypeRegistry>();
        return services;
    }
}
