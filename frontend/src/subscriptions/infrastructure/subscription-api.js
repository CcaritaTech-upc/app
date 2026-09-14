import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BaseEndpoint } from "../../shared/infrastructure/base-endpoint.js";
import { APP_URL } from "../../shared/infrastructure/constants.js";

const subscriptionsEndpointPath = import.meta.env.VITE_SUBSCRIPTIONS_ENDPOINT_PATH;

export class SubscriptionApi extends BaseApi {
    #subscriptionsEndpoint;

    constructor() {
        super();
        this.#subscriptionsEndpoint = new BaseEndpoint(this, subscriptionsEndpointPath);
    }

    async getSubscriptionByBuilderId(builderId) {
        const response = await this.#subscriptionsEndpoint.getAll();
        const allSubscriptions = Array.isArray(response.data) ? response.data : [];
        // A builder can accumulate more than one subscription row over time
        // (e.g. cancel + renew); take the most recently started one for this builder.
        const builderSubs = allSubscriptions.filter(s => Number(s.builderId) === Number(builderId));
        builderSubs.sort((a, b) => new Date(b.startDate || 0) - new Date(a.startDate || 0) || (b.id - a.id));
        const activeSub = builderSubs.find(s => String(s.status).toLowerCase() === 'active');
        const mostRecent = activeSub || builderSubs[0];
        return { data: mostRecent || null };
    }

    getSubscriptionById(id) {
        return this.#subscriptionsEndpoint.getById(id);
    }

    createSubscription(resource) {
        return this.#subscriptionsEndpoint.create(resource);
    }

    updateSubscription(resource) {
        return this.#subscriptionsEndpoint.update(resource.id, resource);
    }

    cancelSubscription(subscriptionId) {
        return this.http.post(`${subscriptionsEndpointPath}/${subscriptionId}/cancel`);
    }

    createCheckoutSession(builderId, planId) {
        // El backend espera las URLs de success y cancel
        // Stripe agrega automáticamente el session_id como query parameter
        const baseUrl = (typeof window !== 'undefined' && window.location?.origin) ? window.location.origin : APP_URL;
        const successUrl = `${baseUrl}/subscriptions/my-subscription?success=true`;
        const cancelUrl = `${baseUrl}/subscriptions/my-subscription?canceled=true`;

        return this.http.post(`${subscriptionsEndpointPath}/payments/sessions`, {
            builderId,
            planId,
            successUrl,
            cancelUrl
        });
    }

    confirmPayment(builderId, sessionId) {
        return this.http.patch(`${subscriptionsEndpointPath}/payments/sessions/${sessionId}`, {
            builderId,
            status: 'confirmed'
        });
    }

    getInvoicesByBuilder(builderId) {
        return this.http.get(`${subscriptionsEndpointPath}/payments/invoices`, { params: { builderId } });
    }
}
