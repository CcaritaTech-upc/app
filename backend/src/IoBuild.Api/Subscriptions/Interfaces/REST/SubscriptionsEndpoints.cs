using IoBuild.Api.CoreBusiness;
using IoBuild.Api.Persistence;
using IoBuild.Api.Subscriptions.Application.Internal.CommandServices;
using IoBuild.Api.Subscriptions.Domain.Services;
using IoBuild.Api.Subscriptions.Domain.Services.Commands;
using IoBuild.Api.Subscriptions.Domain.Services.Queries;
using IoBuild.Api.Subscriptions.Infrastructure.Stripe;
using IoBuild.Api.Subscriptions.Interfaces.REST.Resources;
using IoBuild.Api.Subscriptions.Interfaces.REST.Transform;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Api.Subscriptions.Interfaces.REST;

public static class SubscriptionsEndpoints
{
    public static void MapSubscriptionsEndpoints(this WebApplication app)
    {
        // ── Plans Endpoints ──
        var plans = app.MapGroup("/api/v1/plans").WithTags("Plans");

        plans.MapGet("", async (IPlanQueryService queryService, CancellationToken ct) =>
        {
            var planList = await queryService.Handle(new GetAllPlansQuery(), ct);
            return Results.Ok(planList.Select(PlanResourceFromEntityAssembler.ToResourceFromEntity));
        }).AllowAnonymous();

        plans.MapGet("/{id:int}", async (int id, IPlanQueryService queryService, CancellationToken ct) =>
        {
            var plan = await queryService.Handle(new GetPlanByIdQuery(id), ct);
            return plan is null ? Results.NotFound() : Results.Ok(PlanResourceFromEntityAssembler.ToResourceFromEntity(plan));
        }).AllowAnonymous();

        plans.MapPost("", async (CreatePlanResource resource, IPlanCommandService commandService, IPlanQueryService queryService, CancellationToken ct) =>
        {
            var command = new CreatePlanCommand(resource.Name, resource.Description, resource.Price, resource.Interval, resource.FeaturesJson);
            var planId = await commandService.Handle(command, ct);
            var created = await queryService.Handle(new GetPlanByIdQuery(planId), ct);
            return created is null ? Results.Problem(statusCode: 500) : Results.Created($"/api/v1/plans/{planId}", PlanResourceFromEntityAssembler.ToResourceFromEntity(created));
        }).RequireAuthorization();

        plans.MapPut("/{id:int}", async (int id, UpdatePlanResource resource, IPlanCommandService commandService, CancellationToken ct) =>
        {
            try
            {
                var command = new UpdatePlanCommand(id, resource.Name, resource.Description, resource.Price, resource.Interval, resource.FeaturesJson);
                await commandService.Handle(command, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization();

        // ── Subscriptions Endpoints ──
        var subs = app.MapGroup("/api/v1").WithTags("Subscriptions");

        subs.MapGet("/subscriptions", async (IoBuildDbContext db, CancellationToken ct) =>
        {
            var subscriptions = await db.Subscriptions.ToListAsync(ct);
            var plans = await db.Plans.ToDictionaryAsync(p => p.Id, ct);
            var result = subscriptions.Select(s => new
            {
                s.Id,
                s.BuilderId,
                s.PlanId,
                s.Status,
                s.StartDate,
                s.EndDate,
                Plan = plans.TryGetValue(s.PlanId, out var plan) ? new
                {
                    plan.Id,
                    plan.Name,
                    plan.Price,
                    plan.Description,
                    Features = !string.IsNullOrWhiteSpace(plan.FeaturesJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(plan.FeaturesJson)
                        : new List<string>()
                } : null
            });
            return Results.Ok(result);
        });
        subs.MapGet("/subscriptions/{id:int}", async (int id, IoBuildDbContext db, CancellationToken ct) =>
        {
            var s = await db.Subscriptions.FindAsync([id], ct);
            if (s is null) return Results.NotFound();
            var plan = await db.Plans.FindAsync([s.PlanId], ct);
            return Results.Ok(new
            {
                s.Id,
                s.BuilderId,
                s.PlanId,
                s.Status,
                s.StartDate,
                s.EndDate,
                Plan = plan is not null ? new
                {
                    plan.Id,
                    plan.Name,
                    plan.Price,
                    plan.Description,
                    Features = !string.IsNullOrWhiteSpace(plan.FeaturesJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(plan.FeaturesJson)
                        : new List<string>()
                } : null
            });
        });
        subs.MapPut("/subscriptions/{id:int}", async (int id, CreateSubscriptionRequest request, IoBuildDbContext db, CancellationToken ct) => { var item = await db.Subscriptions.FindAsync([id], ct); if (item is null) return Results.NotFound(); item.PlanId = request.PlanId; item.EndDate = request.EndDate; await db.SaveChangesAsync(ct); return Results.NoContent(); });
        subs.MapPost("/subscriptions/{id:int}/cancel", async (int id, IoBuildDbContext db, CancellationToken ct) => { var item = await db.Subscriptions.FindAsync([id], ct); if (item is null) return Results.NotFound(); item.Status = "cancelled"; await db.SaveChangesAsync(ct); return Results.NoContent(); });
        subs.MapPost("/subscriptions/payments/sessions", async (PaymentCheckoutRequest request, IConfiguration configuration, IPaymentProvider provider, CancellationToken ct) =>
        {
            var restrictedKey = StripeRestrictedKeyResolver.Resolve(configuration);
            if (restrictedKey is null) return Results.Problem(statusCode: 503);
            var options = StripeIntegrationOptions.Create(restrictedKey);
            var session = await provider.CreateCheckoutSessionAsync(request, options, ct);
            return session is null ? Results.Problem(statusCode: 503) : Results.Created($"/api/v1/subscriptions/payments/sessions/{session.Id}", new
            {
                session.Id,
                session.Url,
                sessionId = session.Id,
                checkoutUrl = session.Url,
                session.AmountInCents,
                options.UsesDynamicPaymentMethods
            });
        });
        subs.MapPatch("/subscriptions/payments/sessions/{sessionId}", async (string sessionId, IPaymentProvider provider, IoBuildDbContext db, CancellationToken ct) =>
        {
            var confirmation = await provider.ConfirmSessionAsync(sessionId, ct);
            if (confirmation is null) return Results.Problem(statusCode: 503);

            var existing = await db.Subscriptions.FirstOrDefaultAsync(s => s.BuilderId == confirmation.BuilderId && s.PlanId == confirmation.PlanId && s.Status == "active", ct);
            if (existing is null)
            {
                db.Subscriptions.Add(new Subscription
                {
                    BuilderId = confirmation.BuilderId,
                    PlanId = confirmation.PlanId,
                    Status = "active",
                    StartDate = DateTime.UtcNow
                });
                await db.SaveChangesAsync(ct);
            }

            return Results.Ok(confirmation);
        });
        subs.MapGet("/subscriptions/payments/invoices", async (int builderId, IPaymentProvider provider, CancellationToken ct) =>
        {
            var invoices = await provider.GetInvoicesAsync(builderId, ct);
            return invoices is null ? Results.Problem(statusCode: 503) : Results.Ok(invoices);
        });
        subs.MapPost("/subscriptions", async (CreateSubscriptionRequest request, IoBuildDbContext db, CancellationToken ct) =>
        {
            var subscription = new Subscription { BuilderId = request.BuilderId, PlanId = request.PlanId, StartDate = request.StartDate, EndDate = request.EndDate };
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/subscriptions/{subscription.Id}", subscription);
        });
        subs.MapPost("/webhooks/stripe", async (HttpRequest request, StripeWebhookProcessor processor, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var payload = await reader.ReadToEndAsync(ct);
            using var document = System.Text.Json.JsonDocument.Parse(payload);
            var eventId = document.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
            var eventType = document.RootElement.TryGetProperty("type", out var type) ? type.GetString() : null;
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventType)) return Results.BadRequest();
            var signature = request.Headers["Stripe-Signature"].ToString();
            return await processor.ProcessAsync(new StripeWebhookRequest(eventId, eventType, payload, signature), ct)
                ? Results.Ok(new { received = true, eventId })
                : Results.Unauthorized();
        }).AllowAnonymous();
    }
}
