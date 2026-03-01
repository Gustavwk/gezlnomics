using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class RecurringRuleService : IRecurringRuleService
{
    private readonly IRecurringRuleGateway _gateway;

    public RecurringRuleService(IRecurringRuleGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<IReadOnlyList<RecurringRuleDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rules = await _gateway.GetAllAsync(userId, cancellationToken);
        return rules.Select(Map).ToList();
    }

    public async Task<RecurringRuleDto> CreateAsync(Guid userId, UpsertRecurringRuleRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var now = DateTime.UtcNow;
        var rule = new RecurringRule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Amount = Math.Abs(request.Amount),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Note = request.Note?.Trim(),
            RuleKind = request.RuleKind,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _gateway.AddAsync(rule, cancellationToken);
        return Map(rule);
    }

    public async Task<RecurringRuleDto?> UpdateAsync(Guid userId, Guid id, UpsertRecurringRuleRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var existing = await _gateway.GetByIdAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Title = request.Title.Trim();
        existing.Amount = Math.Abs(request.Amount);
        existing.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
        existing.Note = request.Note?.Trim();
        existing.RuleKind = request.RuleKind;
        existing.Frequency = request.Frequency;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _gateway.UpdateAsync(existing, cancellationToken);
        return Map(existing);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var existing = await _gateway.GetByIdAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _gateway.DeleteAsync(existing, cancellationToken);
        return true;
    }

    private static void Validate(UpsertRecurringRuleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title er påkrævet.");
        }

        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            throw new InvalidOperationException("EndDate kan ikke være før StartDate.");
        }
    }

    private static RecurringRuleDto Map(RecurringRule rule) =>
        new(
            rule.Id,
            rule.Title,
            rule.Amount,
            rule.Category,
            rule.Note,
            rule.RuleKind,
            rule.Frequency,
            rule.StartDate,
            rule.EndDate,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt
        );
}
